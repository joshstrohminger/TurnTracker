import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private _snackBar: MatSnackBar) { }

  public error(message: string, error?: any) {
    console.error(message, error);
    this._snackBar.open(message, null, {panelClass: 'error'});
  }

  public info(message: string) {
    console.log(message);
    this._snackBar.open(message, null, {panelClass: 'info'});
  }

  public success(message: string) {
    console.log(message);
    this._snackBar.open(message, null, {panelClass: 'success'});
  }
}
