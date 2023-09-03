import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Component, OnInit, Inject } from '@angular/core';

export interface IDangerDialogOptions {
  title?: string;
  action?: string;
  prompt?: string;
}

@Component({
  selector: 'app-danger-dialog',
  templateUrl: './danger.dialog.html',
  styleUrls: ['./danger.dialog.scss']
})
export class DangerDialog implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public options: IDangerDialogOptions) {
  }

  ngOnInit() {
  }

}
