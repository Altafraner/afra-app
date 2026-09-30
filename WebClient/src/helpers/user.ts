export function getAvatarLink(userId: string, size: number | 'original') {
    return `/api/people/${userId}/avatar?size=${size}`;
}

export function getOwnAvatarLink(size: number) {
    return `/api/user/avatar?size=${size}`;
}
