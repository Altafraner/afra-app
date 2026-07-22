import { mande, type MandeError } from 'mande';
import type { QuartalEnrollmentOverview } from '@/Profundum/models/feedback';
import type {
    ProfundumFachbereich,
    ProfundumSlot,
    ProfundumTerminInstanceInfo,
} from '@/Profundum/models/verwaltung';

export const useManagement = () => {
    const toast = useToast();
    const api = mande('/api/profundum/management');

    async function getAllQuartaleWithEnrollments(): Promise<
        QuartalEnrollmentOverview[] | null
    > {
        try {
            return await api.get('/feedback/belegung');
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                color: 'error',
                title: 'Es ist ein Fehler aufgetreten',
                description: `Die Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getFachbereiche(): Promise<ProfundumFachbereich[] | null> {
        try {
            return await api.get('/fachbereich');
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                color: 'error',
                title: 'Es ist ein Fehler aufgetreten',
                description: `Die verfügbaren Kategorien der Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getSlots(): Promise<ProfundumSlot[] | null> {
        try {
            return await api.get('/slot');
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                color: 'error',
                title: 'Es ist ein Fehler aufgetreten',
                description: `Die verfügbaren Slots der Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getTerminInstanceInfo(
        terminId: string,
        instanceId: string,
    ): Promise<ProfundumTerminInstanceInfo | null> {
        const api = mande('/api/profundum/attendance/');
        try {
            return await api.get<ProfundumTerminInstanceInfo>(`${instanceId}/${terminId}`);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                color: 'error',
                title: 'Es ist ein Fehler aufgetreten',
                description: `Der Termin konnte nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    return { getAllQuartaleWithEnrollments, getFachbereiche, getSlots, getTerminInstanceInfo };
};
