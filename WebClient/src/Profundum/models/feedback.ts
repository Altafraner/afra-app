import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import type { ProfundumKategorie } from './verwaltung';

export interface Anker {
    id: string;
    label: string;
    kategorieId: string;
}

export interface AnkerChangeRequest {
    label: string;
    kategorieId: string;
}

export interface AnkerOverview {
    ankerByKategorie: Record<string, Anker>;
    kategorien: FeedbackKategorie[];
}

export interface FeedbackKategorie {
    id: string;
    label: string;
    profundumKategorien: ProfundumKategorie[];
}

export interface FeedbackKategorieChangeRequest {
    label: string;
    kategorien: string[];
}

export interface ProfundumEnrollmentOverview {
    id: string;
    label: string;
    students: UserInfoMinimal[];
}

export interface QuartalEnrollmentOverview {
    label: string;
    profunda: ProfundumEnrollmentOverview[];
}
