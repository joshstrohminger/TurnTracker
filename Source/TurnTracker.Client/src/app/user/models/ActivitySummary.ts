import { DateTime } from 'luxon';

export class ActivitySummary {
  public id: number;
  public name: string;
  public description: string;
  public currentTurnUserId?: number;
  public currentTurnUserDisplayName?: string;
  public due?: string;
  public isDisabled: boolean;

  // Properties populated by the client and not the server
  public dueDate?: DateTime;
  public overdue: boolean;
}
