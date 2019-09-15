import { Role } from './Role';

export class AuthenticatedUser {
  public id: number;
  public username: string;
  public displayName: string;
  public refreshToken: string;
  public accessToken: string;
  public role: Role;
}
