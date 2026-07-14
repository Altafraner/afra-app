<script lang="ts" setup>
import { useUser } from '@/stores/user';
import { computed } from 'vue';
import { formatTutor } from '@/helpers/formatters';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import UserPeek from '@/components/UserPeek.vue';
import { UserInfoMinimal } from '@/models/user/user';
import { usePeople } from '@/stores/people';

const user = useUser();
const router = useRouter();
const toast = useToast();
const peopleStore = usePeople();

await peopleStore.updatePersonen();

const isAdmin = computed(() => user.loggedIn && user.isAdmin);

const personen = computed<[string, UserInfoMinimal[]][]>(() => {
    const sorted: UserInfoMinimal[] =
        peopleStore.personen?.sort((a, b) => {
            const A = (formatTutor(a) || '').toLowerCase();
            const B = (formatTutor(b) || '').toLowerCase();
            return A < B ? -1 : A > B ? 1 : 0;
        }) ?? [];

    const grouped = sorted.reduce<Record<string, UserInfoMinimal[]>>((acc, p) => {
        const key = p.gruppe && p.gruppe.trim() !== '' ? p.gruppe : p.rolle;
        (acc[key] ??= []).push(p);
        return acc;
    }, {});

    const parseGroup = (str: string) => {
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

        if (pa.num !== pb.num) return (pa.num ?? 0) - (pb.num ?? 0);
        return pa.suffix.localeCompare(pb.suffix, 'de', { sensitivity: 'base' });
    });
});

const impersonate = async (userToImpersonate: UserInfoMinimal) => {
    try {
        await mande(`/api/user/${userToImpersonate.id}/impersonate`).get();
    } catch {
        toast.add({
            color: 'error',
            title: 'Impersonieren fehlgeschlagen',
        });
    }
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
                <h3 class="font-bold mb-2">{{ gruppe }}</h3>
                <ul>
                    <li class="flex flex-col gap-2">
                        <div v-for="u in users" :key="u.id" class="flex flex-row items-center">
                            <UButton
                                icon="i-lucide-users"
                                variant="subtle"
                                @click="impersonate(u)"
                            />
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
