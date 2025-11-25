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
    blockId: string;
    studentId: string;
}
