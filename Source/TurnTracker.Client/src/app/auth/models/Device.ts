import { DateTime } from 'luxon';
import { Session } from './Session';

export class Device {
    public name: string;
    public created: string;
    public updated: string;
    public id: number;
    public current: boolean;
    public sessions: Session[];

  // Properties populated by the client and not the server
  public createdDate: DateTime;
  public updatedDate: DateTime;
}
