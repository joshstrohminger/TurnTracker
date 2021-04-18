import { User } from './User';
import { Unit } from './Unit';
import { NotificationSetting } from './NotificationSetting';

export class EditableActivity {
    public id: number;
    public name: string;
    public description: string;
    public isDisabled: boolean;
    public periodUnit?: Unit;
    public periodCount?: number;
    public takeTurns: boolean;
    public participants: User[];
    public defaultNotificationSettings: NotificationSetting[];
}
