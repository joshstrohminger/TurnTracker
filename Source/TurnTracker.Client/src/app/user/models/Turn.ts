import { DateTime } from 'luxon';

export class Turn {
    public id: number;
    public userId: number;
    public occurred: DateTime;
    public creatorId: number;
    public isDisabled: boolean;
    public modifier: string;
    public modified: DateTime;
}
