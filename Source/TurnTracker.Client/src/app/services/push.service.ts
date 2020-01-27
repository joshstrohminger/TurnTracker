import { Injectable } from '@angular/core';
import { MessageService } from './message.service';
import { UserService } from './user.service';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { IUser } from '../auth/models/IUser';
import { combineLatest, Subject, from, EMPTY, of } from 'rxjs';
import { mergeMap, filter, startWith, switchMap } from 'rxjs/operators';
import { UserPropertyChange } from '../auth/models/UserPropertyChange';

@Injectable({
  providedIn: 'root'
})
export class PushService {
  private _started = false;
  private _user: IUser;
  private _recheck = new Subject<void>();

  private _sub: PushSubscription;
  public get PushEnabled() {
    return !!this._sub;
  }

  constructor(
    private _swPush: SwPush,
    private _http: HttpClient,
    private _userService: UserService,
    private _messageService: MessageService
  ) {
    console.log('push constructed');
  }

  public start() {
    if (this._started) {
      return;
    }
    this._started = true;
    console.log('push started', this._swPush.isEnabled);

    combineLatest([
      this._swPush.subscription,
      this._userService.currentUser$.pipe(
        switchMap(user => (user
          ? user.propertyChanged$.pipe(filter(change => change.propertyName === 'enablePushNotifications'))
          : EMPTY)
          .pipe(startWith(new UserPropertyChange(user, 'user')))
      )),
      this._http.get('notification/push/publickey', {responseType: 'text'}),
      this._recheck.asObservable()])
    .subscribe(([subscription, userChange, serverPublicKey]) => {
      console.log('push updated', subscription, userChange, serverPublicKey);
      this._sub = subscription;
      this._user = userChange.user;
      if (this._user) {
        if (this._user.enablePushNotifications && !this._sub) {
          console.log('requesting push permission');
          from(this._swPush.requestSubscription({serverPublicKey})).subscribe(
            requestedSubscription => console.log('sub', JSON.stringify(requestedSubscription, null, 2)),
            error => this._messageService.error('Failed to request push notification permission', error)
          );
        } else if (!this._user.enablePushNotifications && this._sub) {
          console.log('cancelling push permission');
          from(this._swPush.unsubscribe()).subscribe(
            () => {},
            error => this._messageService.error('Failed to cancel push notifications', error)
          );
        } else {
          console.log('no change');
        }
      }
    });
  }

  public refresh() {
    this._recheck.next();
  }
}
