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
    // const options = <PublicKeyCredentialCreationOptions>{
    //   challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
    //   rp: {
    //     name: environment.appName,
    //     id: window.location.hostname,
    //   },
    //   user: {
    //     id: Uint8Array.from(user.id.toString(), c => c.charCodeAt(0)),
    //     name: user.username,
    //     displayName: user.displayName,
    //   },
    //   pubKeyCredParams: [{ alg: -7, type: 'public-key' }],
    //   authenticatorSelection: {
    //     authenticatorAttachment: 'platform',
    //   },
    //   timeout: this.timeout,
    //   attestation: 'direct'
    // };

    // return navigator.credentials.create({
    //   publicKey: options
    // });
  }

  private getCredentialsById(credentialId: string, challenge: string) {
    // const options = <PublicKeyCredentialRequestOptions>{
    //   challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
    //   allowCredentials: [{
    //     id: Uint8Array.from(credentialId, c => c.charCodeAt(0)),
    //     type: 'public-key',
    //   }],
    //   timeout: this.timeout,
    // };

    // return navigator.credentials.get({
    //   publicKey: options
    // });
  }

  private getCredentials(challenge: string) {
    // const options = <PublicKeyCredentialRequestOptions>{
    //   challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
    //   timeout: this.timeout,
    // };

    // return navigator.credentials.get({
    //   publicKey: options
    // });
  }

  public async test(user: IUser) {
    const newCreds = await this.createCredentials(user, 'first');
    console.log('created', newCreds);

    const matchingCred = await this.getCredentialsById('id', 'second');
    console.log('matching', matchingCred);

    const anyCred = await this.getCredentials('third');
    console.log('any', anyCred);
  }
}
