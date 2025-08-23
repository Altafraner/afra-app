import { defineStore } from 'pinia';
import { mande } from 'mande';

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
    },
    actions: {
        async update() {
            const fetchUser = mande('/api/user');

            const userPromise = fetchUser.get();
            try {
                this.user = await userPromise;
                this.loggedIn = true;
            } catch (error) {
                if (error.response.status === 401) {
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
