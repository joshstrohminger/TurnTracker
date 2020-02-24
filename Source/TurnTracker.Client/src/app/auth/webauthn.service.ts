import { Injectable } from '@angular/core';
import { IUser } from './models/IUser';
import { environment } from '../../environments/environment';
import { BehaviorSubject, from } from 'rxjs';
import { skip, share } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { MessageService } from '../services/message.service';
import * as Encodr from 'encodr';

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
    const creds = await this.createCredentials(user, new Date().toString());
    try {
      const response = creds.response as AuthenticatorAttestationResponse;
      const utf8Decoder = new TextDecoder('utf-8');
      const decodedClientData = utf8Decoder.decode(response.clientDataJSON);
      const clientDataObj = JSON.parse(decodedClientData);
      const CBOR = new Encodr('cbor');
      const decodedAttestationObject = CBOR.decode(response.attestationObject);
      const authData = decodedAttestationObject.authData;

      // get the length of the credential ID
      const dataView = new DataView(
          new ArrayBuffer(2));
      const idLenBytes = authData.slice(53, 55);
      idLenBytes.forEach(
          (value, index) => dataView.setUint8(
              index, value));
      const credentialIdLength = dataView.getUint16(0);

      // get the credential ID
      const credentialId = authData.slice(
          55, 55 + credentialIdLength);

      // get the public key object
      const publicKeyBytes = authData.slice(
          55 + credentialIdLength);

      // the publicKeyBytes are encoded again as CBOR
      const publicKeyObject = CBOR.decode(
          publicKeyBytes.buffer);
      const c = {
        id: creds.id,
        data: clientDataObj,
        credId: credentialId,
        pubKey: publicKeyObject
      };
      console.log('creds', c);
      this.lastResult = JSON.stringify(c);
      this._http.post('auth/registerDevice', c).subscribe(() => {
        this._messageService.success('finished sending registration');
      });
    } catch (e) {
      console.error('failed to decode', e);
      this._messageService.error('failed to decode', e);
      this.lastResult = e;
      return;
    }
  }

  private convertCredential(credential: Credential, source: string): PublicKeyCredential {
    if (credential.type === this.publicKeyType) {
      return credential as PublicKeyCredential;
    }
    const message = `Invalid credential type '${credential.type}' from call to '${source}'`;
    this._messageService.error(message);
    throw new Error(message);
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
