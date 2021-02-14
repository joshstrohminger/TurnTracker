import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MatSidenav } from '@angular/material/sidenav';
import { UserService } from './services/user.service';
import { AuthService } from './auth/auth.service';
import { environment } from '../environments/environment';
import { PushService } from './services/push.service';
import { ActivityService } from './services/activity.service';
import { Title } from '@angular/platform-browser';
import { SwUpdateService } from './services/sw-update.service';

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
    _push: PushService,
    public activityService: ActivityService,
    public swUpdates: SwUpdateService,
    titleService: Title,
    activatedRoute: ActivatedRoute) {

    router.events.subscribe(event => {
      // close sidenav on routing
      this.sidenav.close();

      if (event instanceof NavigationEnd) {

        const childRoute = this.getChild(activatedRoute);
        const subTitle = childRoute.snapshot.data?.title;
        var title = this.appName;
        if(subTitle) {
          title += ` - ${subTitle}`;
        }
        titleService.setTitle(title);
    }
    });
  }

  private getChild(activatedRoute: ActivatedRoute) {
    if (activatedRoute.firstChild) {
      return this.getChild(activatedRoute.firstChild);
    } else {
      return activatedRoute;
    }
  }
}
