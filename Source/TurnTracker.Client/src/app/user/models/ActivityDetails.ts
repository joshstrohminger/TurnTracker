import { Unit } from './Unit';
import { Turn } from './Turn';
import { Participant } from './Participant';
import { DateTime } from 'luxon';

export class ActivityDetails {
    public id: number;
    public name: string;
    public description: string;
    public hasDisabledTurns: boolean;
    public due?: string;
    public periodUnit?: Unit;
    public periodCount?: number;
    public ownerName: string;
    public ownerId: number;
    public currentTurnUserId?: number;
    public currentTurnUserDisplayName?: string;
    public modifiedDate: string;
    public participants: Participant[];
    public turns: Turn[];
    public isDisabled: boolean;
    public takeTurns: boolean;

    // Properties populated by the client and not the server
    public dueDate?: DateTime;
    public overdue: boolean;
}
