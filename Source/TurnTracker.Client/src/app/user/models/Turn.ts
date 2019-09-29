import { DateTime } from 'luxon';

export class Turn {
    public id: number;
    public userId: number;
    public occurred: DateTime;
    public created: DateTime;
    public creatorId: number;
    public isDisabled: boolean;
    public modifierId?: number;
    public modified: DateTime;
}
