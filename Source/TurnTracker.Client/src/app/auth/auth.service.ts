import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, of } from 'rxjs';
import { tap, delay, filter, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Profile } from './models/Profile';
import { Role } from './models/Role';
import { Credentials } from './login/Credentials';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  public get isLoggedIn() {
    return !!this._currentUser;
  }

  private _currentUser: Profile = null;
  public get currentUser() {
    return this._currentUser;
  }

  constructor(private router: Router, private route: ActivatedRoute) {}

  loginRedirect(url?: string) {
    const options = {
      queryParams: url ? { redirectUrl: url } : {}
    };
    this.router.navigate(['login'], options);
  }

  login(credentials: Credentials): Observable<string> {
    return of(credentials && credentials.username && credentials.password &&
      credentials.username.toLowerCase() === 'josh' && credentials.password === 'password').pipe(
      delay(1000),
      map(success => {
        if (success) {
          this._currentUser = {
            displayName: 'Joshua',
            id: 1,
            role: Role.Admin,
            username: 'josh'
          };
          this.route.queryParams.pipe(filter(params => params.redirectUrl))
            .subscribe(params => this.router.navigateByUrl(params.redirectUrl));
          return null;
        }

        this._currentUser = null;
        return 'Invalid Credentials';
      })
    );
  }

  logout(): void {
    this._currentUser = null;
  }
}
