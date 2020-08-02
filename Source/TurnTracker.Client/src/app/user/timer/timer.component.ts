import { Component, OnInit, Input, HostBinding, OnDestroy, Output } from '@angular/core';
import { DateTime, Duration } from 'luxon';

@Component({
  selector: 'app-timer',
  templateUrl: './timer.component.html',
  styleUrls: ['./timer.component.scss']
})
export class TimerComponent implements OnInit, OnDestroy {

  @Input() due: DateTime;
  @Input() appendLabel: boolean;
  @Input() set enabled(enable: boolean) {
    if (!this.initialized) {
      this._enabled = enable;
    } else if (!this._enabled && enable) {
      this.startTimer();
    } else if (this._enabled && !enable) {
      this.stopTimer();
    }
  }
  @Input() set maxTimeComponents(max: number) {
    this.yearFormat = this.trimTimeComponents(this.longYearFormat, max);
    this.monthFormat = this.trimTimeComponents(this.longMonthFormat, max);
    this.dayFormat = this.trimTimeComponents(this.longDayFormat, max);
    this.hourFormat = this.trimTimeComponents(this.longHourFormat, max);
    this.minuteFormat = this.trimTimeComponents(this.longMinuteFormat, max);
    this.secondFormat = this.trimTimeComponents(this.longSecondFormat, max);

    if (this.initialized && !this.timerHandle) {
      this.tick();
    }
  }

  readonly longYearFormat = `y'y' M'mo' d'd' h'h' m'm' s's'`;
  yearFormat = this.longYearFormat;
  readonly longMonthFormat = `M'mo' d'd' h'h' m'm' s's'`;
  monthFormat = this.longMonthFormat;
  readonly longDayFormat = `d'd' h'h' m'm' s's'`;
  dayFormat = this.longDayFormat;
  readonly longHourFormat = `h'h' m'm' s's'`;
  hourFormat = this.longHourFormat;
  readonly longMinuteFormat = `m'm' s's'`;
  minuteFormat = this.longMinuteFormat;
  readonly longSecondFormat = `s's'`;
  secondFormat = this.longSecondFormat;

  _enabled = false;
  initialized = false;
  isOverdue = false;
  remaining: string;

  private timerHandle: number;

  constructor() { }

  ngOnInit() {
    if (!DateTime.isDateTime(this.due)) {
      throw new TypeError(`The 'due' attribute is required`);
    }
    if (!this.due.isValid) {
      throw new TypeError(`The 'due' attribute is invalid: ${this.due.invalidExplanation}`);
    }

    if (this._enabled) {
      this.startTimer();
    } else {
      this.tick();
    }
    this.initialized = true;
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  private trimTimeComponents(format: string, max: number): string {
    max = Math.floor(max);

    if (max <= 0) {
      return format;
    } else {
      return format.split(' ').slice(0, max).join(' ');
    }
  }

  private startTimer() {
    if (!this.timerHandle) {
      this.timerHandle = window.setInterval(() => this.tick(), 1000);
      this.tick();
    }
  }

  private stopTimer() {
    if (this.timerHandle) {
      window.clearInterval(this.timerHandle);
      this.timerHandle = undefined;
    }
  }

  private tick() {
    const now = DateTime.local();
    this.isOverdue = now > this.due;
    const diff = this.due.diff(now);
    this.remaining = this.getFormat(this.isOverdue ? diff.negate() : diff);
  }

  private getFormat(diff: Duration): string {
    const d = diff.shiftTo('years', 'months', 'days', 'hours', 'minutes', 'seconds');
    if (d.years) {
      return d.toFormat(this.yearFormat);
    }
    if (d.months) {
      return d.toFormat(this.monthFormat);
    }
    if (d.days) {
      return d.toFormat(this.dayFormat);
    }
    if (d.hours) {
      return d.toFormat(this.hourFormat);
    }
    if (d.minutes) {
      return d.toFormat(this.minuteFormat);
    }
    return d.toFormat(this.secondFormat);
  }
}
