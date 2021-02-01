import { NotificationType } from './NotificationType';

export class NotificationSetting {
  public participantId: number;
  public type: NotificationType;
  public sms: boolean;
  public email: boolean;
  public push: boolean;

  public get anyChecked(): boolean {
    return this.sms || this.email || this.push;
  }

  constructor(type: NotificationType) {
    this.type = type;
    this.sms = false;
    this.email = false;
    this.push = false;
  }

  public isAllowed(takeTurns: boolean): boolean {
    switch(this.type) {
      case NotificationType.TurnTakenMine:
      case NotificationType.OverdueMine:
        return takeTurns;
    }
    return true;
  }
}
