import { UserInfoMinimal } from '@/models/user/user.ts';
import { AttendanceState } from '@/Attendance/models/attendance.ts';

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

export interface StudentDashboard {
    weeks: StudentWeek[];
}

export interface StudentWeek {
    monday: string;
    warnings: string[];
    dailyWarnings: Record<string, string[]>;
    events: ScopedDashboardStudentEventDescriptor[];
}

export interface ScopedDashboardStudentEventDescriptor {
    scope: string;
    label?: string;
    start: string;
    slotLabel: string;
    payload: any;
    attendance?: AttendanceState;
    location?: string;
}
