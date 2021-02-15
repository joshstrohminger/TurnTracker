import { Participant } from '../models/Participant';

export interface TakeTurnDialogConfig {
  activityModifedDate: string;
  activityName: string;
  activityId: number;
  participants: Participant[];
  myUserId: number;
}
