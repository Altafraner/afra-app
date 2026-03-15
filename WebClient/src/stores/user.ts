import { defineStore } from 'pinia';

export const useUser = defineStore('user', {
    state: () => ({
        loading: true,
        loggedIn: false,
        user: null,
    }),
    getters: {
        isStudent: (state) =>
            state.user.rolle === 'Oberstufe' || state.user.rolle === 'Mittelstufe',
        isMittelstufe: (state) => state.user.rolle === 'Mittelstufe',
        isTeacher: (state) => state.user.rolle === 'Tutor',
        isOtiumsverantwortlich: (state) =>
            state.user.berechtigungen.includes('Otiumsverantwortlich'),
        isProfundumsverantwortlich: (state) =>
            state.user.berechtigungen.includes('Profundumsverantwortlich'),
        isAdmin: (state) => state.user.berechtigungen.includes('Admin'),
        isImpersonating: (state) => state.user?.impersonationId != null,
    },
});
