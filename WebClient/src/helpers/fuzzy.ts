const normalize = (s: string) =>
    s
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase();

function subsequenceScore(query: string, target: string): number | null {
    let score = 0;
    let targetIndex = 0;
    let consecutive = 0;
    for (const ch of query) {
        const foundIndex = target.indexOf(ch, targetIndex);
        if (foundIndex === -1) return null;
        consecutive = foundIndex === targetIndex ? consecutive + 1 : 1;
        score += consecutive;
        targetIndex = foundIndex + 1;
    }
    if (target.startsWith(query)) score += query.length;
    return score;
}

export function fuzzyMatch(query: string, haystack: string): number | null {
    const tokens = normalize(query).split(/\s+/).filter(Boolean);
    if (tokens.length === 0) return 0;

    const target = normalize(haystack);
    let total = 0;
    for (const token of tokens) {
        const tokenScore = subsequenceScore(token, target);
        if (tokenScore === null) return null;
        total += tokenScore;
    }
    return total;
}
