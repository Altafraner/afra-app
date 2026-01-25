import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import type { ProfundumSlot } from '@/Profundum/models/verwaltung';

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

export const formatStudent = (student: UserInfoMinimal, noBreak: boolean = false) =>
    student.vorname + (noBreak ? '\u00A0' : ' ') + student.nachname;

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

export const chooseSeverity = (
    now: number,
    warnThreshold: number = 70,
    invert: boolean = false,
) => {
    if (now <= warnThreshold) return !invert ? 'success' : 'danger';
    if (now < 100) return 'warn';
    return !invert ? 'danger' : 'success';
};

export const formatDayOfWeek = (number: number) => wochentage[number % 7];

export const formatDayOfWeekFromEnum = (
    day: 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday',
) => {
    if (day === 'Sunday') return formatDayOfWeek(0);
    if (day === 'Monday') return formatDayOfWeek(1);
    if (day === 'Tuesday') return formatDayOfWeek(2);
    if (day === 'Wednesday') return formatDayOfWeek(3);
    if (day === 'Thursday') return formatDayOfWeek(4);
    if (day === 'Friday') return formatDayOfWeek(5);
    if (day === 'Saturday') return formatDayOfWeek(6);
    throw Error(`Unknown day: ${day}`);
};

export const formatSlot = (slot: ProfundumSlot) => {
    return `${slot.jahr} / ${slot.jahr + 1} ${slot.quartal} ${formatDayOfWeekFromEnum(slot.wochentag)}`;
};
