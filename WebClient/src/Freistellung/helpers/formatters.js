/**
 * Formats a date string (ISO or similar) as dd.MM.yyyy.
 * @param {string} dateStr
 * @returns {string}
 */
export function formatFreistellungDate(dateStr) {
    const d = new Date(dateStr);
    return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
}

/**
 * Formats a date range as a human-readable string.
 * If both dates are on the same day, shows a single date; otherwise shows a range.
 * @param {string} von
 * @param {string} bis
 * @returns {string}
 */
export function formatFreistellungDateRange(von, bis) {
    const vonDate = new Date(von).toDateString();
    const bisDate = new Date(bis).toDateString();
    return vonDate === bisDate
        ? formatFreistellungDate(von)
        : `${formatFreistellungDate(von)} ${formatFreistellungTime(von)} – ${formatFreistellungDate(bis)} ${formatFreistellungTime(bis)}`;
}

/**
 * Formats a date string as HH:mm.
 * @param {string} dateStr
 * @returns {string}
 */
export function formatFreistellungTime(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}

/** Status severity map for the overall Freistellungsantrag status */
export const statusSeverity = {
    Gestellt: 'info',
    AlleLehrerGenehmigt: 'warn',
    Abgelehnt: 'danger',
    Bestaetigt: 'warn',
    SchulleiterBestaetigt: 'success',
    SekretariatAbgelehnt: 'danger',
    SchulleiterAbgelehnt: 'danger',
};

/** Status label map for the overall Freistellungsantrag status */
export const statusLabel = {
    Gestellt: 'Eingereicht',
    AlleLehrerGenehmigt: 'Alle genehmigt',
    Abgelehnt: 'Abgelehnt',
    Bestaetigt: 'Sekretariat bestätigt',
    SchulleiterBestaetigt: 'Genehmigt',
    SekretariatAbgelehnt: 'Sekretariat abgelehnt',
    SchulleiterAbgelehnt: 'Schulleiter abgelehnt',
};

/** Entscheidung (decision) severity map */
export const entscheidungSeverity = {
    Ausstehend: 'secondary',
    Genehmigt: 'success',
    Abgelehnt: 'danger',
};

/** Entscheidung (decision) label map */
export const entscheidungLabel = {
    Ausstehend: 'Ausstehend',
    Genehmigt: 'Genehmigt',
    Abgelehnt: 'Abgelehnt',
};
