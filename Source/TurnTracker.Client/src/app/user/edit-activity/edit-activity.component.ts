import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'src/app/services/message.service';
import { AuthError } from 'src/app/auth/models/AuthError';
import { EditableActivity } from '../models/EditableActivity';
import { FormGroup, FormBuilder, Validators, FormControl, FormArray } from '@angular/forms';
import { Unit } from '../models/Unit';
import { User } from '../models/User';
import { UserService } from 'src/app/services/user.service';
import { debounceTime, tap, switchMap, finalize, filter, map } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { ActivityDetails } from '../models/ActivityDetails';
import { TurnTrackerValidators } from 'src/app/validators/TurnTrackerValidators';
import { TitleContentService } from 'src/app/services/title-content.service';
import { NotificationSetting } from '../models/NotificationSetting';
import { NotificationType } from '../models/NotificationType';

@Component({
  selector: 'app-edit-activity',
  templateUrl: './edit-activity.component.html',
  styleUrls: ['./edit-activity.component.scss']
})
export class EditActivityComponent implements OnInit {

  private _activityId: number;
  public readonly myId: number;
  public readonly myName: string;
  public editForm: FormGroup<{
    name: FormControl<string>,
    description: FormControl<string>,
    takeTurns: FormControl<boolean>,
    periodCount: FormControl<number>,
    periodUnit: FormControl<string | Unit>,
    searchControl: FormControl<string | User>,
  }>;
  public unitValues = Object.keys(Unit).map(x => parseInt(x, 10)).filter(x => !isNaN(<any>x));
  public notifications: NotificationSetting[] = Object.keys(NotificationType)
    .filter(key => !isNaN(Number(NotificationType[key])))
    .map(key => new NotificationSetting(NotificationType[key]));
  public units = Unit;
  private countDigits = 3;
  public readonly defaultUnit = 'Non-Periodic';
  public readonly countMin = 1;
  public isLoading = false;
  public availableUsers: User[] = [];
  public participants: User[] = [];
  readonly countMax = parseInt('9'.repeat(this.countDigits), 10);
  public get hasUnitDefined() {
    return this.editForm && this.isUnit(this.editForm.value.periodUnit);
  }

  constructor(
    private _http: HttpClient,
    private _route: ActivatedRoute,
    private _router: Router,
    private _formBuilder: FormBuilder,
    private _messageService: MessageService,
    private titleContentService: TitleContentService,
    userService: UserService) {
      const me = userService.currentUser;
      this.myId = me.id;
      this.myName = me.displayName;
    }

  ngOnInit() {
    let getActivity: Observable<EditableActivity>;
    const idParam = this._route.snapshot.paramMap.get('id');
    if (idParam === null) {
      this._activityId = 0;
      getActivity = of(<EditableActivity>{
        id: 0,
        isDisabled: false,
        name: '',
        periodCount: null,
        periodUnit: this.defaultUnit as any,
        takeTurns: true,
        participants: [<User>{
          id: this.myId,
          name: this.myName
        }]
      });
    } else {
      const id = parseInt(idParam, 10);
      if (idParam !== null && (isNaN(id) || id <= 0)) {
        this._messageService.error('Invalid activity ID');
        this._router.navigateByUrl('/activities');
        return;
      }
      this._activityId = id;
      getActivity = this._http.get<EditableActivity>(`activity/${id}/edit`);
    }

    getActivity.subscribe(activity => {
      this.titleContentService.setTitleContent(activity.name);

      this.editForm = this._formBuilder.group({
        name: [activity.name, [Validators.required, TurnTrackerValidators.whitespace]],
        description: [activity.description],
        takeTurns: [activity.takeTurns],
        periodCount: [activity.periodCount,
          [Validators.required, Validators.pattern('[0-9]+'), Validators.min(this.countMin), Validators.max(this.countMax)]],
        periodUnit: [activity.periodUnit === null ? this.defaultUnit : activity.periodUnit],
        searchControl: ['' as string | User]
      });
      this.participants = activity.participants;

      if (activity.defaultNotificationSettings) {
        for (const note of activity.defaultNotificationSettings) {
          const index = this.notifications.findIndex(n => n.type === note.type);
          if (index >= 0) {
            Object.assign(this.notifications[index], note);
          }
        }
      }

      const periodUnitChangeHandler = value => {
        const control = this.editForm.controls.periodCount;
        if (isNaN(value)) {
          control.disable();
        } else {
          control.enable();
        }
      };

      periodUnitChangeHandler(this.editForm.value.periodUnit);
      this.editForm.controls.periodUnit.valueChanges.subscribe(periodUnitChangeHandler);

      this.editForm.controls.searchControl.valueChanges.pipe(
        map(x => {
          if (this.isUser(x)) {
            console.log('selected a user', x);
            // don't add an existing user to the list of participants
            if (!this.participants.find(p => p.id === x.id)) {
              this.participants.push(x);
            }
            this.editForm.controls.searchControl.reset();
            this.availableUsers = [];
            return null;
          }
          return x;
        }),
        filter(x => x && !this.isUser(x) && !!x.trim()),
        debounceTime(500),
        tap(() => {
          this.availableUsers = [];
          this.isLoading = true;
        }),
        switchMap(value => this._http.get<User[]>('users', {params: new HttpParams().set('filter', value)})
          .pipe(finalize(() => this.isLoading = false)))
      ).subscribe(users => {
        // todo filter out user that are already participants
        this.availableUsers = (users && users.filter(u => !this.participants.find(p => p.id === u.id))) || [];
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

  isUnit(x: string | Unit): x is Unit {
    return !isNaN(x as Unit);
  }

  isUser(x: string | User): x is User {
    return !!(x as User)?.id;
  }

  removeUser(user: User, index: number) {
    if (user.id === this.myId) {
      return;
    }

    this.participants.splice(index, 1);
  }

  displayUser(user?: User): string | undefined {
    return user ? user.name : undefined;
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

  saveActivity() {
    const takeTurns = this.editForm.value.takeTurns;
    const activity: EditableActivity = {
      id: this._activityId,
      name: this.editForm.value.name,
      description: this.editForm.value.description,
      periodCount: this.editForm.value.periodCount,
      periodUnit: Number(this.editForm.value.periodUnit),
      takeTurns,
      isDisabled: false,
      participants: this.participants,
      defaultNotificationSettings: this.notifications.filter(note => note.anyChecked && note.isAllowed(takeTurns))
    };
    this._http.post<ActivityDetails>('activity/save', activity).subscribe(activity => {
      this._messageService.success(`Saved activity`);
      this._router.navigate(['/activity', activity.id], {replaceUrl: true});
    },
    error => {
      const message = error && error.error || 'unknown';
      this._messageService.error(`Error saving activity, ${message}`, error);
    });
  }
}
