import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, of, concat } from 'rxjs';
import { map, first, defaultIfEmpty, catchError, filter, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Credentials } from './login/Credentials';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { AuthenticatedUser } from './models/AuthenticatedUser';
import { Profile } from './models/Profile';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  public get isLoggedIn() {
    return !!this._currentUser;
  }

  private _currentUser: AuthenticatedUser = null;
  public get currentUser() {
    return this._currentUser;
  }

  constructor(private router: Router, private route: ActivatedRoute, private http: HttpClient) {
    const saved = localStorage.getItem('user');
    if (saved) {
      this._currentUser = JSON.parse(saved);
    }
  }

  private saveUser(user?: AuthenticatedUser) {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('access-token', user.accessToken);
      localStorage.setItem('refresh-token', user.refreshToken);
      this._currentUser = user;
    } else {
      localStorage.removeItem('user');
      localStorage.removeItem('access-token');
      localStorage.removeItem('refresh-token');
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
          const url = params && params.redirectUrl || '/home';
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
        this.saveUser();
      },
      () => {
        this.router.navigateByUrl('/login');
        this.saveUser();
      });
  }

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>('auth/profile');
  }
}
