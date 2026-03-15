import { useUser } from '@/stores/user';
import { useRoute, useRouter } from 'vue-router';
import { mande } from 'mande';
import { registerLoggedInRoutes, registerLoggedOutRoutes } from '@/router/index';

export function useUserManagement() {
    const userStore = useUser();
    const router = useRouter();
    const route = useRoute();

    async function updateUser() {
        const fetchUser = mande('/api/user');

        const wasLoggedIn = userStore.loggedIn;

        const userPromise = fetchUser.get();
        try {
            userStore.user = await userPromise;
            userStore.loggedIn = true;
            if (!wasLoggedIn) {
                registerLoggedInRoutes(router);
                // we need to force vue router to reevaluate which route we're on since the route for our current url might have changed with logging in.
                await router.replace(route.fullPath);
            }
        } catch (error) {
            if (error.response.status === 401) {
                userStore.loggedIn = false;
                userStore.user = null;
                console.info('Not logged in');
                registerLoggedOutRoutes(router);
            } else {
                console.error('Error fetching user', error);
                throw error;
            }
        } finally {
            userStore.loading = false;
        }
    }

    async function logout() {
        const logoutUser = mande('/api/user/logout');
        await logoutUser.get();
        registerLoggedOutRoutes(router);
        userStore.loggedIn = false;
        userStore.user = null;
        await router.replace(route.fullPath);
    }

    async function impersonateUser(userId: string) {
        await mande(`/api/user/${userId}/impersonate`).get();
        await updateUser();
    }

    return { updateUser, logout, impersonateUser };
}
