import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ActivityDetails } from '../models/ActivityDetails';
import { switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Unit } from '../models/Unit';
import { DateTime } from 'luxon';
import { AuthService } from 'src/app/auth/auth.service';
import { NewTurn } from '../models/NewTurn';

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html',
  styleUrls: ['./activity.component.scss']
})
export class ActivityComponent implements OnInit {

  activity: ActivityDetails;
  busy = false;
  names = new Map<number, string>();
  myUserId: number;
  public get units() {
    return Unit;
  }

  constructor(private route: ActivatedRoute, private http: HttpClient, private authService: AuthService) { }

  ngOnInit() {
    if (this.authService.isLoggedIn) {
      this.myUserId = this.authService.currentUser.id;
    }
    this.refreshActivityUnsafe();
  }

  takeTurnWithOptions() {
    if (this.busy) {
      return;
    }
    this.busy = true;
    console.log('taking turn with options');
    this.busy = false;
  }

  takeTurn(forUserId?: number) {
    if (this.busy) {
      return;
    }
    this.busy = true;

    const turn = <NewTurn>{
      activityId: this.activity.id,
      forUserId: forUserId || this.myUserId,
      when: DateTime.local()
    };

    this.http.post('turn', turn)
      .subscribe(success => this.refreshActivityUnsafe(), error => {
        this.busy = false;
        console.error('failed to take turn', error);
        alert('failed to take turn');
      });
  }

  refreshActivity() {
    if (this.busy) {
      return;
    }
    this.refreshActivityUnsafe();
  }

  private refreshActivityUnsafe() {
    this.busy = true;

    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => this.http.get<ActivityDetails>(`activity/${params.get('id')}`))
    ).subscribe(activity => {
      this.busy = false;
      activity.participants.forEach(p => this.names.set(p.userId, p.name));
      this.activity = activity;
    }, error => {
      alert('failed to get activity: ' + error);
      this.busy = false;
    });
  }
}
