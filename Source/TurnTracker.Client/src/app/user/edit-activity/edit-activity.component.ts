import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { switchMap, map } from 'rxjs/operators';
import { MessageService } from 'src/app/services/message.service';
import { ActivityDetails } from '../models/ActivityDetails';
import { Message } from '@angular/compiler/src/i18n/i18n_ast';
import { inherits } from 'util';
import { ActivitySummary } from '../models/ActivitySummary';
import { AuthError } from 'src/app/auth/models/AuthError';

enum ErrorType {
  BadActivityId,
  NotAllowedToEditActivity,
  FailedToGetActivity,
  FailedToSaveActivity
}

class EditError {
  public message: string;
  constructor(message: string) {
    this.message = message;
  }
}

@Component({
  selector: 'app-edit-activity',
  templateUrl: './edit-activity.component.html',
  styleUrls: ['./edit-activity.component.scss']
})
export class EditActivityComponent implements OnInit {

  private _activityId: number;

  constructor(
    private _http: HttpClient,
    private _route: ActivatedRoute,
    private _router: Router,
    private _messageService: MessageService) { }

  ngOnInit() {
    const idParam = this._route.snapshot.paramMap.get('id');
    if (idParam === null) {
      this._messageService.info('must be adding');
      return;
    }

    const id = parseInt(idParam, 10);
    if (isNaN(id) || id <= 0) {
      this._messageService.error('Invalid activity ID');
      this._router.navigateByUrl('/activities');
      return;
    }

    this._activityId = id;

    this._http.get<ActivityDetails>(`activity/${id}`).subscribe(activity => {
      console.log('activity', activity);
      this._messageService.success('got activity details');
    },
    error => {
      if (error instanceof AuthError) {
        // do nothing we should be getting redirected
      } else if (error instanceof HttpErrorResponse && error.status === 403) {
          this._messageService.error('Not allowed to edit activity', error);
          this._router.navigate(['/activity', this._activityId]);
          return;
      } else {
        this._messageService.error('Failed to get activity', error);
      }
    });
  }
}
