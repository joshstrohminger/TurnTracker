export interface AuthenticatorAttestationRawResponse {
  id: string;
  rawId: string;
  type: string;
  extensions: AuthenticationExtensionsClientOutputs;
  response: {
    attestationObject: string;
    clientDataJSON: string;
  };
}
