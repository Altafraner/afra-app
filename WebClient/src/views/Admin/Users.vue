<script lang="ts" setup>
import { useUser } from '@/stores/user';
import { computed, h, resolveComponent } from 'vue';
import { formatTutor } from '@/helpers/formatters';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import UserPeek from '@/components/UserPeek.vue';
import { UserInfoMinimal } from '@/models/user/user';
import { usePeople } from '@/stores/people';
import { useConfirmPopover } from '@/composables/confirmPopover.ts';
import { TableColumn } from '@nuxt/ui';

const user = useUser();
const router = useRouter();
const toast = useToast();
const peopleStore = usePeople();
const confirm = useConfirmPopover();

const UButton = resolveComponent('UButton');

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

const deleteUser = async (userToDelete: UserInfoMinimal) => {
    try {
        const confirmed = await confirm.requireConfirm(
            'Das Löschen kann nicht vollständig und nur unter hohen Umständen rückgängig gemacht werden.',
            'Wollen Sie die Person wirklich löschen?',
        );
        if (!confirmed) return;
        await mande(`/api/people/${userToDelete.id}`).delete();
    } catch {
        toast.add({
            color: 'error',
            title: 'Löschen fehlgeschlagen',
        });
    }
    await peopleStore.updatePersonen(true);
};

const columns: TableColumn<UserInfoMinimal>[] = [
    {
        header: 'Nutzer',
        cell: ({ row }) =>
            h(UserPeek, {
                person: row.original,
            }),
        meta: {
            class: {
                td: 'w-full',
            },
        },
    },
    {
        id: 'impersonate',
        cell: ({ row }) =>
            h(UButton, {
                icon: 'i-lucide-user',
                variant: 'subtle',
                label: 'Impersonieren',
                onClick: () => impersonate(row.original),
            }),
    },
    {
        id: 'delete',
        cell: ({ row }) =>
            h(UButton, {
                icon: 'i-lucide-x',
                variant: 'subtle',
                color: 'error',
                label: 'Löschen',
                onClick: () => deleteUser(row.original),
            }),
    },
];
</script>

<template>
    <template v-if="isAdmin">
        <h1>Admin-Bereich</h1>
        <h2>Nutzerübersicht</h2>
        <div v-for="[gruppe, users] in personen" :key="gruppe" class="mb-4">
            <h3 class="font-bold mb-2">{{ gruppe }}</h3>
            <UTable
                :columns="columns"
                :data="users"
                :ui="{
                    td: 'whitespace-normal text-default px-2 py-1.5',
                    th: 'px-2 py-1.5',
                    root: 'overflow-x-visible',
                }"
            />
        </div>
    </template>
    <template v-else>
        <h1>Kein Zugriff</h1>
        <p>Du hast keine Berechtigung, auf den Admin-Bereich zuzugreifen.</p>
    </template>
</template>

<style scoped></style>
