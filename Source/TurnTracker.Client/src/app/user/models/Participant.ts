import { NotificationSetting } from './NotificationSetting';

export enum VerificationStatus {
    None,
    Pending,
    Expired,
    Verified
}

export class Participant {
    public id: number;
    public userId: number;
    public name: string;
    public turnsNeeded: number;
    public hasDisabledTurns: boolean;
    public turnOrder: number;
    public notificationSettings: NotificationSetting[];
    public mobileNumberVerification: VerificationStatus;
    public emailVerification: VerificationStatus;
}
