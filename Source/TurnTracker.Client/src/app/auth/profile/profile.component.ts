import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';
import { Profile } from '../models/Profile';
import { MessageService } from 'src/app/services/message.service';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { TurnTrackerValidators } from 'src/app/validators/TurnTrackerValidators';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  user: Profile;
  displayNameControl: FormControl;

  public get roles() {
    return Role;
  }

  constructor(
    private _authService: AuthService,
    private _messageService: MessageService,
    private _formBuilder: FormBuilder,
    private _http: HttpClient) { }

  ngOnInit() {
    this._authService.getProfile().subscribe(
      profile => {
        this.user = profile;
      },
      error => {
        this._messageService.error('Failed to get profile', error);
      });
  }

  getErrorMessage() {
    if (this.displayNameControl.hasError('required') || this.displayNameControl.hasError('nonwhitespace')) {
      return 'Required';
    }
    if (this.displayNameControl.hasError('newvalue')) {
      return 'Enter new DisplayName';
    }
    return '';
  }

  editDisplayName() {
    this.displayNameControl = this._formBuilder.control(this.user.displayName, [
      Validators.required,
      TurnTrackerValidators.nonWhitespace,
      TurnTrackerValidators.newValue(this.user.displayName)
    ]);
  }

  cancelEditingDisplayName() {
    this.displayNameControl = null;
  }

  saveDisplayName() {
    const displayName = (<string>this.displayNameControl.value).trim();
    this.displayNameControl.disable();
    this._http.put<Profile>('profile/displayname', displayName).subscribe(
      profile => {
        this._messageService.success('Saved DisplayName');
        this.user = profile;
        this.displayNameControl = null;
        this._authService.setCurrentUserDisplayName(this.user.displayName);
      },
      error => {
        this._messageService.error('Failed to save DisplayName', error);
        this.displayNameControl.enable();
      });
  }
}
