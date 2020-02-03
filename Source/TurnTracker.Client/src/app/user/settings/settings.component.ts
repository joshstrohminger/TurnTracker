import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { IUser } from 'src/app/auth/models/IUser';
import { PushService } from 'src/app/services/push.service';
import { HttpClient } from '@angular/common/http';
import { MessageService } from 'src/app/services/message.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  me: IUser;

  constructor(
    private _userService: UserService,
    public pushService: PushService,
    private _http: HttpClient,
    private _messageSevice: MessageService
    ) { }

  ngOnInit() {
    this.me = this._userService.currentUser;
  }

  public sendTestMessageToThisDevice() {
    this._http.post('notification/push/test/one', this.pushService.Sub)
    .subscribe(
      () => this._messageSevice.success('Sent push notification'),
      error => this._messageSevice.error('Failed to send push notification', error)
    );
  }

  public sendTestMessageToAllDevices() {
    this._http.post('notification/push/test/all', null)
    .subscribe(
      () => this._messageSevice.success('Sent push notifications'),
      error => this._messageSevice.error('Failed to send push notifications', error)
    );
  }
}
