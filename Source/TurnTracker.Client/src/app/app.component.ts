import { Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { MatSidenav } from '@angular/material';
import { ProfileService } from './services/profile.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  @ViewChild('sidenav', {static: true}) sidenav: MatSidenav;

  constructor(router: Router, public profileService: ProfileService) {
    router.events.subscribe(event => {
      // close sidenav on routing
      this.sidenav.close();
    });
  }
}
