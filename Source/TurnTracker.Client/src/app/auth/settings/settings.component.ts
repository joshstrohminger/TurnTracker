import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Role } from '../models/Role';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  public get user() {
    return this.authService.currentUser;
  }

  public get roles() {
    return Role;
  }

  constructor(private authService: AuthService) { }

  ngOnInit() {
  }

}
