import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
import { switchMap } from 'rxjs/operators';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Unit } from '../models/Unit';
import { DateTime } from 'luxon';
import { NewTurn } from '../models/NewTurn';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MessageService } from 'src/app/services/message.service';
import { TakeTurnDialog } from '../take-turn/take-turn.dialog';
import { TakeTurnDialogConfig } from '../take-turn/TakeTurnDialogConfig';
import { NotificationSetting } from '../models/NotificationSetting';
import { NotificationType } from '../models/NotificationType';
import { NotificationPipe } from '../notification.pipe';
import { Turn } from '../models/Turn';
import { TurnDetailsDialog } from '../turn-details/turn-details.dialog';
import { TurnDetailsDialogConfig } from '../turn-details/TurnDetailsDialogConfig';
import { VerificationStatus } from '../models/Participant';
import { UserService } from 'src/app/services/user.service';
import { DeleteActivityDialog } from '../delete-activity/delete-activity.dialog';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit {

  private _activityId: number;
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
  mobileNumberVerified: boolean;
  emailVerified: boolean;
  notifications: NotificationSetting[] = Object.keys(NotificationType)
    .filter(key => !isNaN(Number(NotificationType[key])))
    .map(key => new NotificationSetting(NotificationType[key]));

  public get units() {
    return Unit;
  }

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _http: HttpClient,
    private _userService: UserService,
    private _messageService: MessageService,
    private _dialog: MatDialog) {
      this.turns.filterPredicate = (turn: Turn, filter: string) => {
        return turn && (!filter || !turn.isDisabled);
      };
      this.filterTurns(false);
    }

  ngOnInit() {
    this.myUserId = this._userService.currentUser.id;

    const idParam = this._route.snapshot.paramMap.get('id');
    const id = parseInt(idParam, 10);
    if (isNaN(id) || id <= 0) {
      this._messageService.error('Invalid activity ID');
      this._router.navigateByUrl('/activities');
      return;
    }

    this._activityId = id;
    this.refreshActivity();
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

  editActivity() {
    this._router.navigate(['/activity', this._activityId, 'edit']);
  }

  deleteActivity() {
    const dialogRef = this._dialog.open(DeleteActivityDialog, {data: this.activity});
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        if (this.busy) {
          return;
        }
        this.busy = true;
        this._http.post(`activity/${this._activityId}/delete`, null).subscribe(
          () => {
            this.busy = false;
            this._messageService.success(`Deleted activity ${this.activity.name}`);
            this._router.navigateByUrl('/activities');
          }, error => {
            this.busy = false;
            if (error instanceof HttpErrorResponse && error.status === 403) {
              this._messageService.error('Not allowed to delete activity');
            } else {
              this._messageService.error(`Failed to delete activity`, error);
            }
          }
        );
      }
    });
  }

  showTurnDetails(turn: Turn) {
    const canModifyTurn = !this.activity.isDisabled && (this.myUserId === turn.creatorId || this.myUserId === turn.userId);
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
        if (error instanceof HttpErrorResponse && error.status === 403) {
          this._messageService.error('Not allowed to take turn');
          this._router.navigateByUrl('/activities');
        } else {
          this._messageService.error('Failed to take turn', error);
        }
      });
  }

  private toggleTurnDisabled(turn: Turn) {
    if (this.busy) {
      return;
    }
    this.busy = true;
    const url = `turn/${turn.id}`;
    const request = turn.isDisabled ? this._http.put<ActivityDetails>(url, null) : this._http.delete<ActivityDetails>(url);
    request.subscribe(updatedDetails => {
      this._messageService.success(`${turn.isDisabled ? 'Enabled' : 'Disabled'} turn`);
      this.busy = false;
      this._includeTurns = true;
      this.updateActivity(updatedDetails);
    }, error => {
      this.busy = false;
      if (error instanceof HttpErrorResponse && error.status === 403) {
        this._messageService.error('Not allowed to modify turn');
      } else {
        this._messageService.error(`Failed to ${turn.isDisabled ? 'enable' : 'disable'} turn`, error);
      }
    });
  }

  public toggleActivityDisabled() {
    if (this.busy) {
      return;
    }
    this.busy = true;
    const url = `activity/${this._activityId}`;
    const request = this.activity.isDisabled ? this._http.put<ActivityDetails>(url, null) : this._http.delete<ActivityDetails>(url);
    request.subscribe(updatedDetails => {
      this._messageService.success(`${this.activity.isDisabled ? 'Enabled' : 'Disabled'} activity`);
      this.busy = false;
      this._includeTurns = true;
      this.updateActivity(updatedDetails);
    }, error => {
      this.busy = false;
      if (error instanceof HttpErrorResponse && error.status === 403) {
        this._messageService.error('Not allowed to modify activity');
      } else {
        this._messageService.error(`Failed to ${this.activity.isDisabled ? 'enable' : 'disable'} activity`, error);
      }
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
        if (error instanceof HttpErrorResponse && error.status === 403) {
          this._messageService.error('Not allowed to view activity');
          this._router.navigateByUrl('/activities');
        } else {
          this._messageService.error('Failed to get activity', error);
        }
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

        this.mobileNumberVerified = participant.mobileNumberVerification === VerificationStatus.Verified;
        this.emailVerified = participant.emailVerification === VerificationStatus.Verified;

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
