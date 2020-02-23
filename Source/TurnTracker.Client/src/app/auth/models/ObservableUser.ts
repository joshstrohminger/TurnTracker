import { Role } from './Role';
import { IUser } from './IUser';
import { Subject, Observable } from 'rxjs';
import { UserPropertyChange } from './UserPropertyChange';

export class ObservableUser implements IUser {
  private _id: number;
  public get id() {
    return this._id;
  }

  private _username: string;
  public get username() {
    return this._username;
  }

  private _displayName: string;
  public get displayName() {
    return this._displayName;
  }
  public set displayName(displayName: string) {
    if (this._displayName !== displayName) {
      this._displayName = displayName;
      this._changed('displayName');
    }
  }

  private _role: Role;
  public get role() {
    return this._role;
  }

  private _showDisabledActivities: boolean;
  public get showDisabledActivities() {
    return this._showDisabledActivities;
  }
  public set showDisabledActivities(showDisabledActivities: boolean) {
    if (this._showDisabledActivities !== showDisabledActivities) {
      this._showDisabledActivities = showDisabledActivities;
      this._changed('showDisabledActivities');
    }
  }

  private _enablePushNotifications: boolean;
  public get enablePushNotifications() {
    return this._enablePushNotifications;
  }
  public set enablePushNotifications(enablePushNotifications: boolean) {
    if (this._enablePushNotifications !== enablePushNotifications) {
      this._enablePushNotifications = enablePushNotifications;
      this._changed('enablePushNotifications');
    }
  }

  private _snoozeHours: number;
  public get snoozeHours() {
    return this._snoozeHours;
  }
  public set snoozeHours(snoozeHours: number) {
    if (this._snoozeHours !== snoozeHours) {
      this._snoozeHours = snoozeHours;
      this._changed('snoozeHours');
    }
  }

  private _propertyChanged = new Subject<UserPropertyChange>();
  public get propertyChanged$(): Observable<UserPropertyChange> {
    return this._propertyChanged.asObservable();
  }

  public complete() {
    this._propertyChanged.complete();
  }

  constructor(user: IUser) {
    this._id = user.id;
    this._username = user.username;
    this._displayName = user.displayName;
    this._role = user.role;
    this._showDisabledActivities = user.showDisabledActivities;
    this._enablePushNotifications = user.enablePushNotifications;
    this._snoozeHours = user.snoozeHours;
  }

  private _changed(propertyName: string) {
    this._propertyChanged.next(new UserPropertyChange(this, propertyName));
  }
}
