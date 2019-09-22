import { DateTime } from 'luxon';

export class ActivitySummary {
  public id: number;
  public name: string;
  public currentTurnUserId?: number;
  public currentTurnUserDisplayName?: string;
  public due?: string;

  // Properties populated by the client and not the server
  public dueDate?: DateTime;
  public overdue: boolean;
}
