import { Participant } from '../models/Participant';

export interface TakeTurnDialogConfig {
  activityId: string;
  participants: Participant[];
  myUserId: number;
}
