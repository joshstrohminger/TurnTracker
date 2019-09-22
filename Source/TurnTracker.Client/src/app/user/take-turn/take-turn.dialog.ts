import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Participant } from '../models/Participant';
import { DateTime } from 'luxon';
import { TakeTurnDialogConfig } from './TakeTurnDialogConfig';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NewTurn } from '../models/NewTurn';

@Component({
  selector: 'app-take-turn',
  templateUrl: './take-turn.dialog.html',
  styleUrls: ['./take-turn.dialog.scss']
})
export class TakeTurnDialog implements OnInit {

  participants: Participant[];
  activityName: string;
  turnForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private _config: TakeTurnDialogConfig,
    formBuilder: FormBuilder,
    private _dialog: MatDialogRef<TakeTurnDialogConfig>) {

    this.activityName = _config.activityName;
    this.participants = _config.participants;
    this.turnForm = formBuilder.group({
      forUserId: _config.myUserId,
      when: DateTime.local().toISO({includeOffset: false})
    });
  }

  ngOnInit() {
  }

  takeTurn() {
    let when = DateTime.fromISO(this.turnForm.value.when);
    if (!when.isValid || when > DateTime.local()) {
      when = null;
    }
    const turn = new NewTurn(this._config.activityId, this.turnForm.value.forUserId, when);
    this._dialog.close(turn);
  }
}
