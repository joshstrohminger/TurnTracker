import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'src/app/services/message.service';
import { AuthError } from 'src/app/auth/models/AuthError';
import { EditableActivity } from '../models/EditableActivity';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Unit } from '../models/Unit';

@Component({
  selector: 'app-edit-activity',
  templateUrl: './edit-activity.component.html',
  styleUrls: ['./edit-activity.component.scss']
})
export class EditActivityComponent implements OnInit {

  private _activityId: number;
  public editForm: FormGroup;
  public unitValues = Object.keys(Unit).map(x => parseInt(x, 10)).filter(x => !isNaN(<any>x));
  public units = Unit;

  constructor(
    private _http: HttpClient,
    private _route: ActivatedRoute,
    private _router: Router,
    private _formBuilder: FormBuilder,
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

    this._http.get<EditableActivity>(`activity/${id}/edit`).subscribe(activity => {
      this.editForm = this._formBuilder.group({
        name: [activity.name, [Validators.required]],
        takeTurns: [activity.takeTurns],
        periodCount: [{value: activity.periodCount, disabled: !!activity.periodUnit}],
        periodUnit: [activity.periodUnit]
      });

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
