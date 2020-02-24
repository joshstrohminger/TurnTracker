import { Injectable } from '@angular/core';
import { IUser } from './models/IUser';
import { environment } from '../../environments/environment';
import { BehaviorSubject, from } from 'rxjs';
import { skip, share } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { MessageService } from '../services/message.service';

@Injectable({
  providedIn: 'root'
})
export class WebauthnService {
  private readonly timeout = 60000;
  private readonly publicKeyType = 'public-key';

  private _isAvailable$ = from(PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()).pipe(share());
  public get isAvailable$() {
    return this._isAvailable$;
  }

  public lastResult: string;

  constructor(private _http: HttpClient, private _messageService: MessageService) {
  }

  public async registerDevice(user: IUser) {
    const credentials = await this.createCredentials(user, new Date().toString());
    console.log('creds', credentials);
    this.lastResult = JSON.stringify(credentials);
    this._http.post('auth/registerDevice', credentials).subscribe(() => {
      this._messageService.success('finished sending registration');
    });
  }

  private convertCredential(credential: Credential, source: string): PublicKeyCredential {
    if (credential.type === this.publicKeyType) {
      return credential as PublicKeyCredential;
    }
    const message = `Invalid credential type '${credential.type}' from call to '${source}'`;
    this._messageService.error(message);
    throw message;
  }

  private async createCredentials(user: IUser, challenge: string): Promise<PublicKeyCredential> {
    const options = <PublicKeyCredentialCreationOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      rp: { name: environment.appName },
      user: {
        id: Uint8Array.from(user.id.toString(), c => c.charCodeAt(0)),
        name: user.username,
        displayName: user.displayName,
      },
      pubKeyCredParams: [{ alg: -7, type: this.publicKeyType }],
      authenticatorSelection: { userVerification: 'required' },
      timeout: this.timeout,
      attestation: 'direct'
    };

    console.log('creating creds', options);

    const credential = await navigator.credentials.create({
      publicKey: options
    });

    return this.convertCredential(credential, 'create');
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

  public async test(user: IUser) {
    console.log('checking availablility');
    const available = await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
    console.log('available', available);

    if (!available) { return; }

    const newCreds = await this.createCredentials(user, new Date().toString());
    console.log('created', newCreds);

    const matchingCred = await this.getCredentialsById(newCreds.id, new Date().toString());
    console.log('matching', matchingCred);

    const anyCred = await this.getCredentials(new Date().toString());
    console.log('any', anyCred);
  }
}
