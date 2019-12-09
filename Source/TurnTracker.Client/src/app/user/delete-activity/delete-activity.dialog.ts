import { MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Inject } from '@angular/core';
import { ActivityDetails } from '../models/ActivityDetails';

@Component({
  selector: 'app-delete-activity',
  templateUrl: './delete-activity.dialog.html',
  styleUrls: ['./delete-activity.dialog.scss']
})
export class DeleteActivityDialog implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public activity: ActivityDetails) {
  }

  ngOnInit() {
  }

}
