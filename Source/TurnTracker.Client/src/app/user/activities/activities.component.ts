import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivitySummary } from '../models/ActivitySummary';
import { DateTime } from 'luxon';
import { AuthService } from 'src/app/auth/auth.service';
import { MatSnackBar } from '@angular/material';
import { ErrorService } from 'src/app/services/error.service';

@Component({
  selector: 'app-activities',
  templateUrl: './activities.component.html',
  styleUrls: ['./activities.component.scss']
})
export class ActivitiesComponent implements OnInit {

  activities: ActivitySummary[];
  myUserId: number;

  constructor(private _http: HttpClient, private _authService: AuthService, private _errorService: ErrorService) { }

  ngOnInit() {
    if (this._authService.isLoggedIn) {
      this.myUserId = this._authService.currentUser.id;
    }

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
        this.activities = activities;
      }, error => this._errorService.show('Failed to get activities', error));
  }

}
