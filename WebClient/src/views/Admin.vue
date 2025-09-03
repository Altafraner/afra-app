<script setup>
import { useUser } from '@/stores/user.js';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { computed } from 'vue';
import { formatTutor } from '@/helpers/formatters.js';
import { Button } from 'primevue';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import UserPeek from '@/components/UserPeek.vue';

const user = useUser();
const otium = useOtiumStore();
const router = useRouter();

await otium.updatePersonen();

const isAdmin = computed(() => user.loggedIn && user.isAdmin);

const personen = computed(() => {
    const sorted = [...otium.personen].sort((a, b) => {
        const A = (formatTutor(a) || '').toLowerCase();
        const B = (formatTutor(b) || '').toLowerCase();
        return A < B ? -1 : A > B ? 1 : 0;
    });

    const grouped = sorted.reduce((acc, p) => {
        const key = p.gruppe && p.gruppe.trim() !== '' ? p.gruppe : p.rolle;
        (acc[key] ??= []).push(p);
        return acc;
    }, {});

    const parseGroup = (str) => {
        const match = /^(\d+)(.*)$/i.exec(str);
        if (match) {
            return {
                num: parseInt(match[1], 10),
                suffix: match[2].trim().toLowerCase(),
                hasNum: true,
            };
        }
        return { num: null, suffix: str.toLowerCase(), hasNum: false };
    };

    return Object.entries(grouped).sort(([a], [b]) => {
        const pa = parseGroup(a);
        const pb = parseGroup(b);

        if (pa.hasNum && !pb.hasNum) return 1;
        if (!pa.hasNum && pb.hasNum) return -1;

        if (!pa.hasNum && !pb.hasNum) {
            return pa.suffix.localeCompare(pb.suffix, 'de', { sensitivity: 'base' });
        }

        if (pa.num !== pb.num) return pa.num - pb.num;
        return pa.suffix.localeCompare(pb.suffix, 'de', { sensitivity: 'base' });
    });
});

const impersonate = async (userToImpersonate) => {
    console.log(userToImpersonate);
    await mande(`/api/user/${userToImpersonate.id}/impersonate`).get();
    await user.update();
    await router.push('/');
};
</script>

<template>
    <template v-if="isAdmin">
        <h1>Admin-Bereich</h1>
        <h2>Impersonieren</h2>
        <ul>
            <li v-for="[gruppe, users] in personen" :key="gruppe" class="mb-4">
                <ul>
                    <h3 class="font-bold mb-2">{{ gruppe }}</h3>
                    <li class="flex flex-col gap-2">
                        <div v-for="u in users" :key="u.id">
                            <Button icon="pi pi-users" size="small" @click="impersonate(u)" />
                            <UserPeek :person="u" />
                        </div>
                    </li>
                </ul>
            </li>
        </ul>
    </template>
    <template v-else>
        <h1>Kein Zugriff</h1>
        <p>Du hast keine Berechtigung, auf den Admin-Bereich zuzugreifen.</p>
    </template>
</template>

<style scoped></style>
