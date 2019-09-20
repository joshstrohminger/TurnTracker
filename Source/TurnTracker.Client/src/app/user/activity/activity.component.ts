import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
import { switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Unit } from '../models/Unit';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit {

  activity: ActivityDetails;
  names = new Map<number, string>();
  public get units() {
    return Unit;
  }

  constructor(private route: ActivatedRoute, private http: HttpClient) { }

  ngOnInit() {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => this.http.get<ActivityDetails>(`activity/${params.get('id')}`))
    ).subscribe(activity => {
      activity.participants.forEach(p => this.names.set(p.userId, p.name));
      this.activity = activity;
    }, error => alert('failed to get activity: ' + error));
  }

}
