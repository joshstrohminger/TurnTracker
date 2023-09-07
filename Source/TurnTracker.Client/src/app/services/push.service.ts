import { Injectable } from '@angular/core';
import { MessageService } from './message.service';
import { UserService } from './user.service';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { IUser } from '../auth/models/IUser';
import { combineLatest, from, EMPTY, of, Observable } from 'rxjs';
import { filter, startWith, switchMap, map } from 'rxjs/operators';
import { UserPropertyChange } from '../auth/models/UserPropertyChange';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PushService {
  private _user: IUser;

  private _sub: PushSubscription;
  public get PushEnabled() {
    return !!this._sub;
  }
  public get Sub() {
    return this._sub;
  }

  private blocked = false;
  public get isBlocked(): boolean {
    return this.blocked;
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

    this._swPush.notificationClicks.subscribe(
      ({action, notification}) => {
          this.handleNotificationClick(action, notification);
      });

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
    }

    if (this._user.enablePushNotifications && !this.blocked) {
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
    let sub$: Observable<{request: boolean, sub: PushSubscription}>;
    if (this._sub) {
      console.log('saving push subscription');
      sub$ = of({request: false, sub: this._sub});
    } else {
      console.log('requesting push permission');
      sub$ = from(this._swPush.requestSubscription({ serverPublicKey })).pipe(map(sub => {return {request: true, sub};}));
    }

    sub$.pipe(
      switchMap(x => {
        if (x.request) {
          console.log('successfully requested permission, let the SwPush.subscription trigger the server update');
          return of(x.request);
        } else {
          console.log('sending subscription to server', x);
          return this._http.post('notification/push/subscribe', x.sub.toJSON()).pipe(map(() => x.request));
        }
    })
    ).subscribe(
      (request) => {
        this.blocked = false;
        if (request) {
          this._messageService.success('Subscribed to push notifications on this device');
        }
      },
      error => {
        let errorMessage = 'Failed to subscribe to push notifications on this device';
        if (error instanceof DOMException) {
          console.log(`DOMException Name: ${error.name}, Message: ${error.message}`);
          if (error.name === 'NotAllowedError') {
            this.blocked = true;
            return;
          } else {
            errorMessage += `: ${error.message}`;
          }
        } else if(error.error) {
          errorMessage += `: ${error.error}`;
        }
        this._messageService.error(errorMessage, error);
      });
  }

  private handleNotificationClick(action: string, notification: NotificationOptions & {
    title: string;
}) {
    console.log('action clicked', action, notification);
    let url = window.location.origin;
    switch (action) {
      case 'about':
        url += '/about';
        break;
    }
    console.log(`not opening url: ${url}`);
  }
}
