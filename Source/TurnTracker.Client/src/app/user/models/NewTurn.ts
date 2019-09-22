import { DateTime } from 'luxon';

export class NewTurn {
    public activityId: number;
    public forUserId: number;
    public when: DateTime;

    constructor(activityId: number, forUserId: number, when?: DateTime) {
      this.activityId = activityId;
      this.forUserId = forUserId;
      this.when = when || DateTime.local();
    }
}
