import { Injectable } from '@angular/core';
import { IUser } from './models/IUser';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WebauthnService {

  private readonly timeout = 60000;

  constructor() { }

  private createCredentials(user: IUser, challenge: string) {
    const options = <PublicKeyCredentialCreationOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      rp: {
        name: environment.appName,
        // id: window.location.host,
      },
      user: {
        id: Uint8Array.from(user.id.toString(), c => c.charCodeAt(0)),
        name: user.username,
        displayName: user.displayName,
      },
      pubKeyCredParams: [{ alg: -7, type: 'public-key' }],
      authenticatorSelection: {
        //authenticatorAttachment: 'platform',
        userVerification: 'required'
        //userVerification: 'discouraged'
      },
      timeout: 60000,
      attestation: 'direct'
    };

    console.log('creating creds', options);

    return navigator.credentials.create({
      publicKey: options
    });
  }

  private getCredentialsById(credentialId: string, challenge: string) {
    console.log('geting creds by id', credentialId);

    const options = <PublicKeyCredentialRequestOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      allowCredentials: [{
        id: Uint8Array.from(credentialId, c => c.charCodeAt(0)),
        type: 'public-key',
      }],
      timeout: 60000,
    };

    return navigator.credentials.get({
      publicKey: options
    });
  }

  private getCredentials(challenge: string) {
    console.log('getting any');
    const options = <PublicKeyCredentialRequestOptions>{
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      timeout: this.timeout,
    };

    return navigator.credentials.get({
      publicKey: options
    });
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
