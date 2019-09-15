import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';
import { Profile } from '../models/Profile';

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

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.authService.getProfile().subscribe(
      profile => this.user = profile,
      error => console.error('failed to get profile', error));
  }
}
