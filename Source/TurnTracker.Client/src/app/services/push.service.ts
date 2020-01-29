import { Injectable } from '@angular/core';
import { MessageService } from './message.service';
import { UserService } from './user.service';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { IUser } from '../auth/models/IUser';
import { combineLatest, from, EMPTY, of, Observable } from 'rxjs';
import { filter, startWith, switchMap } from 'rxjs/operators';
import { UserPropertyChange } from '../auth/models/UserPropertyChange';

@Injectable({
  providedIn: 'root'
})
export class PushService {
  private _user: IUser;

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
    this.start();
  }

  private start() {
    console.log('push started', this._swPush.isEnabled);

    combineLatest([
      this._http.get('notification/push/publickey', {responseType: 'text'}),
      this._swPush.subscription,
      this._userService.currentUser$.pipe(
        switchMap(user => (user
          ? user.propertyChanged$.pipe(filter(change => change.propertyName === 'enablePushNotifications'))
          : EMPTY)
          .pipe(startWith(new UserPropertyChange(user, 'user')))
      ))])
    .subscribe(([serverPublicKey, subscription, userChange]) => {
      console.log('push updated', subscription, serverPublicKey, userChange);
      this._sub = subscription;
      this._user = userChange.user;
      this.checkSubscription(serverPublicKey);
    });
  }

  private checkSubscription(serverPublicKey: string) {
    if (!this._user) {
      return;
    } else if (this._user.enablePushNotifications) {
      this.requestPermission(serverPublicKey);
    } else if (!this._user.enablePushNotifications && this._sub) {
      this.cancelPermission(this._sub);
    } else {
      console.log('no change');
    }
  }

  private cancelPermission(sub: PushSubscription) {
    console.log('cancelling push permission');

    this._http.post('notification/push/unsubscribe', sub.toJSON()).pipe(
      switchMap(() => from(this._swPush.unsubscribe()))
    ).subscribe(
      () => this._messageService.success('Unsubscribed from push notifications on this device'),
      error => this._messageService.error('Failed to unsubscribe from push notifications on this device', error));
  }

  private requestPermission(serverPublicKey: string) {
    let sub$: Observable<PushSubscription>;
    let request = false;
    if (this._sub) {
      console.log('saving push subscription');
      sub$ = of(this._sub);
    } else {
      console.log('requesting push permission');
      request = true;
      sub$ = from(this._swPush.requestSubscription({ serverPublicKey }));
    }

    sub$.pipe(
      switchMap(sub => this._http.post('notification/push/subscribe', sub.toJSON()))
    ).subscribe(
      () => {
        if (request) {
          this._messageService.success('Subscribed to push notifications on this device');
        }
      },
      error => this._messageService.error('Failed to subscribe to push notifications on this device', error));
  }
}
