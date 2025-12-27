import { useToast } from 'primevue';
import { mande, type MandeError } from 'mande';
import type {
    AnkerChangeRequest,
    AnkerOverview,
    FeedbackKategorieChangeRequest,
} from '@/Profundum/models/feedback';

export const useFeedback = () => {
    const toast = useToast();
    const api = mande('/api/profundum/bewertung');

    async function getAllAnker(): Promise<AnkerOverview | null> {
        try {
            return await api.get<AnkerOverview>('/anker');
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Anker konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getAnkerForProfundum(profundumId: string): Promise<AnkerOverview> {
        try {
            return await api.get<AnkerOverview>(`/${profundumId}`);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Anker konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return {
                kategorien: [],
                ankerByKategorie: {},
            };
        }
    }

    async function getBewertung(
        studentId: string,
        profundumId: string,
    ): Promise<{ [key: string]: number | null }> {
        try {
            return await api.get<{ [key: string]: number | null }>(
                `/${profundumId}/${studentId}`,
            );
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Bewertung konnte nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return {};
        }
    }

    async function createAnker(label: string, kategorie: string): Promise<void> {
        try {
            const data: AnkerChangeRequest = {
                label: label,
                kategorieId: kategorie,
            };
            await api.post('/anker', data);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Der Anker konnte nicht erstellt werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function updateAnker(id: string, label: string, kategorie: string): Promise<void> {
        try {
            const data: AnkerChangeRequest = {
                label: label,
                kategorieId: kategorie,
            };
            await api.put(`/anker/${id}`, data);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Der Anker konnte nicht geändert werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function deleteAnker(id: string): Promise<void> {
        try {
            await api.delete(`/anker/${id}`);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Der Anker konnte nicht gelöscht werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function createKategorie(label: string, kategorien: string[]): Promise<void> {
        try {
            const data: FeedbackKategorieChangeRequest = {
                label,
                kategorien,
            };
            await api.post(`/kategorie`, data);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Kategorie konnte nicht erstellt werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function updateKategorie(
        id: string,
        label: string,
        kategorien: string[],
    ): Promise<void> {
        try {
            const data: FeedbackKategorieChangeRequest = {
                label,
                kategorien,
            };
            await api.put(`/kategorie/${id}`, data);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Kategorie konnte nicht geändert werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function deleteKategorie(id: string): Promise<void> {
        try {
            await api.delete(`/kategorie/${id}`);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Kategorie konnte nicht gelöscht werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    async function bewertungAbgeben(
        studentId: string,
        profundumId: string,
        bewertungen: { [key: string]: number | null },
    ): Promise<void> {
        try {
            await api.put(`/${profundumId}/${studentId}`, bewertungen);
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Das Feedback konnte nicht eingereicht werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    return {
        getAllAnker,
        getAnkerForProfundum,
        getBewertung,
        createAnker,
        updateAnker,
        deleteAnker,
        createKategorie,
        updateKategorie,
        deleteKategorie,
        bewertungAbgeben,
    };
};
