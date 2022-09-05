import { ValidationErrors, AbstractControl, ValidatorFn, UntypedFormGroup } from '@angular/forms';

export class TurnTrackerValidators {

  static newValue(originalValue: any): ValidatorFn {
    return (control: AbstractControl): ValidationErrors => {
      if (control && control.value === originalValue) {
        return {
          'newvalue': 'Must be a new value'
        };
      }
      return null;
    };
  }

  static whitespace: ValidatorFn = (control: AbstractControl): ValidationErrors => {
    if (control.value) {
      const length = ('' + control.value).trim().length;
      if (length === 0) {
        return {
          'whitespace': 'Value only contains whitespace'
        };
      }
    }
    return null;
  }

  static int: ValidatorFn = (control: AbstractControl): ValidationErrors => {
    if (control && !Number.isInteger(control.value)) {
      return {
        'int': 'Value must be an integer'
      };
    }
    return null;
  }

  static minTrimmedLength(min: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors => {
      if (control) {
        const actual = ('' + control.value).trim().length;
        if (actual < min) {
          return {
            'mintrimmedlength': {min, actual}
          };
        }
      }
      return null;
    };
  }

  static different(...controlNames: string[]): ValidatorFn {
    return (group: UntypedFormGroup): ValidationErrors => {
      if (group && controlNames.length >= 2) {
        let firstValue;
        let first = true;
        for (const name of controlNames) {
          const control = group.get(name);
          if (control) {
            const value = control.value;
            if (first) {
              firstValue = value;
              first = false;
            } else if (firstValue !== value) {
              return {
                'different': 'Values must match'
              };
            }
          } else {
            return {
              'different': `Invalid control name '${name}'`
            };
          }
        }
      }
      return null;
    };
  }
}
