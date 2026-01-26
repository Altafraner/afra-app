export interface UserInfoMinimal {
    id: string;
    vorname: string;
    nachname: string;
    rolle: 'Oberstufe' | 'Mittelstufe' | 'Tutor';
    gruppe: string;
    email: string;
}
