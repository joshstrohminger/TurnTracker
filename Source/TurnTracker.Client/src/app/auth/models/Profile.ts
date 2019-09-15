import { Role } from './Role';

export class Profile {
  public id: number;
  public username: string;
  public displayName: string;
  public role: Role;
  public mobileNumber: string;
  public email: string;
  public multiFactorEnabled: boolean;
}
