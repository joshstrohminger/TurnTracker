import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private readonly baseUrl = `${window.location.origin}/api/`;

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const savedAccessToken = localStorage.getItem('access-token');
    return (savedAccessToken ? of(savedAccessToken) : this.getAccessTokenUsingRefreshToken(next))
      .pipe(
        catchError(error => of(null as string)),
        map(accessToken => {
          const update: {setHeaders?: {}, url: string} = {
            url: `${this.baseUrl}${request.url}`
          };
          if (accessToken) {
            update.setHeaders = { Authorization: `Bearer ${accessToken}` };
          }
          return request.clone(update);
        }),
        mergeMap(requestToSend => {
          console.log('making request', requestToSend);
          return next.handle(requestToSend);
        })
      );
  }

  private getAccessTokenUsingRefreshToken(next: HttpHandler): Observable<string> {
    const refreshToken = localStorage.getItem('refresh-token');

    if (!refreshToken) {
      return of(null);
    }

    const refreshRequest = new HttpRequest(
      'POST',
      `${this.baseUrl}auth/refresh`,
      {
        headers: new HttpHeaders({ Authorization: `Bearer ${refreshToken}` }),
        responseType: 'text'
      });

    return next.handle(refreshRequest).pipe(
      map(
        event => {
          console.log('used refresh token', event);
          const accessToken = '' + event;
          localStorage.setItem('access-token', accessToken);
          return accessToken;
        },
        error => {
          console.error('failed to use refresh token', error);
          return null;
        })
    );

  }
}
