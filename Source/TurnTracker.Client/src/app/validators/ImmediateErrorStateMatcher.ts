import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';

export class ImmediateErrorStateMatcher implements ErrorStateMatcher {

  isErrorState(control: FormControl, form: FormGroupDirective | NgForm): boolean {
    return !!(control && control.invalid);
  }

}
