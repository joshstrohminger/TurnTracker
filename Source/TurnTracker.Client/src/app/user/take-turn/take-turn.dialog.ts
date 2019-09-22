import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Participant } from '../models/Participant';
import { DateTime } from 'luxon';
import { TakeTurnDialogConfig } from './TakeTurnDialogConfig';

@Component({
  selector: 'app-take-turn',
  templateUrl: './take-turn.dialog.html',
  styleUrls: ['./take-turn.dialog.scss']
})
export class TakeTurnDialog implements OnInit {

  selectedParticipant: Participant;
  participants: Participant[];
  isDateProvided = false;
  when: DateTime;

  constructor(@Inject(MAT_DIALOG_DATA) config: TakeTurnDialogConfig) {
    this.participants = config.participants;
    this.selectedParticipant = this.participants.find(p => p.id === config.myUserId);
    this.when = DateTime.local();
  }

  ngOnInit() {
  }

}
