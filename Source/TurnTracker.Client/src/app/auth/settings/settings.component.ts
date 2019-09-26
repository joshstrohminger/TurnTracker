import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';
import { Profile } from '../models/Profile';
import { MessageService } from 'src/app/services/message.service';

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

  constructor(private _authService: AuthService, private _messageService: MessageService) { }

  ngOnInit() {
    this._authService.getProfile().subscribe(
      profile => this.user = profile,
      error => this._messageService.error('Failed to get profile and settings', error));
  }
}
