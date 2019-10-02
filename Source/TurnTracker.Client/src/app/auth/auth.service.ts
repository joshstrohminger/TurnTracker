import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, of, concat } from 'rxjs';
import { map, first, defaultIfEmpty, catchError, filter, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Credentials } from './login/Credentials';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { AuthenticatedUser } from './models/AuthenticatedUser';
import { Profile } from './models/Profile';
import { TokenInfo } from './models/TokenInfo';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  public readonly refreshUrl = 'auth/refresh';
  private readonly userKey = 'user';
  private readonly accessTokenKey = 'access-token';
  private readonly accessTokenExpirationKey = 'access-token-exp';
  private readonly refreshTokenKey = 'refresh-token';
  private readonly refreshTokenExpirationKey = 'refresh-token-exp';

  public get isLoggedIn() {
    return !!this._currentUser;
  }

  private _currentUser: AuthenticatedUser = null;
  public get currentUser() {
    return this._currentUser;
  }

  constructor(private router: Router, private route: ActivatedRoute, private http: HttpClient) {
    const saved = localStorage.getItem(this.userKey);
    if (saved) {
      this._currentUser = JSON.parse(saved);
    }
  }

  setCurrentUserDisplayName(displayName: string) {
    const saved = localStorage.getItem(this.userKey);
    if (saved) {
      this._currentUser = JSON.parse(saved);
      this._currentUser.displayName = displayName;
      localStorage.setItem(this.userKey, JSON.stringify(this._currentUser));
    }
  }

  private saveUser(user?: AuthenticatedUser) {
    if (user) {
      localStorage.setItem(this.userKey, JSON.stringify(user));
      this.saveAccessToken(user.accessToken);
      this.saveRefreshToken(user.refreshToken);
      this._currentUser = user;
    } else {
      localStorage.removeItem(this.userKey);
      this.saveAccessToken();
      this.saveRefreshToken();
      this._currentUser = null;
    }
  }

  loginRedirect(url?: string) {
    const options = {
      queryParams: url ? { redirectUrl: url } : {}
    };
    this.router.navigateByUrl('/login', options);
  }

  login(credentials: Credentials): Observable<string> {

    return this.http.post<AuthenticatedUser>('auth/login', credentials).pipe(
      map(profile => {
        this.saveUser(profile);
        this.route.queryParams.pipe(first()).subscribe(params => {
          const url = params && params.redirectUrl || '/activities';
          this.router.navigateByUrl(url);
        });
        return null as string;
      }), catchError((error: HttpErrorResponse) => {
        switch (error.status) {
          case 401:
            return of('Invalid credentials');
          default:
            return of('Unknown error');
        }
      }));
  }

  logout(): void {
    this.http.post('auth/logout', null).subscribe(
      () => console.log('logged out successfully'),
      error => {
        console.error('failed to logout', error);
        this.logoutClientOnly();
      },
      () => this.logoutClientOnly());
  }

  logoutClientOnly(): void {
    this.saveUser();
    this.router.navigateByUrl('/login');
  }

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>('profile');
  }

  getAccessToken(): TokenInfo {
    return this.getToken(this.accessTokenKey, this.accessTokenExpirationKey);
  }

  getRefreshToken(): TokenInfo {
    return this.getToken(this.refreshTokenKey, this.refreshTokenExpirationKey);
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
    return this.saveToken(accessToken, this.accessTokenKey, this.accessTokenExpirationKey);
  }

  saveRefreshToken(refreshToken?: string): boolean {
    return this.saveToken(refreshToken, this.refreshTokenKey, this.refreshTokenExpirationKey);
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
