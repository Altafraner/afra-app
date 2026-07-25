import { useRouter } from 'vue-router';
import { useUser } from '@/stores/user';

export function useLogout() {
    const router = useRouter();
    const user = useUser();
    const toast = useToast();

    async function logout() {
        try {
            await user.logout();
            await router.push('/');
            toast.add({
                color: 'success',
                title: 'Abgemeldet!',
                description: 'Sie wurden erfolgreich abgemeldet.',
                duration: 3000,
            });
        } catch (error) {
            toast.add({
                color: 'error',
                title: 'Fehler!',
                description: 'Sie konnten nicht abgemeldet werden.',
            });
        }
    }

    return { logout };
}
