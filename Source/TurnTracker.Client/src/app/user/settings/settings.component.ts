import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { IUser } from 'src/app/auth/models/IUser';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  me: IUser;

  constructor(
    private _userService: UserService) { }

  ngOnInit() {
    this.me = this._userService.currentUser;
  }
}
