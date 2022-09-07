import { ApplicationRef, Injectable, NgZone } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { DateTime } from 'luxon';
import { BehaviorSubject, concat, interval, merge, Observable, Subject } from 'rxjs';
import { first } from 'rxjs/operators';
import { MessageService } from './message.service';

@Injectable({
  providedIn: 'root'
})
export class SwUpdateService {
  private manualUpdateCheckSubject = new Subject();

  public get isEnabled(): boolean {
    return this.updates.isEnabled;
  }

  private updateAvailable = false;
  public get isUpdateAvailable(): boolean {
    return this.updateAvailable;
  }

  private updating = false;
  public get isUpdating(): boolean {
    return this.updating;
  }

  private lastCheckedSubject = new BehaviorSubject<string>('never');
  public get lastChecked$(): Observable<string> {
    return this.lastCheckedSubject.asObservable();
  }

  constructor(private updates: SwUpdate, private messageService: MessageService, appRef: ApplicationRef, private zone: NgZone) {
    if(!this.updates.isEnabled) {
      return;
    }

    this.updates.versionUpdates.subscribe(e => {
      switch(e.type) {
        case 'VERSION_DETECTED':
          console.log('SW version detected on server', e);
          return;
        case 'VERSION_READY':
          console.log('SW version ready', e);
          this.updateAvailable = true;
          this.messageService.info('There is an update available!');
          return;
        case 'VERSION_INSTALLATION_FAILED':
          console.log('Failed to install version', e);
          return;
      }
    })

    // Allow the app to stabilize first, before starting to check periodically or when manually triggered
    const appIsStable$ = appRef.isStable.pipe(first(isStable => isStable === true));
    const everySixHours$ = interval(6 * 60 * 60 * 1000);
    const trigger$ = merge(everySixHours$, this.manualUpdateCheckSubject.asObservable());
    const triggerAfterStable$ = concat(appIsStable$, trigger$);

    triggerAfterStable$.subscribe(() => updates.checkForUpdate().then(
      () => {
        this.zone.run(() => {
          this.lastCheckedSubject.next(DateTime.local().toLocaleString(DateTime.DATETIME_FULL));
        });
        console.log(`Checked for an update at ${this.lastCheckedSubject.value}`);
      },
      error => console.error(`Failed to check for an update at ${DateTime.local().toLocaleString(DateTime.DATETIME_FULL)}`, error)));
  }

  public checkForUpdate() {
    if(!this.updates.isEnabled) {
      return;
    }

    this.manualUpdateCheckSubject.next();
  }

  public updateNow() {
    if(!this.updates.isEnabled) {
      return;
    }

    this.updating = true;
    this.updates.activateUpdate().then(() => {
      this.messageService.success('Application updated! Reloading...');
      document.location.reload();
    }, error => {
      this.updating = false;
      this.messageService.error('Failed to update', error);
    });
  }
}
