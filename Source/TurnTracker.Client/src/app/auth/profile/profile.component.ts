import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';
import { Profile } from '../models/Profile';
import { MessageService } from 'src/app/services/message.service';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { TurnTrackerValidators } from 'src/app/validators/TurnTrackerValidators';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ImmediateErrorStateMatcher } from 'src/app/validators/ImmediateErrorStateMatcher';
import { ParentErrorStateMatcher } from 'src/app/validators/ParentErrorStateMatcher';
import { PasswordChange } from '../models/PasswordChange';
import { UserService } from 'src/app/services/user.service';
import { Device } from '../models/Device';
import { DateTime } from 'luxon';
import { finalize } from 'rxjs/operators';
import { Session } from '../models/Session';
import { WebauthnService } from '../webauthn.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  registerForm: FormGroup<{deviceName: FormControl<string>}>;
  registering = false;
  deleting = false;
  user: Profile;
  displayNameControl: FormControl<string>;
  passwordForm: FormGroup<{oldPassword: FormControl<string>, newPassword: FormControl<string>, confirmationPassword: FormControl<string>}>;
  passwordStrengthColor = 'warn';
  passwordStrength = 0;
  readonly passwordDesiredLength = 30;
  readonly immediateErrors = new ImmediateErrorStateMatcher();
  readonly parentErrors = new ParentErrorStateMatcher('different');
  readonly passwordStrengthIcons = ['😡', '😠', '🙁', '😕', '😒', '😑', '😐', '🙂', '😃', '😄', '😁'];
  passwordStrengthIcon = this.passwordStrengthIcons[0];
  devices: Device[];

  public get roles() {
    return Role;
  }

  public get deviceAlreadyRegistered() {
    return this.devices && !!this.devices.find(d => d.current && d.id);
  }

  public anySessionsToDelete(device: Device) {
    return !!device.sessions.find(d => !d.current);
  }

  constructor(
    private _authService: AuthService,
    private _messageService: MessageService,
    private _formBuilder: FormBuilder,
    private _http: HttpClient,
    private _userService: UserService,
    public webauthnService: WebauthnService) { }

  ngOnInit() {
    this._authService.getProfile().subscribe(
      profile => {
        this.user = profile;
      },
      error => {
        this._messageService.error('Failed to get profile', error);
      });

      this.getDevices();
  }

  private getDevices() {
    this._http.get<Device[]>('session').subscribe(devices => {
      for (const device of devices) {
        device.createdDate = device.created ? DateTime.fromISO(device.created) : undefined;
        device.updatedDate = DateTime.fromISO(device.updated);
        for (const session of device.sessions) {
          session.createdDate = DateTime.fromISO(session.created);
          session.updatedDate = DateTime.fromISO(session.updated);
        }
      }
      this.devices = devices;
      this.registerForm = this._formBuilder.group({
        deviceName: ['', [Validators.required, TurnTrackerValidators.whitespace]]
      });
      this.recheckFormEnabled();
    }, () => this.recheckFormEnabled());
  }

  private recheckFormEnabled() {
    if (this.registerForm) {
      if (!this.registering && !this.deviceAlreadyRegistered) {
        this.registerForm.enable();
      } else {
        this.registerForm.disable();
      }
    }
  }

  registerDevice() {
    this.registering = true;
    this.recheckFormEnabled();

    this.webauthnService.registerDevice$(this.registerForm.value.deviceName)
    .subscribe(
      () => {
        this.registering = false;
        this.getDevices();
      }, () => {
        this.registering = false;
        this.recheckFormEnabled();
      });
  }

  editDisplayName() {
    this.displayNameControl = this._formBuilder.control(this.user.displayName, [
      TurnTrackerValidators.minTrimmedLength(3),
      TurnTrackerValidators.newValue(this.user.displayName)
    ]);
  }

  cancelEditingDisplayName() {
    this.displayNameControl = null;
  }

  getDisplayNameErrorMessage() {
    const minLengthError = this.displayNameControl.getError('mintrimmedlength');
    if (minLengthError) {
      return `Min Character Length: ${minLengthError.actual} of ${minLengthError.min}`;
    }
    if (this.displayNameControl.hasError('newvalue')) {
      return 'Enter new DisplayName';
    }
    return '';
  }

  saveDisplayName() {
    const displayName = (<string>this.displayNameControl.value).trim();
    this.displayNameControl.disable();
    this._http.put<Profile>('profile/displayname', displayName).subscribe(
      profile => {
        this._messageService.success('Saved DisplayName');
        this.user = profile;
        this.displayNameControl = null;
        this._userService.currentUser.displayName = this.user.displayName;
      },
      error => {
        this._messageService.error('Failed to save DisplayName', error);
        this.displayNameControl.enable();
      });
  }

  editPassword() {
    this.passwordForm = this._formBuilder.group({
      oldPassword: ['', [Validators.required, TurnTrackerValidators.whitespace]],
      newPassword: ['', [TurnTrackerValidators.minTrimmedLength(12)]],
      confirmationPassword: ['', [Validators.required]]
    }, { validators: TurnTrackerValidators.different('newPassword', 'confirmationPassword') });
    this.passwordForm.controls.newPassword.valueChanges.subscribe(value => {
      const length = value ? value.trim().length : 0;
      const scale = 100 / this.passwordDesiredLength;
      this.passwordStrength = Math.min(100, Math.max(0, scale * length));
      this.passwordStrengthIcon = this.passwordStrengthIcons[Math.floor(this.passwordStrength / 10)];
      this.passwordStrengthColor = this.passwordStrength >= 80 ? 'primary' : this.passwordStrength >= 50 ? 'accent' : 'warn';
    });
  }

  cancelEditingPassword() {
    this.passwordForm = null;
  }

  savePassword() {
    this.passwordForm.disable();
    const change = <PasswordChange>{
      oldPassword: this.passwordForm.value.oldPassword,
      newPassword: this.passwordForm.value.newPassword
    };
    this._http.post('auth/changepassword', change).subscribe(
      () => {
        this._messageService.success('Changed password');
        this.passwordForm = null;
      },
      (error: HttpErrorResponse) => {
        let message: string;
        switch (error.status) {
          case 400:
            message = 'Invalid new password';
            break;
          case 401:
            message = 'Invalid original password';
            break;
          default:
            message = 'Failed to change password';
            break;
        }

        this._messageService.error(message, error);
        this.passwordForm.enable();
      }
    );
  }

  deleteSession(device: Device, session: Session) {
    if (this.deleting || session.current) {
      return;
    }

    this.deleting = true;
    this._http.delete(`session/${session.id}`)
      .pipe(finalize(() => this.deleting = false))
      .subscribe(() => {
        this._messageService.success('Deleted session');
        const sessionsIndex = device.sessions.findIndex(s => s.id === session.id);
        if (sessionsIndex < 0) {
          console.error('Failed to find session after deletion');
        } else {
          device.sessions.splice(sessionsIndex, 1);
          if (device.sessions.length === 0 && !device.id) {
            // remove the web device since it doesn't have any sessions
            const deviceIndex = this.devices.findIndex(d => d.id === device.id);
            if (deviceIndex >= 0) {
              this.devices.splice(deviceIndex, 1);
            }
          }
        }
      }, error => this._messageService.error('Failed to delete session', error)
    );
  }

  deleteDevice(device: Device) {
    if (this.deleting || device.current || !device.id) {
      return;
    }

    this.deleting = true;
    this._http.delete(`auth/device/${device.id}`)
      .pipe(finalize(() => this.deleting = false))
      .subscribe(() => {
        this._messageService.success('Deleted device');
        const index = this.devices.findIndex(d => d.id === device.id);
        if (index < 0) {
          console.error('Failed to find device after deletion');
        } else {
          this.devices.splice(index, 1);
        }
        this.recheckFormEnabled();
      }, error => {
        this._messageService.error('Failed to delete device', error);
        this.recheckFormEnabled();
      });
  }

  deleteAllWebSessions() {
    if (this.deleting) {
      return;
    }

    const index = this.devices.findIndex(d => !d.id);
    if (index < 0) {
      this._messageService.error('Error deleting all web sessions');
    }
    const webDevice = this.devices[index];

    if (!this.anySessionsToDelete(webDevice)) {
      return;
    }

    this.deleting = true;
    this._http.delete('session/web')
    .pipe(finalize(() => this.deleting = false))
    .subscribe(() => {
      webDevice.sessions = webDevice.sessions.filter(x => x.current);
      if (webDevice.sessions.length === 0) {
        // remove the web device if it doesn't have any sessions
        this.devices.splice(index, 1);
      }
      this._messageService.success('Deleted all web sessions');
    }, error => {
      this._messageService.error('Failed to delete all web sessions', error);
    });
  }
}
