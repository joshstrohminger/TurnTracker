import { Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { MatSidenav } from '@angular/material/sidenav';
import { UserService } from './services/user.service';
import { AuthService } from './auth/auth.service';
import { environment } from '../environments/environment';
import { PushService } from './services/push.service';
import { ActivityService } from './services/activity.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  @ViewChild('sidenav', {static: true}) sidenav: MatSidenav;
  appName = environment.appName;

  constructor(
    router: Router,
    public userService: UserService,
    public authService: AuthService,
    push: PushService,
    public activityService: ActivityService) {
    router.events.subscribe(event => {
      // close sidenav on routing
      this.sidenav.close();
    });
  }
}
