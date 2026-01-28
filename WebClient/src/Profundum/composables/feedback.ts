import { useToast } from 'primevue';
import { mande, type MandeError } from 'mande';
import type {
    AnkerChangeRequest,
    AnkerOverview,
    FeedbackKategorieChangeRequest,
    ProfundumFeedbackStatus,
} from '@/Profundum/models/feedback';
import { formatMachineDate } from '@/helpers/formatters';

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

    async function createKategorie(
        label: string,
        kategorien: string[],
        isFachlich: boolean,
    ): Promise<void> {
        try {
            const data: FeedbackKategorieChangeRequest = {
                label,
                kategorien,
                isFachlich,
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
        isFachlich: boolean,
    ): Promise<void> {
        try {
            const data: FeedbackKategorieChangeRequest = {
                label,
                kategorien,
                isFachlich,
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

    async function getControl(): Promise<Record<string, ProfundumFeedbackStatus[]>> {
        try {
            return await api.get<Record<string, ProfundumFeedbackStatus[]>>('/control/status');
        } catch (e) {
            const mandeError: MandeError = e as MandeError;
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Übersicht konnte nicht abgerufen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
        }
    }

    function downloadForStudent(
        studentId: string,
        schuljahr: number,
        halbjahr: boolean,
        ausgabedatum: Date,
    ): string | undefined {
        try {
            ausgabedatum.setHours(12);
            const url = `/api/profundum/bewertung/${studentId}.pdf?schuljahr=${schuljahr}&halbjahr=${halbjahr}&ausgabedatum=${formatMachineDate(ausgabedatum)}`;
            const a = document.createElement('a');
            a.href = url;
            a.download = '';
            a.rel = 'noopener';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            return url;
        } catch (e) {
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Das herunterladen ist fehlgeschlagen`,
            });
            console.error(e);
        }
    }

    function downloadForAll(
        schuljahr: number,
        halbjahr: boolean,
        single: boolean,
        byGm: boolean,
        byClass: boolean,
        ausgabedatum: Date,
        doublesided: boolean,
    ): string | undefined {
        try {
            ausgabedatum.setHours(12);
            const url = `/api/profundum/bewertung/batch.zip?schuljahr=${schuljahr}&halbjahr=${halbjahr}&single=${single}&byGm=${byGm}&byClass=${byClass}&ausgabedatum=${formatMachineDate(ausgabedatum)}&doublesided=${doublesided}`;
            const a = document.createElement('a');
            a.href = url;
            a.download = '';
            a.rel = 'noopener';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            return url;
        } catch (e) {
            toast.add({
                severity: 'error',
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Das herunterladen ist fehlgeschlagen`,
            });
            console.error(e);
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
        getControl,
        downloadForStudent,
        downloadForAll,
    };
};
