import { ApplicationRef, Injectable } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { DateTime } from 'luxon';
import { concat, interval, merge, Subject } from 'rxjs';
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

  private checked: DateTime;
  public get lastChecked(): string {
    return this.checked?.toLocaleString(DateTime.DATETIME_FULL) || 'never';
  }

  constructor(private updates: SwUpdate, private messageService: MessageService, appRef: ApplicationRef) {
    if(!this.updates.isEnabled) {
      return;
    }

    this.updates.available.subscribe(_ => {
      this.updateAvailable = true;
      this.messageService.info("There is an update available!");
    });

    this.updates.activated.subscribe(_ => this.messageService.success('Application updated!'));

    // Allow the app to stabilize first, before starting to check periodically or when manually triggered
    const appIsStable$ = appRef.isStable.pipe(first(isStable => isStable === true));
    const everySixHours$ = interval(6 * 60 * 60 * 1000);
    const trigger$ = merge(everySixHours$, this.manualUpdateCheckSubject.asObservable());
    const triggerAfterStable$ = concat(appIsStable$, trigger$);

    triggerAfterStable$.subscribe(() => updates.checkForUpdate().then(
      () => {
        this.checked = DateTime.local();
        console.log(`Checked for an update at ${this.lastChecked}`);
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
    this.updates.activateUpdate().then(() => document.location.reload(), error => {
      this.updating = false;
      this.messageService.error('Failed to update', error);
    });
  }
}
