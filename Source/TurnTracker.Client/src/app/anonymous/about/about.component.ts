import { Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { DateTime } from 'luxon';
import { SwUpdateService } from 'src/app/services/sw-update.service';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {

  public buildDate: string;
  public version = environment.version;
  public appName = environment.appName;

  constructor(public swUpdates: SwUpdateService) {
    try {
      const localBuildDate = DateTime.fromHTTP(environment.buildDate);
      this.buildDate = localBuildDate.toLocaleString(DateTime.DATETIME_FULL);
    } catch (error) {
      console.error('Failed to parse build date from HTTP: ' + environment.buildDate, error);
    }

    if(this.swUpdates.isEnabled) {
      this.swUpdates.checkForUpdate();
    }
  }

  ngOnInit() {
  }

}
