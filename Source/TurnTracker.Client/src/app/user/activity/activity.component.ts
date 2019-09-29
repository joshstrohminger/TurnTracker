import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
import { switchMap } from 'rxjs/operators';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { Unit } from '../models/Unit';
import { DateTime } from 'luxon';
import { AuthService } from 'src/app/auth/auth.service';
import { NewTurn } from '../models/NewTurn';
import { MatDialog, MatTableDataSource } from '@angular/material';
import { MessageService } from 'src/app/services/message.service';
import { TakeTurnDialog } from '../take-turn/take-turn.dialog';
import { TakeTurnDialogConfig } from '../take-turn/TakeTurnDialogConfig';
import { NotificationSetting } from '../models/NotificationSetting';
import { NotificationType } from '../models/NotificationType';
import { NotificationPipe } from '../notification.pipe';
import { Turn } from '../models/Turn';
import { TurnDetailsDialog } from '../turn-details/turn-details.dialog';
import { TurnDetailsDialogConfig } from '../turn-details/TurnDetailsDialogConfig';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit {

  private _activityId: string;
  private _notificationPipe = new NotificationPipe();
  private _includeTurns = false;
  private _hasTurns = false;
  public get hasTurns() {
    return this._hasTurns;
  }

  activity: ActivityDetails;
  busy = false;
  names = new Map<number, string>();
  myUserId: number;
  turns = new MatTableDataSource<Turn>();
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
    private _messageService: MessageService,
    private _dialog: MatDialog) {
      this.turns.filterPredicate = (turn: Turn, filter: string) => {
        return turn && (!filter || !turn.isDisabled);
      };
      this.filterTurns(false);
    }

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

  filterTurns(includeDisabledTurns: boolean) {
    this.turns.filter = includeDisabledTurns ? '' : 'asdf';
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

  showTurnDetails(turn: Turn) {
    const canModifyTurn = this.myUserId === turn.creatorId || this.myUserId === turn.userId;
    const dialogRef = this._dialog.open(TurnDetailsDialog, {data: <TurnDetailsDialogConfig>{
      turn: turn,
      names: this.names,
      canModifyTurn: canModifyTurn
    }});
    dialogRef.afterClosed().subscribe((toggleTurnDisabled: boolean) => {
      if (toggleTurnDisabled && canModifyTurn) {
        this.toggleTurnDisabled(turn);
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
        this._includeTurns = true;
        this.updateActivity(updatedDetails);
      }, error => {
        this.busy = false;
        this._messageService.error('Failed to take turn', error);
      });
  }

  toggleTurnDisabled(turn: Turn) {
    if (this.busy) {
      return;
    }
    this.busy = true;
    const url = `turn/${turn.id}`;
    const request = turn.isDisabled ? this._http.put<ActivityDetails>(url, null) : this._http.delete<ActivityDetails>(url);
    request.subscribe(updatedDetails => {
      this.busy = false;
      this._includeTurns = true;
      this.updateActivity(updatedDetails);
    }, error => {
      this.busy = false;
      this._messageService.error('Failed to modify turn', error);
    });
  }

  refreshActivity() {
    if (this.busy) {
      return;
    }
    this.refreshActivityUnsafe();
  }

  loadTurns() {
    if (this.busy || this._includeTurns) {
      return;
    }
    this._includeTurns = true;
    this.refreshActivityUnsafe();
  }

  private refreshActivityUnsafe() {
    this.busy = true;
    this._http.get<ActivityDetails>(`activity/${this._activityId}${this._includeTurns ? '/allturns' : ''}`)
      .subscribe(activity => {
        this.busy = false;
        this.updateActivity(activity);
      }, error => {
        this.busy = false;
        this._messageService.error('Failed to get activity', error);
      });
  }

  private updateActivity(activity: ActivityDetails) {
    for (const participant of activity.participants) {
      this.names.set(participant.userId, participant.name);

      // replace the current notification settings with those included here
      if (participant.userId === this.myUserId) {
        for (const note of this.notifications) {
          note.participantId = participant.id;
        }

        if (participant.notificationSettings) {
          for (const note of participant.notificationSettings) {
            const index = this.notifications.findIndex(n => n.type === note.type);
            if (index >= 0) {
              this.notifications[index] = note;
            }
          }
        }
      }
    }

    const dueDate = DateTime.fromISO(activity.due);
    if (dueDate.isValid) {
      activity.dueDate = dueDate;
      activity.overdue = dueDate <= DateTime.local();
    }

    const turns = activity.turns;
    activity.turns = null;
    this.activity = activity;
    this.turns.data = turns || [];
    this._hasTurns = !!turns;
  }

  saveNotificationSetting(note: NotificationSetting) {
    console.log('note changed', note);
    const name = this._notificationPipe.transform(note.type);
    this._http.post('notification', note)
      .subscribe(() => this._messageService.success(`Saved '${name}'`),
        error => this._messageService.error(`Error saving '${name}'`, error));
  }
}
