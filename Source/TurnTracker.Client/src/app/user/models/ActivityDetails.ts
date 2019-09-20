import { Unit } from './Unit';
import { Turn } from './Turn';
import { Participant } from './Participant';

export class ActivityDetails {
    public id: number;
    public name: string;
    public hasDisabledTurns: boolean;
    public due?: string;
    public period?: string;
    public periodUnit?: Unit;
    public periodCount?: number;
    public ownerName: string;
    public participants: Participant[];
    public turns: Turn[];
}
