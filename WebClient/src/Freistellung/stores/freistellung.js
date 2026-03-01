import { defineStore } from 'pinia';
import { mande } from 'mande';

export const useFreistellungStore = defineStore('freistellung', {
    state: () => ({
        meineAntraege: null,
        pendingAntraege: null,
        processedLehrerAntraege: null,
        sekretariatAntraege: null,
        processedSekretariatAntraege: null,
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
        async updatePendingAntraege() {
            const api = mande('/api/freistellung/lehrer');
            try {
                this.pendingAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching pending Freistellungsanträge', error);
            }
        },
        async updateProcessedLehrerAntraege() {
            const api = mande('/api/freistellung/lehrer/bearbeitet');
            try {
                this.processedLehrerAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching processed Freistellungsanträge', error);
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
        async updateProcessedSekretariatAntraege() {
            const api = mande('/api/freistellung/sekretariat/bearbeitet');
            try {
                this.processedSekretariatAntraege = await api.get();
            } catch (error) {
                console.error('Error fetching processed Sekretariat Freistellungsanträge', error);
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
