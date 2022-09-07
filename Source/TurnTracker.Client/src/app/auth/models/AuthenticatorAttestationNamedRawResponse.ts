import { AuthenticatorAttestationRawResponse } from "./AuthenticatorAttestationRawResponse";

export interface AuthenticatorAttestationNamedRawResponse {
  rawResponse: AuthenticatorAttestationRawResponse;
  deviceName: string;
}
