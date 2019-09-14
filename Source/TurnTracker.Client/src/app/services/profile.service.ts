import { Injectable } from '@angular/core';
import { Profile } from './models/Profile';
import { Role } from './models/Role';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  private _currentUser: Profile;
  public get currentUserProfile() {
    return this._currentUser;
  }

  constructor(private router: Router) { }

  public login(username: string, password: string) {
    if (username && password && username.toLowerCase() === 'josh' && password === 'password') {
      this._currentUser = {
        displayName: 'Joshua',
        id: 1,
        role: Role.Admin,
        username: 'josh'
      };
      this.router.navigate(['home']);
    }
  }

  public logout() {
    this._currentUser = null;
    this.router.navigate(['login']);
  }
}
