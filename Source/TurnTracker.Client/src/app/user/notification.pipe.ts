import { Pipe, PipeTransform } from '@angular/core';
import { NotificationType } from './models/NotificationType';

@Pipe({
  name: 'notification'
})
export class NotificationPipe implements PipeTransform {

  transform(type: NotificationType, ...args: any[]): any {
    switch (type) {
      case NotificationType.OverdueAnybody:
        return 'Any turn is overdue';
      case NotificationType.OverdueMine:
        return 'My turn is overdue';
      case NotificationType.TurnTakenAnybody:
        return 'Anybody took a turn';
      case NotificationType.TurnTakenMine:
        return 'Anybody took a turn and I\'m next';
      default:
        return type;
    }
  }

}
