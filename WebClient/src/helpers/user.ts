export function getAvatarLink(userId: string, size: number | 'original') {
    return `/api/people/${userId}/avatar?size=${size}`;
}

export function getOwnAvatarLink(size: number, cacheId: string) {
    return `/api/user/avatar?size=${size}&cacheId=${cacheId}`;
}
