import { Role } from './Role';

export class Profile {
  public id: number;
  public username: string;
  public displayName: string;
  public role: Role;
  public mobileNumber: string;
  public mobileNumberVerified: boolean;
  public email: string;
  public emailVerified: boolean;
}
