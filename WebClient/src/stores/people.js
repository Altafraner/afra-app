import { defineStore } from 'pinia';
import { mande } from 'mande';

export const usePeople = defineStore('settings', {
    state: () => ({
        personen: null,
    }),
    actions: {
        async updatePersonen() {
            if (this.personen) return;
            const personenGetter = mande('/api/people');

            try {
                this.personen = await personenGetter.get();
            } catch (error) {
                console.error('Error fetching personen', error);
            }
        },
    },
});
