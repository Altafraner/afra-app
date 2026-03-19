import { defineStore } from 'pinia';
import { mande } from 'mande';

export const useProfundumEinwahl = defineStore('profundumEinwahl', {
    state: () => ({
        isEinwahlActive: false,
    }),
    actions: {
        async update() {
            try {
                const api = mande('/api/profundum/sus/einwahl');
                this.isEinwahlActive = await api.get('/aktiv');
            } catch {
                this.isEinwahlActive = false;
            }
        },
    },
});
