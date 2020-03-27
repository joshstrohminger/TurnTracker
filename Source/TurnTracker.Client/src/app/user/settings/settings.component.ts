import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { IUser } from 'src/app/auth/models/IUser';
import { PushService } from 'src/app/services/push.service';
import { HttpClient } from '@angular/common/http';
import { MessageService } from 'src/app/services/message.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { takeUntil, debounceTime, filter } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { ImmediateErrorStateMatcher } from 'src/app/validators/ImmediateErrorStateMatcher';
import { WebauthnService } from 'src/app/auth/webauthn.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit, OnDestroy {
  me: IUser;
  settingsForm: FormGroup;
  private unsubscribe$ = new Subject<void>();
  readonly immediateErrors = new ImmediateErrorStateMatcher();

  constructor(
    private _userService: UserService,
    public pushService: PushService,
    private _http: HttpClient,
    private _messageSevice: MessageService,
    private _builder: FormBuilder
    ) { }

  ngOnInit() {
    this.me = this._userService.currentUser;
    this.settingsForm = this._builder.group({
      push: [this.me.enablePushNotifications],
      snoozeHours: [this.me.snoozeHours, Validators.compose([Validators.required, Validators.min(1), Validators.max(255)])]
    });
    this.settingsForm.controls.push.valueChanges
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe(x => this.me.enablePushNotifications = x);
    this.settingsForm.controls.snoozeHours.valueChanges
      .pipe(takeUntil(this.unsubscribe$), filter(x => this.settingsForm.controls.snoozeHours.valid), debounceTime(100))
      .subscribe(x => this.me.snoozeHours = x);
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
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
