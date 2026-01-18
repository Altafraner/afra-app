import { UserInfoMinimal } from '@/models/user/userInfoMinimal';

type ProfundumQuartal = 'Q1' | 'Q2' | 'Q3' | 'Q4';
type Wochentag =
    | 'Sunday'
    | 'Monday'
    | 'Tuesday'
    | 'Wednesday'
    | 'Thursday'
    | 'Friday'
    | 'Saturday';

export interface ProfundumFachbereich {
    id: string;
    label: string;
}

export interface ProfundumInstanz {
    id: string;
    profundumId: string;
    profundumInfo: ProfundumDefinition;
    slots: string[];
    maxEinschreibungen: null | number;
    numEinschreibungen: number;
    verantwortlicheIds: string[];
    verantwortliche: UserInfoMinimal[];
    ort: string;
}

export interface ProfundumDefinition {
    id: string;
    bezeichnung: string;
    beschreibung: string;
    kategorieId: string;
    fachbereiche: ProfundumFachbereich[];
    fachbereichIds: string[];
    minKlasse: null | number;
    maxKlasse: null | number;
    dependencyIds: string[];
}

export interface ProfundumSlot {
    id: string;
    jahr: number;
    quartal: ProfundumQuartal;
    wochentag: Wochentag;
    einwahlZeitraumId: string;
}
