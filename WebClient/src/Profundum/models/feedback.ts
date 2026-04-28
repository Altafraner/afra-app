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

export interface MenteeFeedback {
    person: UserInfoMinimal;
    feedback: StudentFeedbackHierarchie;
}

export interface StudentFeedbackHierarchie {
    enrollments: { [key: string]: FeedbackEnrollmentInfo };
    kategorien: FeedbackKategorieGroup[];
}

export interface FeedbackEnrollmentInfo {
    slot: ProfundumSlot;
    profundum: string;
}

export interface FeedbackKategorieGroup {
    id: string;
    label: string;
    isFachlich: boolean;
    anker: FeedbackAnkerGroup[];
}

export interface FeedbackAnkerGroup {
    id: string;
    label: string;
    ratingsBySlot: ratingBySlot;
}

export type rating = 1 | 2 | 3 | 4;

export type ratingBySlot = { [key: string]: rating };
