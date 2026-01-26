import { useToast } from 'primevue';
import { mande, type MandeError } from 'mande';
import type { QuartalEnrollmentOverview } from '@/Profundum/models/feedback';
import type { ProfundumFachbereich, ProfundumSlot } from '@/Profundum/models/verwaltung';

export const useManagement = () => {
    const toast = useToast();
    const api = mande('/api/profundum/management');

    async function getAllQuartaleWithEnrollments(): Promise<
        QuartalEnrollmentOverview[] | null
    > {
        try {
            return await api.get('/feedback/belegung');
        } catch (e) {
            const mandeError: MandeError = e;
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getFachbereiche(): Promise<ProfundumFachbereich[]> {
        try {
            return await api.get('/fachbereich');
        } catch (e) {
            const mandeError: MandeError = e;
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die verfügbaren Kategorien der Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getSlots(): Promise<ProfundumSlot[]> {
        try {
            return await api.get('/slot');
        } catch (e) {
            const mandeError: MandeError = e;
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die verfügbaren Slots der Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    return { getAllQuartaleWithEnrollments, getFachbereiche, getSlots };
};
