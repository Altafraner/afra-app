import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

const wochentage = [
    'Sonntag',
    'Montag',
    'Dienstag',
    'Mittwoch',
    'Donnerstag',
    'Freitag',
    'Samstag',
];
const padString = (text: any, n: number) => String(text).padStart(n, '0');
export const formatTutor = (tutor: UserInfoMinimal) =>
    tutor ? tutor.nachname + ', ' + tutor.vorname : '';

export const formatStudent = (student: UserInfoMinimal) =>
    student.vorname + ' ' + student.nachname;

export const formatPerson = (person: UserInfoMinimal) =>
    person.rolle === 'Oberstufe' || person.rolle === 'Mittelstufe'
        ? formatStudent(person)
        : formatTutor(person);

export const formatDate = (date: Date) =>
    date.toLocaleDateString('de-DE', {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
    });

export const formatDateTime = (date: Date) =>
    date.toLocaleDateString('de-DE', {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        hour: '2-digit',
        minute: '2-digit',
    });

export const formatMachineDate = (date: Date) => date.toISOString().split('T')[0];

export const formatTime = (date: Date) =>
    padString(date.getHours(), 2) + ':' + padString(date.getMinutes(), 2);

export const chooseColor = (now: number, max: number) => {
    if (max === 0 || now <= 0.7) return 'var(--p-button-success-background)';
    if (now < 1) return 'var(--p-button-warn-background)';
    return 'var(--p-button-danger-background)';
};

export const chooseSeverity = (now: number) => {
    if (now <= 70) return 'success';
    if (now < 100) return 'warn';
    return 'danger';
};

export const formatDayOfWeek = (number: number) => wochentage[number % 7];
