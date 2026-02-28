import type { UserInfoMinimal } from '@/models/user/user';

export type FreistellungsStatus =
    | 'Eingereicht'
    | 'BeiSekretariat'
    | 'WartetAufEltern'
    | 'ElternbestaetigungEingereicht'
    | 'BeimSchulleiter'
    | 'Abgelehnt'
    | 'Genehmigt'
    | 'Abgeschlossen';

export type EntscheidungsStatus = 'Ausstehend' | 'Genehmigt' | 'Abgelehnt';

export interface BetroffeneStunde {
    id: string;
    datum: string;
    block: number;
    fach: string;
    lehrer: UserInfoMinimal;
}

export interface LehrerEntscheidung {
    id: string;
    lehrer: UserInfoMinimal;
    status: EntscheidungsStatus;
    kommentar: string | null;
    entschiedenAm: string | null;
}

export interface FreistellungsStatistik {
    anzahlAntraegeSchuljahr: number;
    anzahlStundenSchuljahr: number;
}

export interface VerlaufEintrag {
    zeitpunkt: string;
    person: UserInfoMinimal | null;
    neuerStatus: FreistellungsStatus;
    kommentar: string | null;
}

export interface Freistellungsantrag {
    id: string;
    grund: string;
    von: string;
    bis: string;
    beschreibung: string;
    status: FreistellungsStatus;
    erstelltAm: string;
    student: UserInfoMinimal;
    betroffeneStunden: BetroffeneStunde[];
    entscheidungen: LehrerEntscheidung[];
    verlauf: VerlaufEintrag[];
    elternbestaetigungErforderlich: boolean | null;
    elternbestaetigungVorhanden: boolean;
    elternbestaetigungHinweis: string | null;
    schulleiterKommentar: string | null;
    statistik: FreistellungsStatistik;
}

/** The payload sent when a student submits a new leave request. */
export interface CreateFreistellungsantrag {
    grund: string;
    beschreibung: string;
    von: string;
    bis: string;
    stunden: {
        datum: string;
        block: number;
        fach: string;
        lehrerId: string;
    }[];
}
