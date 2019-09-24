import { NotificationType } from './NotificationType';

export class NotificationSetting {
  public type: NotificationType;
  public sms: boolean;
  public email: boolean;
  public push: boolean;

  constructor(type: NotificationType) {
    this.type = type;
    this.sms = false;
    this.email = false;
    this.push = false;
  }
}
