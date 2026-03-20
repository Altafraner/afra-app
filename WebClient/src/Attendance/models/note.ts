import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

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
