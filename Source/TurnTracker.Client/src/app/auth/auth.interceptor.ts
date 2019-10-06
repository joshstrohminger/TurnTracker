import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders,
  HttpErrorResponse,
  HttpResponse,
  HttpEventType
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { map, mergeMap, catchError, filter } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { AuthError } from './models/AuthError';
import { MessageService } from '../services/message.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private readonly baseUrl = `${window.location.origin}/api/`;

  constructor(private _authService: AuthService, private _messageService: MessageService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    if (request.url.endsWith('/login')) {
      return next.handle(this.modifyRequest(request));
    }

    const accessTokenInfo = this._authService.getAccessToken();
    return (accessTokenInfo.isValid ? of(accessTokenInfo.token) : this.getAccessTokenUsingRefreshToken(next)).pipe(
      map(accessToken => this.modifyRequest(request, accessToken)),
      mergeMap(preppedRequest => next.handle(preppedRequest).pipe(
        catchError(error => {
          if (error instanceof AuthError) {
            console.log('rethrowing AuthError: ' + error.statusText);
            throw error;
          } else if (error instanceof HttpErrorResponse && error.status === 401) {
            console.log('invalid access token, refreshing');
            return this.getAccessTokenUsingRefreshToken(next).pipe(
              map(accessToken => this.modifyRequest(request, accessToken)),
              mergeMap(secondRequest => next.handle(secondRequest))
            );
          } else {
            throw error;
          }
        })
      )),
      catchError(error => {
        if (error instanceof AuthError) {
          this._messageService.error(`Logged out: ${error.statusText}`, error);
          this._authService.logoutClientOnly();
        }
        throw error;
      })
    );
  }

  private modifyRequest(request: HttpRequest<any>, accessToken?: string): HttpRequest<any> {
    const update: { setHeaders?: {}, url: string } = {
      url: `${this.baseUrl}${request.url}`
    };
    if (accessToken) {
      update.setHeaders = { Authorization: `Bearer ${accessToken}` };
    }
    return request.clone(update);
  }

  private getAccessTokenUsingRefreshToken(next: HttpHandler): Observable<string> {
    const refreshTokenInfo = this._authService.getRefreshToken();

    if (!refreshTokenInfo.isValid) {
      return throwError(new AuthError('missing refresh token'));
    }

    const refreshRequest = new HttpRequest(
      'POST',
      `${this.baseUrl}${this._authService.refreshUrl}`,
      null,
      {
        headers: new HttpHeaders().set('Authorization', `Bearer ${refreshTokenInfo.token}`),
        responseType: 'text'
      });

    return next.handle(refreshRequest).pipe(
      filter(event => event instanceof HttpResponse),
      map(
        (response: HttpResponse<string>) => {
          console.log('used refresh token', response);
          const accessToken = response.body;
          this._authService.saveAccessToken(accessToken);
          return accessToken;
        }),
      catchError(error => {
        const message = 'failed to use refresh token';
        console.error(message, error);
        throw new AuthError(message);
      })
    );
  }
}
