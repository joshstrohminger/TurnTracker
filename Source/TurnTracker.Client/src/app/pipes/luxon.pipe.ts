import { Pipe, PipeTransform } from '@angular/core';
import { DateTime } from 'luxon';

export type LuxonFormatName = 'short' | 'medium' | 'long';

@Pipe({
  name: 'luxon'
})
export class LuxonPipe implements PipeTransform {

  transform(value: DateTime, format?: LuxonFormatName): string {
    if (DateTime.isDateTime(value) && value.isValid) {
      return value.toLocaleString(this.getOptions(format));
    }
  }

  private getOptions(format: LuxonFormatName | undefined) {
    switch(format) {
      case 'long':
        return DateTime.DATETIME_FULL_WITH_SECONDS;
      case 'short':
        return DateTime.DATETIME_SHORT_WITH_SECONDS;
      default:
        return DateTime.DATETIME_MED_WITH_SECONDS;
    }
  }

}
