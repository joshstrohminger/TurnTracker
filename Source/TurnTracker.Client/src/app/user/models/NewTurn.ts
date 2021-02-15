import { DateTime } from 'luxon';
import { ActivityDetails } from './ActivityDetails';

export class NewTurn {
    public activityId: number;
    public forUserId: number;
    public when: DateTime;
    public modifiedDate: string;

    constructor(activityModifiedDate: string, activityId: number, forUserId: number, when?: DateTime) {
      this.activityId = activityId;
      this.modifiedDate = activityModifiedDate;
      this.forUserId = forUserId;
      this.when = when || DateTime.local();
    }
}
