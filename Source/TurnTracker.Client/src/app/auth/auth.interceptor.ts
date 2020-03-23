import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders,
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { map, mergeMap, catchError, filter, share, tap, finalize } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { AuthError } from './models/AuthError';
import { MessageService } from '../services/message.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private readonly baseUrl = `${window.location.origin}/api/`;
  private refreshOperation$: Observable<string>;

  constructor(private _authService: AuthService, private _messageService: MessageService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const lastSegment = request.url.substring(request.url.lastIndexOf('/') + 1);
    switch (lastSegment) {
      case 'login':
      case 'publickey':
      case 'StartDeviceAssertion':
      case 'CompleteDeviceAssertion':
        console.log(`sending anonymous request for ${request.url}`);
        return next.handle(this.modifyRequest(request));
    }

    const accessTokenInfo = this._authService.getAccessToken();

    return (accessTokenInfo.isValid ?
        of(accessTokenInfo.token) :
        this.getAccessTokenUsingRefreshToken(next, request.url, 'before request')).pipe(
      map(accessToken => this.modifyRequest(request, accessToken)),
      mergeMap(preppedRequest => next.handle(preppedRequest).pipe(
        catchError(error => {
          if (error instanceof AuthError) {
            console.log(`rethrowing AuthError for '${request.url}': ${error.statusText}`);
            throw error;
          } else if (error instanceof HttpErrorResponse) {
            switch (error.status) {
              case 401:
                return this.getAccessTokenUsingRefreshToken(next, request.url, 'after request').pipe(
                  map(accessToken => this.modifyRequest(request, accessToken)),
                  mergeMap(secondRequest => next.handle(secondRequest))
                );
              case 504:
                const message = 'No connection';
                console.error(message, error);
                throw new AuthError(message);
              default:
                throw error;
            }
          } else {
            throw error;
          }
        })
      )),
      catchError(error => {
        if (error instanceof AuthError) {
          console.error(`Failed URL: ${request.url}`);
          if (error.statusText !== 'missing refresh token') {
            const errorMessage = `Logged out: ${error.statusText}`;
            this._messageService.error(errorMessage, error);
          }
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

  private getAccessTokenUsingRefreshToken(next: HttpHandler, originalUrl: string, reason: string): Observable<string> {
    const refreshTokenInfo = this._authService.getRefreshToken();

    if (!refreshTokenInfo.isValid) {
      return throwError(new AuthError('missing refresh token'));
    }

    if (!this.refreshOperation$) {
      console.log(`invalid access token ${reason} for '${originalUrl}', refreshing`);
      const refreshRequest = new HttpRequest(
        'POST',
        `${this.baseUrl}${this._authService.refreshUrl}`,
        null,
        {
          headers: new HttpHeaders().set('Authorization', `Bearer ${refreshTokenInfo.token}`),
          responseType: 'text'
        });

      this.refreshOperation$ = next.handle(refreshRequest).pipe(
        filter(event => event instanceof HttpResponse),
        map(
          (response: HttpResponse<string>) => {
            console.log('used refresh token', response);
            const accessToken = response.body;
            this._authService.saveAccessToken(accessToken);
            return accessToken;
          }),
        catchError(error => {
          const authError = this._toAuthError(error);
          throw authError;
        }),
        finalize(() => this.refreshOperation$ = null),
        share()
      );
    } else {
      console.log(`invalid access token ${reason} for '${originalUrl}', waiting for ongoing refresh`);
    }

    return this.refreshOperation$;
  }

  private _toAuthError(error: any): AuthError {
    let message = 'Error';
    if (error instanceof HttpErrorResponse) {
      switch (error.status) {
        case 504:
          message = 'No connection';
          break;
        default:
          message = error.statusText;
          break;
      }
    }
    console.error(message, error);
    return new AuthError(message);
  }
}
