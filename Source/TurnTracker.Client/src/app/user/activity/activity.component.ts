import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
import { switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Unit } from '../models/Unit';
import { DateTime } from 'luxon';
import { AuthService } from 'src/app/auth/auth.service';
import { NewTurn } from '../models/NewTurn';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ErrorService } from 'src/app/services/error.service';
import { TakeTurnDialog } from '../take-turn/take-turn.dialog';
import { TakeTurnDialogConfig } from '../take-turn/TakeTurnDialogConfig';
import { NotificationSetting } from '../models/NotificationSetting';
import { NotificationType } from '../models/NotificationType';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit {

  private _activityId: string;
  private _openedTurns = false;

  activity: ActivityDetails;
  includeTurns = false;
  busy = false;
  names = new Map<number, string>();
  myUserId: number;
  notifications: NotificationSetting[] = Object.keys(NotificationType)
    .filter(key => !isNaN(Number(NotificationType[key])))
    .map(key => new NotificationSetting(NotificationType[key]));

  public get units() {
    return Unit;
  }

  constructor(
    private _route: ActivatedRoute,
    private _http: HttpClient,
    private _authService: AuthService,
    private _errorService: ErrorService,
    private _dialog: MatDialog) { }

  ngOnInit() {
    if (this._authService.isLoggedIn) {
      this.myUserId = this._authService.currentUser.id;
    }
    this._route.paramMap.pipe(
      switchMap((params: ParamMap) => params.get('id'))
    ).subscribe(id => {
      this._activityId = id;
      this.refreshActivity();
    });
  }

  takeTurnWithOptions() {
    if (this.busy) {
      return;
    }
    this.busy = true;
    const dialogRef = this._dialog.open(TakeTurnDialog, {data: <TakeTurnDialogConfig>{
      activityName: this.activity.name,
      activityId: this.activity.id,
      myUserId: this.myUserId,
      participants: this.activity.participants
    }});
    dialogRef.afterClosed().subscribe((result: NewTurn) => {
      this.busy = false;
      if (result) {
        this.takeTurnUnsafe(result);
      }
    });
  }

  takeTurn(forUserId?: number) {
    if (this.busy) {
      return;
    }
    this.busy = true;

    const turn = new NewTurn(this.activity.id, forUserId || this.myUserId);
    this.takeTurnUnsafe(turn);
  }

  private takeTurnUnsafe(turn: NewTurn) {
    this._http.post<ActivityDetails>('turn', turn)
      .subscribe(updatedDetails => {
        this.busy = false;
        this.includeTurns = true;
        this.updateActivity(updatedDetails);
      }, error => {
        this.busy = false;
        this._errorService.show('Failed to take turn', error);
      });
  }

  refreshActivity() {
    if (this.busy) {
      return;
    }
    this.refreshActivityUnsafe();
  }

  loadTurns() {
    if (this.busy || this._openedTurns) {
      return;
    }
    this._openedTurns = true;
    this.includeTurns = true;
    this.refreshActivityUnsafe();
  }

  private refreshActivityUnsafe() {
    this.busy = true;
    this._http.get<ActivityDetails>(`activity/${this._activityId}${this.includeTurns ? '/allturns' : ''}`)
      .subscribe(activity => {
        this.busy = false;
        this.updateActivity(activity);
      }, error => {
        this.busy = false;
        this._errorService.show('Failed to get activity', error);
      });
  }

  private updateActivity(activity: ActivityDetails) {
    activity.participants.forEach(p => this.names.set(p.userId, p.name));
    const dueDate = DateTime.fromISO(activity.due);
    if (dueDate.isValid) {
      activity.dueDate = dueDate;
      activity.overdue = dueDate <= DateTime.local();
    }
    this.activity = activity;
  }
}
