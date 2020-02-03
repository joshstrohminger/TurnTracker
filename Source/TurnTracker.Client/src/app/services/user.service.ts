import { Injectable } from '@angular/core';
import { IUser } from '../auth/models/IUser';
import { ObservableUser } from '../auth/models/ObservableUser';
import { UserPropertyChange } from '../auth/models/UserPropertyChange';
import { of, Observable, BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { MessageService } from './message.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private readonly _userKey = 'user';

  private readonly _userSubject = new BehaviorSubject<ObservableUser>(null);
  public get currentUser$() {
    return this._userSubject.asObservable();
  }

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
      this._currentUser.propertyChanged$.subscribe(change => this._persistUser(change));
    } else {
      this._currentUser = null;
    }
    console.log('next user', this._currentUser);
    this._userSubject.next(this._currentUser);
    this._persistUser(new UserPropertyChange(this._currentUser));
  }

  constructor(private _http: HttpClient, private _messageService: MessageService) {
    const saved = localStorage.getItem(this._userKey);
    if (saved) {
      const user = JSON.parse(saved);
      this.currentUser = user;
    } else {
      this.currentUser = null;
    }
  }

  private _saveChange(propertyName: string): Observable<Object> {
    switch (propertyName) {
      case 'showDisabledActivities':
        return this._saveShowDisabledActivities();
      case 'enablePushNotifications':
        return this._saveEnablePushNotifications();
      default:
        // other properties are saved through other means, but we still need to update the current user
        return of(true);
    }
  }

  private _persistUser(change: UserPropertyChange) {
    if (change.user) {
        this._saveChange(change.propertyName).subscribe(
          () => {
            try {
              const user = <IUser>{
                displayName: this._currentUser.displayName,
                id: this._currentUser.id,
                role: this._currentUser.role,
                showDisabledActivities: this._currentUser.showDisabledActivities,
                enablePushNotifications: this._currentUser.enablePushNotifications,
                username: this._currentUser.username
              };
              const json = JSON.stringify(user);
              localStorage.setItem(this._userKey, json);
            } catch (error) {
              console.error('Failed to persist user', error);
            }
          },
          error => {
            this._messageService.error('Failed to persist user setting', error);
          }
        );
    } else {
      localStorage.removeItem(this._userKey);
    }
  }

  private _saveShowDisabledActivities(): Observable<Object> {
    return this._http.put(`settings/ShowDisabledActivities`, this._currentUser.showDisabledActivities);
  }

  private _saveEnablePushNotifications(): Observable<Object> {
    return this._http.put(`settings/EnablePushNotifications`, this._currentUser.enablePushNotifications);
  }
}
