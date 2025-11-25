import type { UserInfoMinimal } from '@/models/user/userInfoMinimal.ts';

export interface Note {
    id: string;
    content: string;
    created: string;
    changed: string;
    person: UserInfoMinimal;
}

export interface NoteCreationRequest {
    content: string;
    blockId: string;
    studentId: string;
}
