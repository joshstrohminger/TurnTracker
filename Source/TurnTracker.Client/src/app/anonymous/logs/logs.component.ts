import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, filter, takeUntil } from 'rxjs/operators';
import { ISavedLog, LogLevel, LogService } from 'src/app/services/log.service';
import { DateTime } from 'luxon';
import { ImmediateErrorStateMatcher } from 'src/app/validators/ImmediateErrorStateMatcher';
import { MessageService } from 'src/app/services/message.service';
import { MatAccordion } from '@angular/material/expansion';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})
export class LogsComponent implements OnInit, OnDestroy {

  @ViewChild(MatAccordion) accordion: MatAccordion;

  private readonly unsubscribe$ = new Subject<void>();
  configForm: FormGroup<{enabled: FormControl<boolean>, limit: FormControl<number>}>;
  readonly immediateErrors = new ImmediateErrorStateMatcher();

  public busy = false;

  public get LogLevels() {
    return LogLevel;
  }

  public get logs(): ISavedLog[] {
    return this._logService.logs;
  }

  public get min(): number {
    return this._logService.MinLimit;
  }

  public get max(): number {
    return this._logService.MaxLimit;
  }

  public get canShare(): boolean {
    return !!navigator.canShare;
  }

  constructor(private _logService: LogService, private _builder: FormBuilder, private _messageService: MessageService) { }

  ngOnInit(): void {
    this.configForm = this._builder.group({
      enabled: [this._logService.enabled],
      limit: [this._logService.limit, Validators.compose([Validators.required, Validators.min(1), Validators.max(1000)])]
    });

    if (!this._logService.enabled) {
      this.configForm.controls.limit.disable();
    }

    this.configForm.controls.enabled.valueChanges
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe(enabled => {
        this._logService.enabled = enabled;
        this.configForm.controls.limit[enabled ? 'enable' : 'disable']();
      });

    this.configForm.controls.limit.valueChanges
      .pipe(takeUntil(this.unsubscribe$), filter(() => this.configForm.controls.limit.valid), debounceTime(100))
      .subscribe(limit => this._logService.limit = limit);
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  download(): void {
    if (this.busy || !this.logs.length) {
      return;
    }
    this.busy = true;

    const a = document.createElement("a");
    const file = this.getLogBlob();
    a.href = URL.createObjectURL(file);
    a.download = this.getLogFilename(false);
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(a.href), 1500);
    this.busy = false;
  }

  share(): void {
    if (this.busy || !this.logs.length || !this.canShare) {
      return;
    }
    this.busy = true;

    const blob = this.getLogBlob();
    const file = new File([blob], this.getLogFilename(true), {type: blob.type});
    const content = {
      title: 'TurnTracker Client Log File',
      text: `${this.logs.length} log entries from ${DateTime.now().toLocaleString(DateTime.DATETIME_MED_WITH_SECONDS)}.`,
      files: [file]
    };

    if (navigator.canShare(content)) {
      navigator.share(content)
        .then(() => {
          this.busy = false;
          this._messageService.success('Shared logs')
        })
        .catch(e => {
          this.busy = false;
          if (e instanceof DOMException || e instanceof TypeError) {
            this._messageService.error(`Failed to share: ${e.message}`, e);
          } else {
            this._messageService.error('Failed to share', e);
          }
        });
    } else {
      this._messageService.error('Browser did not allow sharing');
      this.busy = false;
    }
  }

  private getLogBlob(): Blob {
    const shareableLogs = this.logs.map(log => {
      const share = {...log};
      share.params = log.params.map(param => JSON.parse(param));
      return share;
    });
    return new Blob([JSON.stringify(shareableLogs, null, 2)], {type: "text/plain"});
  }

  private getLogFilename(sharing: boolean): string {
    return `turn-tracker-logs.${DateTime.now().toFormat('yyyy-LL-dd-HHmmss')}.${sharing ? 'txt' : 'json'}`;
  }
}
