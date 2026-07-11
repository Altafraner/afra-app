import { defineStore } from 'pinia';
import { mande, MandeError } from 'mande';
import { UserLoginInfo } from '@/models/user/user.ts';

export const useUser = defineStore('user', {
    state: () => {
        return {
            loading: true,
            loggedIn: false,
            user: null as UserLoginInfo | null,
        };
    },
    getters: {
        isStudent: (state) =>
            state.user?.rolle === 'Oberstufe' || state.user?.rolle === 'Mittelstufe',
        isMittelstufe: (state) => state.user?.rolle === 'Mittelstufe',
        isTeacher: (state) => state.user?.rolle === 'Tutor',
        isOtiumsverantwortlich: (state) =>
            state.user?.berechtigungen.includes('Otiumsverantwortlich') ?? false,
        isProfundumsverantwortlich: (state) =>
            state.user?.berechtigungen.includes('Profundumsverantwortlich') ?? false,
        isAdmin: (state) => state.user?.berechtigungen.includes('Admin') ?? false,
        isImpersonating: (state) => state.user?.impersonationId != null,
    },
    actions: {
        async update() {
            const fetchUser = mande('/api/user');

            const userPromise = fetchUser.get<UserLoginInfo>();
            try {
                this.user = await userPromise;
                this.loggedIn = true;
            } catch (error) {
                const mandeError = error as MandeError;
                if (mandeError.response?.status === 401) {
                    this.loggedIn = false;
                    this.user = null;
                    console.info('Not logged in');
                } else {
                    console.error('Error fetching user', error);
                    throw error;
                }
            } finally {
                this.loading = false;
            }
        },

        async logout() {
            const logoutUser = mande('/api/user/logout');
            await logoutUser.get();
            this.loggedIn = false;
            this.user = null;
        },
    },
});
