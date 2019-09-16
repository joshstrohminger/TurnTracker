export class TokenInfo {
  public token: string;
  public isValid: boolean;

  constructor(token: string, isValid: boolean) {
    this.token = token;
    this.isValid = isValid;
  }
}
