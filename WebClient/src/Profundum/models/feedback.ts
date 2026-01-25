import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import type { ProfundumFachbereich, ProfundumInstanz, ProfundumSlot } from './verwaltung';

type FeedbackStatus = 'Missing' | 'Partial' | 'Done';

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
    ankerByKategorie: Record<string, Anker[]>;
    kategorien: FeedbackKategorie[];
}

export interface FeedbackKategorie {
    id: string;
    label: string;
    fachbereiche: ProfundumFachbereich[];
    isFachlich: boolean;
}

export interface FeedbackKategorieChangeRequest {
    label: string;
    kategorien: string[];
    isFachlich: boolean;
}

export interface ProfundumEnrollmentOverview {
    id: string;
    label: string;
    students: UserInfoMinimal[];
}

export interface QuartalEnrollmentOverview {
    slot: ProfundumSlot;
    profunda: ProfundumEnrollmentOverview[];
}

export interface ProfundumFeedbackStatus {
    instanz: ProfundumInstanz;
    slot: ProfundumSlot;
    status: FeedbackStatus;
}
