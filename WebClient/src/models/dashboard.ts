import { UserInfoMinimal } from '@/models/user/user.ts';

export interface ScopedDashboardTutorEventDescriptor {
    scope: string;
    label: string;
    start: string;
    slotLabel: string;
    occupancy?: number;
    payload?: any;
}

export type DashboardMenteeStatus = 'NotApplicable' | 'Invalid' | 'Uncertain' | 'Valid';

export interface DashboardMenteeOverview {
    mentee: UserInfoMinimal;
    last: DashboardMenteeStatus;
    current: DashboardMenteeStatus;
    next: DashboardMenteeStatus;
}

export interface TutorDashboard {
    events: ScopedDashboardTutorEventDescriptor[];
    mentees: DashboardMenteeOverview[];
}
