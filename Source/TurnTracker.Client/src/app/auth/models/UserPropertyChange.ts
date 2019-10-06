import { IUser } from './IUser';

export class UserPropertyChange {
  public user: IUser;
  public propertyName: string;

  constructor(user: IUser, propertyName?: string) {
    this.user = user;
    this.propertyName = propertyName;
  }
}
