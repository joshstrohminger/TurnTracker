import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';

@Injectable({
  providedIn: 'root'
})
export class ErrorService {

  constructor(private _snackBar: MatSnackBar) { }

  public show(message: string, error?: any) {
    console.error(message, error);
    this._snackBar.open(message);//, null, { duration: 5000 });
  }
}
