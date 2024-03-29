import { ErrorStateMatcher } from '@angular/material/core';
import { UntypedFormControl, FormGroupDirective, NgForm } from '@angular/forms';

export class ParentErrorStateMatcher implements ErrorStateMatcher {

  constructor(private errorName?: string) {

  }

  isErrorState(control: UntypedFormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const invalidCtrl = !!(control && control.invalid && control.parent.dirty);
    const invalidParent = !!(control && control.parent
      && (control.parent.invalid && (!this.errorName || control.parent.hasError(this.errorName)))
      && control.parent.dirty);

    return (invalidCtrl || invalidParent);
  }
}
