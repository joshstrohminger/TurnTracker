import { Component, OnInit, Inject } from '@angular/core';
import { Turn } from '../models/Turn';
import { MAT_DIALOG_DATA } from '@angular/material';
import { TurnDetailsDialogConfig } from './TurnDetailsDialogConfig';

@Component({
  selector: 'app-turn-details',
  templateUrl: './turn-details.dialog.html',
  styleUrls: ['./turn-details.dialog.scss']
})
export class TurnDetailsDialog implements OnInit {

  turn: Turn;
  names: Map<number, string>;

  constructor(@Inject(MAT_DIALOG_DATA) config: TurnDetailsDialogConfig) {
    this.names = config.names;
    this.turn = config.turn;
    console.log('details', this.turn, this.names);
  }

  ngOnInit() {
  }

}
