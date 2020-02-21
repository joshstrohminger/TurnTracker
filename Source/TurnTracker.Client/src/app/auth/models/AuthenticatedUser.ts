import { Role } from './Role';
import { IUser } from './IUser';

export class AuthenticatedUser implements IUser {
  public id: number;
  public username: string;
  public displayName: string;
  public refreshToken: string;
  public accessToken: string;
  public role: Role;
  public showDisabledActivities: boolean;
  public enablePushNotifications: boolean;
  public snoozeHours: number;
}
