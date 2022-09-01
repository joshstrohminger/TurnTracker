import { Pipe, PipeTransform } from '@angular/core';
import { DateTime } from 'luxon';

@Pipe({
  name: 'luxon'
})
export class LuxonPipe implements PipeTransform {

  transform(value: DateTime, ...args: unknown[]): string {
    if (DateTime.isDateTime(value) && value.isValid) {
      return value.toLocaleString(DateTime.DATETIME_MED_WITH_SECONDS);
    }
  }

}
