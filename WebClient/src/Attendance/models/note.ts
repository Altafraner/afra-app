import type { UserInfoMinimal } from '@/models/user/user';

export interface Note {
    id: string;
    content: string;
    created: string;
    changed: string;
    creator: UserInfoMinimal;
}

export interface NoteCreationRequest {
    content: string;
    scope: string;
    slotId: string;
    studentId: string;
}
