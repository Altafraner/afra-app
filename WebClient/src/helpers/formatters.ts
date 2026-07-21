import type { UserInfoMinimal } from '@/models/user/user';
import type { ProfundumSlot } from '@/Profundum/models/verwaltung';
import { CalendarDateTime, getDayOfWeek } from '@internationalized/date';

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

export const formatDate = (date: Date, hideWeekday: boolean = false) =>
    date.toLocaleDateString('de-DE', {
        weekday: hideWeekday ? undefined : 'short',
        day: '2-digit',
        month: 'short',
        year: 'numeric',
    });

export const formatDateTime = (date: Date) =>
    date.toLocaleDateString('de-DE', {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    });

const shortWochentage = ['Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa', 'So'];
export function formatCalendarDateTime(date: CalendarDateTime) {
    const dayOfWeek = getDayOfWeek(date, 'de-DE', 'mon');
    return `${shortWochentage[dayOfWeek]}., ${padNumber(date.day, 2)}.${padNumber(date.month, 2)}.${date.year} ${padNumber(date.hour, 2)}:${padNumber(date.minute, 2)} Uhr`;
}

export function padNumber(number: number, length: number): string {
    const string = number.toString();
    return string.padStart(length, '0');
}

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

export const chooseColorNuxtUi = (
    now: number,
    warnThreshold: number = 70,
    invert: boolean = false,
) => {
    if (now <= warnThreshold) return !invert ? 'success' : 'error';
    if (now < 100) return 'warning';
    return !invert ? 'error' : 'success';
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
