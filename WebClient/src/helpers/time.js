/**
 * Check if current time is in the given time interval on the given date
 * @param {string} date The date the interval is on
 * @param {Object} timeInterval The time interval with start and end in "HH:MM" or "HH:MM:SS" format
 * @returns {boolean} True if current time is in the interval, false otherwise
 */
export function isNowInInterval(date, timeInterval) {
    const [sh, sm, ss] = timeInterval.start.split(':').map(Number);
    const [eh, em, es] = timeInterval.end.split(':').map(Number);
    const now = new Date();
    const start = new Date(date);
    const end = new Date(date);
    start.setHours(sh, sm, ss ?? 0, 0);
    end.setHours(eh, em, es ?? 0, 0);

    return now >= start && now < end;
}

/**
 * Check if current time is in a date-time interval.
 * @param {{ start?: string, end?: string } | undefined | null} interval The interval with ISO-like start/end date-times.
 * @returns {boolean} True if now is in the interval, false otherwise.
 */
export function isNowInDateTimeInterval(interval) {
    if (!interval?.start || !interval?.end) return false;

    const start = new Date(interval.start);
    const end = new Date(interval.end);

    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return false;

    const now = new Date();
    return now >= start && now < end;
}

