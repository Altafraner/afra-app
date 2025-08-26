import { defineStore } from 'pinia';
import { mande } from 'mande';

export const useOtiumStore = defineStore('otium', {
    state: () => ({
        blocks: null,
        schuljahr: null,
        defaultDay: null,
        kategorien: null,
        personen: null,
    }),
    actions: {
        async updateKategorien() {
            if (this.kategorien) return;
            const kategorieGetter = mande('/api/Otium/kategorie');

            try {
                this.kategorien = await kategorieGetter.get();
            } catch (error) {
                console.error('Error fetching kategorien', error);
            }
        },
        async updateSchuljahr(force = false) {
            if (!force && this.schuljahr) return;
            const termineGetter = mande('/api/schuljahr');
            try {
                const termine = await termineGetter.get();
                this.schuljahr = termine.schultage;
                this.defaultDay = termine.standard;
            } catch (error) {
                console.error('Error fetching schuljahr', error);
            }
        },
        async updatePersonen() {
            if (this.personen) return;
            const personenGetter = mande('/api/people');

            try {
                this.personen = await personenGetter.get();
            } catch (error) {
                console.error('Error fetching personen', error);
            }
        },
        async updateBlocks(force = false) {
            if (!force && this.blocks != null) return;
            const blocksGetter = mande('/api/schuljahr/schemas');
            try {
                this.blocks = await blocksGetter.get();
            } catch (error) {
                console.error('Error fetching blocks', error);
            }
        },
    },
});
