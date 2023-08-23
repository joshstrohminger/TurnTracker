import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivitySummary } from '../models/ActivitySummary';
import { DateTime } from 'luxon';
import { MatSort } from '@angular/material/sort';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';
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

  @ViewChild(MatSort, {static: false}) set sort(sort: MatSort) {
      this.activities.sort = sort;
  }

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
    // reload the page automatically after an hour
    setTimeout(() => document.location.reload(), 60 * 60 * 1000);

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
        this.anyDisabledActivities = activities.reduce((isDisabled, activity) => isDisabled || activity.isDisabled, false);
        if (this.me && this.me.showDisabledActivities && this.anyDisabledActivities) {
          this.filterActivities(true);
        }
        this.activities.data = activities.sort((a, b) => {
          if (a.due !== b.due) {
            // activies that are due go first
            return a.due ? -1 : 1;
          }
          if (!!a.dueDate && !!b.dueDate) {
            // older due dates go first
            return a.dueDate.diff(b.dueDate).milliseconds;
          }
          // alphabetic fallback
          return a.name.localeCompare(b.name);
        }) || [];
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
