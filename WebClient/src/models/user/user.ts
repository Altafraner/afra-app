export type UserRolle = 'Oberstufe' | 'Mittelstufe' | 'Tutor';
export type UserGlobalPermission =
    | 'Admin'
    | 'Otiumsverantwortlich'
    | 'Profundumsverantwortlich'
    | 'Sekretariat'
    | 'Schulleiter';

export interface UserInfoMinimal {
    id: string;
    vorname: string;
    nachname: string;
    rolle: UserRolle;
    gruppe: string;
    email: string;
}

export interface UserLoginInfo {
    id: string;
    vorname: string;
    nachname: string;
    rolle: UserRolle;
    berechtigungen: UserGlobalPermission[];
    impersonationId: string;
}
