import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivitySummary } from '../models/ActivitySummary';
import { DateTime } from 'luxon';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MessageService } from 'src/app/services/message.service';
import { Router } from '@angular/router';
import { UserService } from 'src/app/services/user.service';
import { IUser } from 'src/app/auth/models/IUser';
import { AuthError } from 'src/app/auth/models/AuthError';

@Component({
  selector: 'app-activities',
  templateUrl: './activities.component.html',
  styleUrls: ['./activities.component.scss']
})
export class ActivitiesComponent implements OnInit {

  activities = new MatTableDataSource<ActivitySummary>();
  anyDisabledActivities = false;
  me: IUser;

  @ViewChild(MatSort, {static: true}) sort: MatSort;
  displayedColumns: string[] = ['name', 'currentTurnUserDisplayName', 'dueDate'];

  constructor(
    private _http: HttpClient,
    private _userService: UserService,
    private _messageService: MessageService,
    private _router: Router) {
      this.activities.filterPredicate = (activity: ActivitySummary, filter: string) => {
        return activity && (!filter || !activity.isDisabled);
      };
      this.filterActivities(false);
    }

  viewDetails(activity: ActivitySummary) {
    this._router.navigate(['/activity', activity.id]);
  }

  ngOnInit() {
    this.me = this._userService.currentUser;

    this._http.get<ActivitySummary[]>('activities/participating')
      .subscribe(activities => {
        for (const activity of activities) {
          if (activity.due) {
            const dueDate = DateTime.fromISO(activity.due);
            if (dueDate.isValid) {
              activity.dueDate = dueDate;
              activity.overdue = dueDate <= DateTime.local();
            }
          }
        }
        this.activities.sort = this.sort;
        this.anyDisabledActivities = activities.reduce((isDisabled, activity) => isDisabled || activity.isDisabled, false);
        if (this.me && this.me.showDisabledActivities && this.anyDisabledActivities) {
          this.filterActivities(true);
        }
        this.activities.data = activities || [];
      }, error => {
        if (!(error instanceof AuthError)) {
          this._messageService.error('Failed to get activities', error);
        }
      });
  }

  filterActivities(includeDisabledActivities: boolean) {
    if (this.me) {
      this.me.showDisabledActivities = includeDisabledActivities;
    }
    this.activities.filter = includeDisabledActivities ? '' : 'asdf';
  }

  addActivity() {
    this._router.navigateByUrl('/activity/add');
  }
}
