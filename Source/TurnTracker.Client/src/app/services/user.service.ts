import { Injectable } from '@angular/core';
import { IUser } from '../auth/models/IUser';
import { ObservableUser } from '../auth/models/ObservableUser';
import { UserPropertyChange } from '../auth/models/UserPropertyChange';
import { of } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private readonly _userKey = 'user';

  private _currentUser: ObservableUser;
  public get currentUser(): IUser {
    return this._currentUser;
  }
  public set currentUser(user: IUser) {
    if (this._currentUser) {
      this._currentUser.complete();
    }

    if (user) {
      this._currentUser = new ObservableUser(user);
      this._currentUser.propertyChanged.subscribe(change => this._persistUser(change));
    } else {
      this._currentUser = null;
    }
    this._persistUser(new UserPropertyChange(this._currentUser));
  }

  constructor(private _http: HttpClient) {
    const saved = localStorage.getItem(this._userKey);
    if (saved) {
      const user = JSON.parse(saved);
      this._currentUser = new ObservableUser(user);
      this._currentUser.propertyChanged.subscribe(change => this._persistUser(change));
    }
  }

  private _persistUser(change: UserPropertyChange) {
    if (change.user) {
        (change.propertyName === 'showDisabledActivities' ? this._saveShowDisabledActivities() : of(true)).subscribe(
          () => {
            try {
              const user = <IUser>{
                displayName: this._currentUser.displayName,
                id: this._currentUser.id,
                role: this._currentUser.role,
                showDisabledActivities: this._currentUser.showDisabledActivities,
                username: this._currentUser.username
              };
              const json = JSON.stringify(user);
              localStorage.setItem(this._userKey, json);
            } catch (error) {
              console.error('Failed to persist user', error);
            }
          }
        );
    } else {
      localStorage.removeItem(this._userKey);
    }
  }

  private _saveShowDisabledActivities() {
    return this._http.put(`settings/ShowDisabledActivities`, this._currentUser.showDisabledActivities);
  }
}
