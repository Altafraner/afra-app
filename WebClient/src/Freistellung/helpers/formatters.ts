import type {
    FreistellungsStatus,
    EntscheidungsStatus,
} from '@/Freistellung/models/freistellung';

/** Formats an ISO date(-time) string as dd.MM.yyyy. */
export function formatFreistellungDate(dateStr: string): string {
    const d = new Date(dateStr);
    return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
}

/** Formats an ISO date-time string as HH:mm. */
export function formatFreistellungTime(dateStr: string): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}

/**
 * Formats a date range as a human-readable string. If both dates are on the same day, shows a
 * single date; otherwise shows a range including the time of day.
 */
export function formatFreistellungDateRange(von: string, bis: string): string {
    const vonDate = new Date(von).toDateString();
    const bisDate = new Date(bis).toDateString();
    return vonDate === bisDate
        ? formatFreistellungDate(von)
        : `${formatFreistellungDate(von)} ${formatFreistellungTime(von)} – ${formatFreistellungDate(bis)} ${formatFreistellungTime(bis)}`;
}

/** UBadge color for the overall Freistellungsantrag status. */
export const statusColor: Record<FreistellungsStatus, string> = {
    Gestellt: 'info',
    BeiSekretariat: 'warning',
    ElternbestaetigungAusstehend: 'warning',
    ElternbestaetigungEingereicht: 'warning',
    BeimSchulleiter: 'warning',
    SchulleiterBestaetigt: 'success',
    Abgelehnt: 'error',
};

/** Label for the overall Freistellungsantrag status. */
export const statusLabel: Record<FreistellungsStatus, string> = {
    Gestellt: 'Eingereicht',
    BeiSekretariat: 'Beim Sekretariat',
    ElternbestaetigungAusstehend: 'Elternbestätigung ausstehend',
    ElternbestaetigungEingereicht: 'Elternbestätigung eingereicht',
    BeimSchulleiter: 'Beim Schulleiter',
    SchulleiterBestaetigt: 'Genehmigt',
    Abgelehnt: 'Abgelehnt',
};

/** UBadge color for a single Entscheidung (teacher/mentor decision). */
export const entscheidungColor: Record<EntscheidungsStatus, string> = {
    Ausstehend: 'neutral',
    Genehmigt: 'success',
    Abgelehnt: 'error',
};

/** Label for a single Entscheidung (teacher/mentor decision). */
export const entscheidungLabel: Record<EntscheidungsStatus, string> = {
    Ausstehend: 'Ausstehend',
    Genehmigt: 'Genehmigt',
    Abgelehnt: 'Abgelehnt',
};
