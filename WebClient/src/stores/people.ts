import { defineStore } from 'pinia';
import { mande } from 'mande';
import { UserInfoMinimal } from '@/models/user/userInfoMinimal.ts';

export const usePeople = defineStore('people', {
    state: (): {
        personen: UserInfoMinimal[] | null;
    } => ({
        personen: null,
    }),
    actions: {
        async updatePersonen() {
            if (this.personen) return;
            const personenGetter = mande('/api/people');

            try {
                this.personen = await personenGetter.get<UserInfoMinimal[]>();
            } catch (error) {
                console.error('Error fetching personen', error);
            }
        },
    },
});
