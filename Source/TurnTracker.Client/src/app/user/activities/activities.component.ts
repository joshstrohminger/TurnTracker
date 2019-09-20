import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivitySummary } from '../models/ActivitySummary';

@Component({
  selector: 'app-activities',
  templateUrl: './activities.component.html',
  styleUrls: ['./activities.component.scss']
})
export class ActivitiesComponent implements OnInit {

  activities: ActivitySummary[];

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.http.get<ActivitySummary[]>('activities/participating')
      .subscribe(activities => this.activities = activities,
        error => {
          alert('failed to get activities: ' + error.status);
      });
  }

}
