import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActivityService } from '../services/activity.service';
import { finalize } from 'rxjs/operators';

@Injectable()
export class ActivityInterceptor implements HttpInterceptor {
  private requests = 0;

  constructor(private _activityService: ActivityService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.begin();
    return next.handle(request).pipe(finalize(() => this.end()));
  }

  private begin(): void {
    this.requests = Math.max(this.requests, 0) + 1;
    if (this.requests === 1) {
      this._activityService.isActive = true;
    }
  }

  private end(): void {
    this.requests = Math.max(this.requests, 1) - 1;
    if (this.requests === 0) {
      this._activityService.isActive = false;
    }
  }
}
