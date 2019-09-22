import { Participant } from '../models/Participant';

export interface TakeTurnDialogConfig {
  activityName: string;
  activityId: number;
  participants: Participant[];
  myUserId: number;
}
