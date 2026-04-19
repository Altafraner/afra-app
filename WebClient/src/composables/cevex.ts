import { mande, type MandeError } from 'mande';
import type { CevexChangeRequest, CevexEntity, CevexInformation } from '../models/admin/cevex';
import { useToast } from 'primevue';
import type { UserInfoMinimal } from '../models/user/userInfoMinimal';

export function useCevex() {
    const toast = useToast();
    const api = mande('/api/people/cevex');

    async function getInformation(): Promise<CevexInformation> {
        try {
            return await api.get<CevexInformation>();
        } catch (e) {
            const mandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Fehler beim Laden der Cevex-Informationen',
                detail: `Entweder ist das Cevex-Modul deaktiviert oder es ist ein unerwarteter Fehler aufgetreten. \n Fehlercode: ${mandeError?.response?.status ?? 'unbekannt'}`,
            });
            throw e;
        }
    }

    async function setMatch(user: UserInfoMinimal, cevex: CevexEntity): Promise<void> {
        try {
            const data: CevexChangeRequest = {
                userId: user.id,
                cevexId: cevex.id!,
            };
            await api.post(data);
        } catch (e) {
            const mandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Fehler beim Aktualisieren der Cevex-Informationen',
                detail: `Entweder ist das Cevex-Modul deaktiviert oder es ist ein unerwarteter Fehler aufgetreten. \n Fehlercode: ${mandeError?.response?.status ?? 'unbekannt'}`,
            });
            throw e;
        }
    }

    return { getInformation, setMatch };
}
