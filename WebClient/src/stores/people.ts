import { defineStore } from 'pinia';
import { mande } from 'mande';
import { UserInfoMinimal } from '@/models/user/user';
import { shallowRef } from 'vue';

export const usePeople = defineStore('people', () => {
    const personen = shallowRef<UserInfoMinimal[] | null>();

    async function updatePersonen(force: boolean = false) {
        if (!force && personen.value) return;
        const personenGetter = mande('/api/people');

        try {
            personen.value = await personenGetter.get<UserInfoMinimal[]>();
        } catch (error) {
            console.error('Error fetching personen', error);
        }
    }

    return { personen, updatePersonen };
});
