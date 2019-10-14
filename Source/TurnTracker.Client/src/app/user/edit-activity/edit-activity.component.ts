import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'src/app/services/message.service';
import { AuthError } from 'src/app/auth/models/AuthError';
import { EditableActivity } from '../models/EditableActivity';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Unit } from '../models/Unit';
import { TurnTrackerValidators } from 'src/app/validators/TurnTrackerValidators';

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
  private countDigits = 3;
  readonly countMin = 1;
  readonly countMax = parseInt('9'.repeat(this.countDigits), 10);

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
        periodCount: [{
          value: activity.periodCount,
          disabled: isNaN(activity.periodUnit)},
        [Validators.required, Validators.pattern('[0-9]+'), Validators.min(this.countMin), Validators.max(this.countMax)]],
        periodUnit: [activity.periodUnit]
      });
      this.editForm.controls.periodUnit.valueChanges.subscribe(value => {
        const control = this.editForm.controls.periodCount;
        if (isNaN(value)) {
          control.disable();
        } else {
          control.enable();
        }
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

  getPeriodCountErrorMessage(): string {
    const control = this.editForm.controls.periodCount;

    if (control.hasError('required')) {
      return 'Required with unit';
    }

    if (control.hasError('pattern')) {
      return 'Must be an integer';
    }

    if (control.hasError('min')) {
      return `Min ${this.countMin}`;
    }

    if (control.hasError('max')) {
      return `Max ${this.countMax}`;
    }

    return '';
  }
}
