import { defineStore } from 'pinia';
import { mande } from 'mande';
import type { ProfundumDefinition } from '@/Profundum/models/verwaltung';

export const useProfunda = defineStore('profunda', {
    state: (): { profunda: ProfundumDefinition[] | null } => ({
        profunda: null,
    }),
    actions: {
        async updateProfunda(force = false) {
            if (!force && this.profunda) return;
            try {
                this.profunda = await mande('/api/profundum/management/profundum').get();
            } catch (error) {
                console.error('Error fetching profunda', error);
            }
        },
    },
});
