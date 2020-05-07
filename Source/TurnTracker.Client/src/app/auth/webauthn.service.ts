import { Injectable } from '@angular/core';
import { from, Observable, EMPTY, of, throwError } from 'rxjs';
import { share, map, flatMap, tap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { MessageService } from '../services/message.service';
import { AnonymousPublicKeyCredentialRequestOptions } from './anonymousPublicKeyCredentialRequestOptions';
import { AuthService } from './auth.service';
import { AuthenticatedUser } from './models/AuthenticatedUser';

@Injectable({
  providedIn: 'root'
})
export class WebauthnService {
  private readonly timeout = 60000;
  private readonly publicKeyType = 'public-key';
  private readonly usernameRequiredKey = 'username-required';

  private _isAvailable$ = from(PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()).pipe(share());
  public get isAvailable$() {
    return this._isAvailable$;
  }

  public get usernameRequired() {
    return !!localStorage.getItem(this.usernameRequiredKey);
  }

  public set usernameRequired(required: boolean) {
    if (required) {
      localStorage.setItem(this.usernameRequiredKey, 'required');
    } else {
      localStorage.removeItem(this.usernameRequiredKey);
    }
  }

  constructor(private _http: HttpClient, private _messageService: MessageService, private _authService: AuthService) {
  }

  private coerceToArrayBuffer(thing, name?: string): ArrayBuffer {
    if (typeof thing === 'string') {
        // base64url to base64
        thing = thing.replace(/-/g, '+').replace(/_/g, '/');

        // base64 to Uint8Array
        const str = window.atob(thing);
        const bytes = new Uint8Array(str.length);
        for (let i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }
        thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
        thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
        throw new TypeError(`could not coerce '${name}' '${thing}' to ArrayBuffer`);
    }

    return thing;
}

  private coerceToBase64Url(thing) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
        thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
        let str = '';
        const len = thing.byteLength;

        for (let i = 0; i < len; i++) {
            str += String.fromCharCode(thing[i]);
        }
        thing = window.btoa(str);
    }

    if (typeof thing !== 'string') {
        throw new Error('could not coerce to string');
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');

    return thing;
  }

  public registerDevice$(deviceName: string) {
    return this.createCredentials$().pipe(flatMap(creds => {
        console.log('created raw creds', creds);
        const response = creds.response as AuthenticatorAttestationResponse;
        const p = {
          id: creds.id,
          rawId: this.coerceToBase64Url(creds.rawId),
          type: creds.type,
          extensions: creds.getClientExtensionResults(),
          response: {
              clientDataJSON: this.coerceToBase64Url(response.clientDataJSON),
              attestationObject: this.coerceToBase64Url(response.attestationObject)
          },
          deviceName: deviceName
        };
        console.log('sending creds', p);

        return this._http.post('auth/CompleteDeviceRegistration', p);
    }),
    tap(
      () => this._messageService.success('Registered credentials'),
      error => this._messageService.error(`Failed to register credentials: ${error.ToString()}`, error)));
  }

  public assertDevice$(username: string): Observable<AuthenticatedUser> {
    if (this.usernameRequired && !username) {
      const message = 'This device requires a username to login';
      this._messageService.error(message);
      return throwError(new Error(message));
    }

    let requestId: string;
    let allowCredentials: PublicKeyCredentialDescriptor[];

    return this._http.post('auth/StartDeviceAssertion', username).pipe(
      flatMap((options: AnonymousPublicKeyCredentialRequestOptions) => {
        console.log('assertion options before mod', options);
        options.challenge = this.coerceToArrayBuffer(options.challenge);
        options.allowCredentials.forEach(listItem => listItem.id = this.coerceToArrayBuffer(listItem.id));
        console.log('assertion options after mod', options);
        requestId = options.requestId;
        allowCredentials = options.allowCredentials;
        return from(navigator.credentials.get({publicKey: options}));
      }),
      map(credential => this.convertCredential(credential, 'get')),
      flatMap(credential => {
        const response = credential.response as AuthenticatorAssertionResponse;
        console.log('assertion result before modding', credential);
        const r = {
            id: credential.id,
            rawId: this.coerceToBase64Url(credential.rawId),
            type: credential.type,
            extensions: credential.getClientExtensionResults(),
            response: {
                authenticatorData: this.coerceToBase64Url(response.authenticatorData),
                clientDataJson: this.coerceToBase64Url(response.clientDataJSON),
                signature: this.coerceToBase64Url(response.signature)
            },
            requestId: requestId
        };
        console.log('sending assertion', r);

        return this._http.post<AuthenticatedUser>('auth/CompleteDeviceAssertion', r);
      }),
      tap(user => {
        console.log('asserted device');
        this._authService.saveAuthenticatedUser(user);
      },
      error => {
        if (!allowCredentials.length && /empty.*allowCredentials.*not supported/i.test(error.message)) {
          // username is required for this device but one wasn't provided, require username in the future
          this.usernameRequired = true;
          this._messageService.error('This device requires a username to login', error);
        } else {
          this._messageService.error(`Device Assertion Failed: ${error.ToString()}`, error);
        }
      }));
  }

  private convertCredential(credential: Credential, source: string): PublicKeyCredential {
    if (credential.type === this.publicKeyType) {
      return credential as PublicKeyCredential;
    }
    const message = `Invalid credential type '${credential.type}' from call to '${source}'`;
    this._messageService.error(message);
    throw new Error(message);
  }

  private createCredentials$(): Observable<PublicKeyCredential> {
    return this._http.post('auth/StartDeviceRegistration', null).pipe(
      flatMap((options: PublicKeyCredentialCreationOptions) => {
        console.log('creating creds before coercion', options);
        // Turn the challenge back into the accepted format of padded base64
        options.challenge = this.coerceToArrayBuffer(options.challenge, 'challenge');
        // Turn ID into a UInt8Array Buffer for some reason
        options.user.id = this.coerceToArrayBuffer(options.user.id, 'user id');

        options.excludeCredentials = options.excludeCredentials.map((c) => {
          c.id = this.coerceToArrayBuffer(c.id, 'exclude id');
          return c;
        });

        if (options.authenticatorSelection.authenticatorAttachment === null) {
          options.authenticatorSelection.authenticatorAttachment = undefined;
        }
        console.log('creating creds after coercion', options);

        return from(navigator.credentials.create({ publicKey: options }));
      }),
      map((credential: Credential) => this.convertCredential(credential, 'create')));
  }

  private async getCredentialsById(credentialId: string, challenge: string): Promise<PublicKeyCredential> {
    console.log('geting creds by id', credentialId);

    const options = <PublicKeyCredentialRequestOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      allowCredentials: [{
        id: Uint8Array.from(credentialId, c => c.charCodeAt(0)),
        type: this.publicKeyType,
      }],
      timeout: this.timeout,
    };

    const credential = await navigator.credentials.get({
      publicKey: options
    });

    return this.convertCredential(credential, 'getId');
  }

  private async getCredentials(challenge: string): Promise<PublicKeyCredential> {
    console.log('getting any');
    const options = <PublicKeyCredentialRequestOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      timeout: this.timeout,
    };

    const credential = await navigator.credentials.get({
      publicKey: options
    });

    return this.convertCredential(credential, 'get');
  }

  public async test() {
    console.log('checking availablility');
    const available = await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
    console.log('available', available);

    if (!available) { return; }

    this.createCredentials$()
    .subscribe(async newCreds => {
      console.log('created', newCreds);

      const matchingCred = await this.getCredentialsById(newCreds.id, new Date().toString());
      console.log('matching', matchingCred);

      const anyCred = await this.getCredentials(new Date().toString());
      console.log('any', anyCred);
      });
  }
}
