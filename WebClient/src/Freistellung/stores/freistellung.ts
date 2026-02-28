import { defineStore } from 'pinia';
import { mande } from 'mande';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';
import type { UserInfoMinimal } from '@/models/user/user';

/** Fetches `path` into `store[key]`, logging (rather than throwing) on failure. */
async function fetchInto<S, K extends keyof S>(
    store: S,
    key: K,
    path: string,
    label: string,
): Promise<void> {
    const api = mande(path);
    try {
        store[key] = (await api.get<S[K]>()) as S[K];
    } catch (error) {
        console.error(`Error fetching ${label}`, error);
    }
}

export const useFreistellungStore = defineStore('freistellung', {
    state: () => ({
        meineAntraege: null as Freistellungsantrag[] | null,
        lehrerAntraege: null as Freistellungsantrag[] | null,
        sekretariatAntraege: null as Freistellungsantrag[] | null,
        schulleiterAntraege: null as Freistellungsantrag[] | null,
        lehrer: null as UserInfoMinimal[] | null,
        offeneAnzahl: 0,
    }),
    actions: {
        updateOffeneAnzahl() {
            return fetchInto(
                this,
                'offeneAnzahl',
                '/api/freistellung/offene-anzahl',
                'offene Freistellungsanträge',
            );
        },
        updateMeineAntraege() {
            return fetchInto(
                this,
                'meineAntraege',
                '/api/freistellung/sus',
                'Freistellungsanträge',
            );
        },
        updateLehrerAntraege() {
            return fetchInto(
                this,
                'lehrerAntraege',
                '/api/freistellung/lehrer',
                'Lehrer Freistellungsanträge',
            );
        },
        updateSekretariatAntraege() {
            return fetchInto(
                this,
                'sekretariatAntraege',
                '/api/freistellung/sekretariat',
                'Sekretariat Freistellungsanträge',
            );
        },
        updateSchulleiterAntraege() {
            return fetchInto(
                this,
                'schulleiterAntraege',
                '/api/freistellung/schulleiter',
                'Schulleiter Freistellungsanträge',
            );
        },
        async updateLehrer() {
            if (this.lehrer) return;
            await fetchInto(this, 'lehrer', '/api/teachers', 'Lehrer');
        },
        refreshMeineAntraege() {
            this.meineAntraege = null;
            return this.updateMeineAntraege();
        },
        refreshLehrerAntraege() {
            this.lehrerAntraege = null;
            return this.updateLehrerAntraege();
        },
        refreshSekretariatAntraege() {
            this.sekretariatAntraege = null;
            return this.updateSekretariatAntraege();
        },
        refreshSchulleiterAntraege() {
            this.schulleiterAntraege = null;
            return this.updateSchulleiterAntraege();
        },
    },
});
