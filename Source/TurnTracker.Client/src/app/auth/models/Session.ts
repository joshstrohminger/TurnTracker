import { DateTime } from 'luxon';

export class Session {
  public name: string;
  public created: string;
  public updated: string;
  public id: number;
  public current: boolean;

  // Properties populated by the client and not the server
  public createdDate: DateTime;
  public updatedDate: DateTime;
}
