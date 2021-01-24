import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, first, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Credentials } from './login/Credentials';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { AuthenticatedUser } from './models/AuthenticatedUser';
import { Profile } from './models/Profile';
import { TokenInfo } from './models/TokenInfo';
import { UserService } from '../services/user.service';
import { MessageService } from '../services/message.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  public readonly refreshUrl = 'auth/refresh';
  private readonly _accessTokenKey = 'access-token';
  private readonly _accessTokenExpirationKey = 'access-token-exp';
  private readonly _refreshTokenKey = 'refresh-token';
  private readonly _refreshTokenExpirationKey = 'refresh-token-exp';
  private readonly _savedUsernameKey = 'saved-username';
  private readonly _shouldSaveUsernameKey = 'should-save-username';

  public get isLoggedIn() {
    return !!this._userService.currentUser;
  }

  public get shouldSaveUsername() {
    return !!localStorage.getItem(this._shouldSaveUsernameKey);
  }

  public set shouldSaveUsername(shouldSave: boolean) {
    if (shouldSave) {
      console.log('setting should save');
      localStorage.setItem(this._shouldSaveUsernameKey, 'true');
    } else {
      console.log('clearing should save');
      localStorage.removeItem(this._shouldSaveUsernameKey);
      localStorage.removeItem(this._savedUsernameKey);
    }
  }

  public get savedUsername() {
    return localStorage.getItem(this._savedUsernameKey);
  }

  public set savedUsername(username: string) {
    localStorage.setItem(this._savedUsernameKey, username);
  }

  constructor(
    private _router: Router,
    private _route: ActivatedRoute,
    private _http: HttpClient,
    private _userService: UserService,
    messageService: MessageService) {
      if (this.isLoggedIn) {
        if (!this.getRefreshToken().isValid ) {
          messageService.error('session expired');
          this.logoutClientOnly();
        }
      }
    }

  private saveUser(user?: AuthenticatedUser) {
    console.log('saving user', user);
    if (user) {
      this.saveAccessToken(user.accessToken);
      this.saveRefreshToken(user.refreshToken);
      this._userService.currentUser = user;
    } else {
      this.saveAccessToken();
      this.saveRefreshToken();
      this._userService.currentUser = null;
    }
  }

  loginRedirect(url?: string) {
    this._router.navigate(['/login'], {queryParams: url ? { redirectUrl: url } : {}} )
  }

  login(credentials: Credentials): Observable<string> {

    return this._http.post<AuthenticatedUser>('auth/login', credentials).pipe(
      map(user => {
        this.saveAuthenticatedUser(user);
        return null as string;
      }), catchError((error: HttpErrorResponse) => {
        switch (error.status) {
          case 401:
            return of('Invalid credentials');
          default:
            console.error('Error while logging in', error);
            return of('Unknown error');
        }
      }));
  }

  saveAuthenticatedUser(user: AuthenticatedUser) {
    this.saveUser(user);
    if (this.shouldSaveUsername) {
      this.savedUsername = user.username;
    }
    this._route.queryParams.pipe(first()).subscribe(params => {
      const url = params && params.redirectUrl || '/activities';
      this._router.navigateByUrl(url);
    });
  }

  logout(): void {
    this._http.post('auth/logout', null).subscribe(
      () => console.log('logged out successfully'),
      error => {
        console.error('failed to logout', error);
        this.logoutClientOnly();
      },
      () => this.logoutClientOnly());
  }

  logoutClientOnly(): void {
    this.saveUser();
    this._router.navigateByUrl('/login');
  }

  getProfile(): Observable<Profile> {
    return this._http.get<Profile>('profile');
  }

  getAccessToken(): TokenInfo {
    return this.getToken(this._accessTokenKey, this._accessTokenExpirationKey);
  }

  getRefreshToken(): TokenInfo {
    return this.getToken(this._refreshTokenKey, this._refreshTokenExpirationKey);
  }

  private getToken(tokenKey: string, expirationKey: string): TokenInfo {
    const token = localStorage.getItem(tokenKey);
    const exp = localStorage.getItem(expirationKey);
    return new TokenInfo(
      token,
      token && exp && (parseInt(exp, 10) > new Date().getTime())
    );
  }

  saveAccessToken(accessToken?: string): boolean {
    return this.saveToken(accessToken, this._accessTokenKey, this._accessTokenExpirationKey);
  }

  saveRefreshToken(refreshToken?: string): boolean {
    return this.saveToken(refreshToken, this._refreshTokenKey, this._refreshTokenExpirationKey);
  }

  private saveToken(token: string, tokenKey: string, expirationKey: string): boolean {
    if (token) {
      try {
        const exp = JSON.parse(atob(token.split('.')[1]))['exp'] as number;
        localStorage.setItem(tokenKey, token);
        // exp is number of seconds but date is number of milliseconds
        localStorage.setItem(expirationKey, (exp * 1000).toString());
      } catch (error) {
        console.error('failed to parse jwt', token);
        return false;
      }
    } else {
      // clear it since one wasn't provided
      localStorage.removeItem(tokenKey);
      localStorage.removeItem(expirationKey);
    }
    return true;
  }
}
