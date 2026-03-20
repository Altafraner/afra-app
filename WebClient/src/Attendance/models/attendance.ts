import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

export type AttendanceState = 'Anwesend' | 'Entschuldigt' | 'Fehlend';

export interface AttendanceNote {
    created: string;
    changed: string;
    content: string;
    creator: UserInfoMinimal;
}

export interface AttendanceStudentStatus {
    student: UserInfoMinimal;
    status: AttendanceState;
    notes: AttendanceNote[];
}

export interface AttendanceEventWithEnrollments {
    eventId: string;
    name: string;
    location: string;
    enrollments: AttendanceStudentStatus[];
    status: boolean;
}

export interface AttendanceEvent {
    eventId: string;
    name: string;
    location: string;
}

export interface AttendanceSlot {
    scope: string;
    slotId: string;
    label: string;
}
