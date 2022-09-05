import { ErrorStateMatcher } from '@angular/material/core';
import { UntypedFormControl, FormGroupDirective, NgForm } from '@angular/forms';

export class ImmediateErrorStateMatcher implements ErrorStateMatcher {

  isErrorState(control: UntypedFormControl, form: FormGroupDirective | NgForm): boolean {
    return !!(control && control.invalid);
  }

}
