import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivitySummary } from '../models/ActivitySummary';
import { DateTime } from 'luxon';
import { AuthService } from 'src/app/auth/auth.service';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MessageService } from 'src/app/services/message.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-activities',
  templateUrl: './activities.component.html',
  styleUrls: ['./activities.component.scss']
})
export class ActivitiesComponent implements OnInit {

  activities: MatTableDataSource<ActivitySummary>;
  myUserId: number;

  @ViewChild(MatSort, {static: true}) sort: MatSort;
  displayedColumns: string[] = ['name', 'currentTurnUserDisplayName', 'dueDate'];

  constructor(
    private _http: HttpClient,
    private _authService: AuthService,
    private _messageService: MessageService,
    private _router: Router) { }

  viewDetails(activity: ActivitySummary) {
    this._router.navigate(['/activity', activity.id]);
  }

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
        this.activities = new MatTableDataSource(activities);
        this.activities.sort = this.sort;
      }, error => this._messageService.error('Failed to get activities', error));
  }

}
