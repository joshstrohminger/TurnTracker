import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, filter, takeUntil } from 'rxjs/operators';
import { ISavedLog, LogLevel, LogService } from 'src/app/services/log.service';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})
export class LogsComponent implements OnInit, OnDestroy {

  private readonly unsubscribe$ = new Subject<void>();
  configForm: FormGroup

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

  constructor(private _logService: LogService, private _builder: FormBuilder) { }

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

}
