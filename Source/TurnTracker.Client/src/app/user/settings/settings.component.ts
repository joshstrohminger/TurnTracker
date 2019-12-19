import { Component, OnInit } from '@angular/core';
import { SwPush } from '@angular/service-worker';
import { HttpClient } from '@angular/common/http';
import { UserService } from 'src/app/services/user.service';
import { IUser } from 'src/app/auth/models/IUser';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  sub: PushSubscription;
  me: IUser;

  constructor(private _swPush: SwPush, private _http: HttpClient, private _userService: UserService) { }

  ngOnInit() {
    this.me = this._userService.currentUser;

    if (this.me.enablePushNotifications) {
      this._http.get('notification/push/publickey', {responseType: 'text'}).subscribe(
        publicKey => console.log('public key', publicKey),
        error => console.error('failed to get public key', error)
      );
    }
  }
}
