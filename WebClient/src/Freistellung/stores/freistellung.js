import { defineStore } from 'pinia';
import { mande } from 'mande';

export const useFreistellungStore = defineStore('freistellung', {
    state: () => ({
        meineAntraege: null,
        lehrerAntraege: null,
        sekretariatAntraege: null,
        schulleiterAntraege: null,
        lehrer: null,
    }),
    actions: {
        async updateMeineAntraege() {
            const api = mande('/api/freistellung/sus');
            try {
                this.meineAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching Freistellungsanträge', error);
            }
        },
        async updateLehrerAntraege() {
            const api = mande('/api/freistellung/lehrer');
            try {
                this.lehrerAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching Lehrer Freistellungsanträge', error);
            }
        },
        async updateSekretariatAntraege() {
            const api = mande('/api/freistellung/sekretariat');
            try {
                this.sekretariatAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching Sekretariat Freistellungsanträge', error);
            }
        },
        async updateSchulleiterAntraege() {
            const api = mande('/api/freistellung/schulleiter');
            try {
                this.schulleiterAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching Schulleiter Freistellungsanträge', error);
            }
        },
        async updateLehrer() {
            if (this.lehrer) return;
            const api = mande('/api/freistellung/lehrer-liste');
            try {
                this.lehrer = await api.get();
            } catch (error) {
                console.error('Error fetching Lehrer', error);
            }
        },
    },
});
