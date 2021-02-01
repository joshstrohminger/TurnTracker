import { EventEmitter } from '@angular/core';
import { Component, Input, OnInit, Output } from '@angular/core';
import { NotificationSetting } from '../models/NotificationSetting';
import { NotificationType } from '../models/NotificationType';

@Component({
  selector: 'notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit {
  @Input()
  notifications: NotificationSetting[];
  @Input()
  mobileNumberVerified: boolean;
  @Input()
  emailVerified: boolean;
  @Input()
  disabled: boolean;
  @Input()
  takeTurns: boolean;
  @Output()
  settingChanged = new EventEmitter<NotificationSetting>();

  constructor() { }

  ngOnInit(): void {
  }

  public allowNotificationType(note: NotificationSetting): boolean {
    switch(note.type) {
      case NotificationType.TurnTakenMine:
      case NotificationType.OverdueMine:
        return this.takeTurns;
    }
    return true;
  }

}
