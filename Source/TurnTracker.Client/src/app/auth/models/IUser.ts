import { Role } from './Role';

export interface IUser {
  id: number;
  username: string;
  displayName: string;
  role: Role;
  showDisabledActivities: boolean;
}
