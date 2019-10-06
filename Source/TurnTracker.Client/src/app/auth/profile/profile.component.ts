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

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  user: Profile;
  displayNameControl: FormControl;
  passwordForm: FormGroup;
  passwordStrengthColor = 'warn';
  passwordStrength = 0;
  readonly passwordDesiredLength = 30;
  readonly immediateErrors = new ImmediateErrorStateMatcher();
  readonly parentErrors = new ParentErrorStateMatcher('different');
  readonly passwordStrengthIcons = ['😡', '😠', '🙁', '😕', '😒', '😑', '😐', '🙂', '😃', '😄', '😁'];
  passwordStrengthIcon = this.passwordStrengthIcons[0];

  public get roles() {
    return Role;
  }

  constructor(
    private _authService: AuthService,
    private _messageService: MessageService,
    private _formBuilder: FormBuilder,
    private _http: HttpClient,
    private _userService: UserService) { }

  ngOnInit() {
    this._authService.getProfile().subscribe(
      profile => {
        this.user = profile;
      },
      error => {
        this._messageService.error('Failed to get profile', error);
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
}
