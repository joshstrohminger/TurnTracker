import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';
import { Profile } from '../models/Profile';
import { MatSnackBar } from '@angular/material';
import { ErrorService } from 'src/app/services/error.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  user: Profile;

  public get roles() {
    return Role;
  }

  constructor(private _authService: AuthService, private _errorService: ErrorService) { }

  ngOnInit() {
    this._authService.getProfile().subscribe(
      profile => this.user = profile,
      error => this._errorService.show('Failed to get profile and settings', error));
  }
}
