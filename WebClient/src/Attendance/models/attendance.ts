import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import type { Note } from '@/Attendance/models/note.ts';

export type AttendanceState = 'Anwesend' | 'Entschuldigt' | 'Fehlend';
export type AttendanceEntryType = 'Automatic' | 'Manual';

export interface AttendanceStudentStatus {
    student: UserInfoMinimal;
    status: AttendanceState;
    notes: Note[];
    type: AttendanceEntryType;
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
