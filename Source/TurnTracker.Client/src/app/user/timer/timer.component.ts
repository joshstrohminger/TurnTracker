import { Component, OnInit, Input, HostBinding, OnDestroy, Output } from '@angular/core';
import { DateTime, Duration } from 'luxon';

@Component({
  selector: 'app-timer',
  templateUrl: './timer.component.html',
  styleUrls: ['./timer.component.scss']
})
export class TimerComponent implements OnInit, OnDestroy {

  @Input() due: DateTime;

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

    this.timerHandle = window.setInterval(() => this.tick(), 1000);
    this.tick();
  }

  ngOnDestroy(): void {
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
      return d.toFormat(`y'y' M'm' d'd' h'h' m'm' s's'`);
    }
    if (d.months) {
      return d.toFormat(`M'm' d'd' h'h' m'm' s's'`);
    }
    if (d.days) {
      return d.toFormat(`d'd' h'h' m'm' s's'`);
    }
    if (d.hours) {
      return d.toFormat(`h'h' m'm' s's'`);
    }
    if (d.minutes) {
      return d.toFormat(`m'm' s's'`);
    }
    return d.toFormat(`s's'`);
  }
}
