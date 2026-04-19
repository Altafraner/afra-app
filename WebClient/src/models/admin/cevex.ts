import type { UserInfoMinimal } from '../user/userInfoMinimal';

export interface CevexInformation {
    available: CevexEntity[];
    matches: CevexMatch[];
}

export interface CevexEntity {
    id?: string;
    lastName?: string;
    firstName?: string;
}

export interface CevexMatch {
    user: UserInfoMinimal;
    cevex?: CevexEntity;
}

export interface CevexChangeRequest {
    userId: string;
    cevexId: string;
}
