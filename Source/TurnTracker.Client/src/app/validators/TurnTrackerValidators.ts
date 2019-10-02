import { ValidationErrors, AbstractControl, ValidatorFn } from '@angular/forms';

export class TurnTrackerValidators {

  static newValue(originalValue: any): ValidatorFn {
    return (control: AbstractControl): ValidationErrors => {
      if (control.value === originalValue) {
        return {
          'newvalue': 'Must be a new value'
        };
      }
      return null;
    };
  }

  static nonWhitespace: ValidatorFn = (control: AbstractControl): ValidationErrors => {
    if (control.value) {
      const length = ('' + control.value).trim().length;
      if (length === 0) {
        return {
          'nonwhitespace': 'Value only contains whitespace'
        };
      }
    }
    return null;
  }
}
