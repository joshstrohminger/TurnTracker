import { Unit } from './Unit';
import { Turn } from './Turn';
import { Participant } from './Participant';
import { DateTime } from 'luxon';

export class ActivityDetails {
    public id: number;
    public name: string;
    public hasDisabledTurns: boolean;
    public due?: string;
    public periodUnit?: Unit;
    public periodCount?: number;
    public ownerName: string;
    public currentTurnUserId?: number;
    public currentTurnUserDisplayName?: string;
    public participants: Participant[];
    public turns: Turn[];
    public isDisabled: boolean;

    // Properties populated by the client and not the server
    public dueDate?: DateTime;
    public overdue: boolean;
}
