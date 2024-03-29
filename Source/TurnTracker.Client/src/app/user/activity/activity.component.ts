import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationStart, Router } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
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
import { DangerDialog, IDangerDialogOptions } from '../danger-dialog/danger.dialog';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { finalize, takeUntil } from 'rxjs/operators';
import { TitleContentService } from 'src/app/services/title-content.service';
import { Overlay } from '@angular/cdk/overlay';
import { ReloadComponent } from '../reload/reload.component';
import { ComponentPortal } from '@angular/cdk/portal';
import { Subject } from 'rxjs';
import { Location } from '@angular/common';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit, OnDestroy {

  private _activityId: number;
  private _notificationPipe = new NotificationPipe();
  private _includeTurns = false;
  public get includeTurns(): boolean {
    return this._includeTurns;
  }
  private _hasTurns = false;
  private _myParticipantId: number;
  public originalDismissTimeofDay: string;
  public get hasTurns() {
    return this._hasTurns;
  }
  private readonly _done = new Subject<void>();

  dismissTimeOfDayControl: FormControl<string>;
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
    private _dialog: MatDialog,
    private _formBuilder: FormBuilder,
    private titleContentService: TitleContentService,
    private overlay: Overlay,
    private location: Location) {
    this.turns.filterPredicate = (turn: Turn, filter: string) => {
      return turn && (!filter || !turn.isDisabled);
    };
    this.filterTurns(false);
  }

  ngOnDestroy(): void {
    this._done.next();
    this._done.complete();
  }

  ngOnInit() {
    this.myUserId = this._userService.currentUser.id;

    const route = this._route.snapshot;
    const id = parseInt(route.paramMap.get('id'), 10);
    if (isNaN(id) || id <= 0) {
      this._messageService.error('Invalid activity ID');
      this._router.navigateByUrl('/activities');
      return;
    }

    this._activityId = id;
    let callback: () => any = null;
    const turnId = parseInt(route.paramMap.get('turnId'), 10);

    if(route.url[route.url.length - 1].path.toLowerCase() === 'taketurn') {
      callback = () => {
        this.location.replaceState(`activity/${this._activityId}`);
        this.takeTurnWithOptions();
      };
    } else if (!isNaN(turnId) && turnId > 0) {
      this._includeTurns = true;
      callback = () => {
        this.location.replaceState(`activity/${this._activityId}`);
        const turn = this.turns.data?.find(t => t.id === turnId);
        if(turn) {
          this.showTurnDetails(turn);
        } else {
          this._messageService.error('Invalid turn ID');
        }
      }
    }
    this.refreshActivity(callback);

    this._router.events.pipe(takeUntil(this._done)).subscribe((event: NavigationStart) => {
      if (event.navigationTrigger === 'popstate') {
        console.log('popping state', event);
      }
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

    this.location.go(`activity/${this._activityId}/taketurn`);

    const dialogRef = this._dialog.open(TakeTurnDialog, {data: <TakeTurnDialogConfig>{
      activityName: this.activity.name,
      activityId: this.activity.id,
      activityModifedDate: this.activity.modifiedDate,
      myUserId: this.myUserId,
      participants: this.activity.participants
    },
    minWidth: '16em'});
    let backdropClicked = false;
    dialogRef.backdropClick().subscribe(() => {
      backdropClicked = true;
    });
    dialogRef.afterClosed().subscribe((result: NewTurn|false|undefined) => {
      this.busy = false;
      // only toggle and go back if the result is false (dialog closed) or the backdrop was clicked, meaning the user initiated closing the dialog
      if(result || result === false || backdropClicked)
      {
        if (result) {
          this.takeTurnUnsafe(result as NewTurn);
        }
        this.location.back();
      }
    });
  }

  editActivity() {
    this._router.navigate(['/activity', this._activityId, 'edit']);
  }

  deleteActivity() {
    const options: IDangerDialogOptions = {
      action: 'Delete Activity',
      prompt: `Are you sure you want to delete activity ${this.activity.name}?`
    };
    const dialogRef = this._dialog.open(DangerDialog, {data: options});
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
            this._router.navigateByUrl('/activities', {replaceUrl: true});
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
    this.location.go(`activity/${this._activityId}/turn/${turn.id}`)

    const canModifyTurn = !this.activity.isDisabled && (this.myUserId === turn.creatorId || this.myUserId === turn.userId);
    const dialogRef = this._dialog.open(TurnDetailsDialog, {data: <TurnDetailsDialogConfig>{
      turn: turn,
      names: this.names,
      canModifyTurn: canModifyTurn
    }});
    let backdropClicked = false;
    dialogRef.backdropClick().subscribe(() => {
      backdropClicked = true;
    });
    dialogRef.afterClosed().subscribe((toggleTurnDisabled: boolean) => {
      // only toggle and go back if the result is true/false or the backdrop was clicked, meaning the user initiated closing the dialog
      if(typeof toggleTurnDisabled === 'boolean' || backdropClicked)
      {
        if (toggleTurnDisabled && canModifyTurn) {
          this.toggleTurnDisabled(turn);
        }
        this.location.back();
      }
    });
  }

  takeTurn(forUserId?: number) {
    if (this.busy) {
      return;
    }
    this.busy = true;

    const turn = new NewTurn(this.activity.modifiedDate, this.activity.id, forUserId || this.myUserId);
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
        } else if (error instanceof HttpErrorResponse && error.status === 409) {
          var config = ReloadComponent.BuildOverlayConfig(this.overlay);
          const overlayRef = this.overlay.create(config);
          const portal = new ComponentPortal(ReloadComponent);
          overlayRef.attach(portal);
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

  refreshActivity(callback: () => any = null) {
    if (this.busy) {
      return;
    }
    this.refreshActivityUnsafe(callback);
  }

  loadTurns() {
    if (this.busy || this._includeTurns) {
      return;
    }
    this._includeTurns = true;
    this.refreshActivityUnsafe();
  }

  private refreshActivityUnsafe(callback: () => any = null) {
    this.busy = true;
    this._http.get<ActivityDetails>(`activity/${this._activityId}${this._includeTurns ? '/allturns' : ''}`)
      .subscribe(activity => {
        this.busy = false;
        this.updateActivity(activity);
        callback?.call(this);
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
    this.titleContentService.setTitleContent(activity.name);

    for (const participant of activity.participants) {
      this.names.set(participant.userId, participant.name);

      // replace the current notification settings with those included here
      if (participant.userId === this.myUserId) {
        this._myParticipantId = participant.id;

        for (const note of this.notifications) {
          note.participantId = participant.id;
        }

        this.mobileNumberVerified = participant.mobileNumberVerification === VerificationStatus.Verified;
        this.emailVerified = participant.emailVerification === VerificationStatus.Verified;

        if (participant.notificationSettings) {
          for (const note of participant.notificationSettings) {
            const index = this.notifications.findIndex(n => n.type === note.type);
            if (index >= 0) {
              Object.assign(this.notifications[index], note);
            }
          }
        }

        if (participant.dismissTimeOfDay) {
          this.originalDismissTimeofDay = participant.dismissTimeOfDay.split(':').slice(0, 2).join(':');
          this.dismissTimeOfDayControl = this._formBuilder.control(this.originalDismissTimeofDay, Validators.required); ;
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

  resetDismissTimeOfDay() {
    this.dismissTimeOfDayControl.setValue(this.originalDismissTimeofDay);
  }

  saveDismissTimeOfDay() {
    const time = this.dismissTimeOfDayControl.value;
    if (this.dismissTimeOfDayControl.disabled || this.dismissTimeOfDayControl.invalid || time === this.originalDismissTimeofDay) {
      return;
    }

    this.dismissTimeOfDayControl.disable();
    this._http.put(`notification/${encodeURIComponent(this._myParticipantId)}/dismissTimeOfDay/${encodeURIComponent(time)}`, null).pipe(
      finalize(() => this.dismissTimeOfDayControl.enable())
    ).subscribe(
      () => this.originalDismissTimeofDay = time,
      error => this._messageService.error('Failed to save time', error));
  }

  saveNotificationSetting(note: NotificationSetting) {
    console.log('note changed', note);
    const name = this._notificationPipe.transform(note.type);
    this._http.post('notification', note)
      .subscribe(() => this._messageService.success(`Saved '${name}'`),
        error => this._messageService.error(`Error saving '${name}'`, error));
  }
}
