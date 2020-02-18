import { Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {

  public env = environment;

  constructor() {
    try {
      const localBuildDate = DateTime.fromHTTP(this.env.buildDate);
      this.env.buildDate = localBuildDate.toLocaleString(DateTime.DATETIME_FULL);
    } catch (error) {
      console.error('Failed to parse build date from HTTP: ' + this.env.buildDate, error);
    }
  }

  ngOnInit() {
  }

}
