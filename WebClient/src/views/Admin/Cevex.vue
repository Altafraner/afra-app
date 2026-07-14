<script lang="ts" setup>
import { useCevex } from '@/composables/cevex';
import { h, ref, resolveComponent } from 'vue';
import type { UserInfoMinimal } from '@/models/user/user';
import type { CevexInformation, CevexMatch } from '@/models/admin/cevex';
import type { TableColumn } from '@nuxt/ui/components/Table.d.vue.ts';
import UserPeek from '@/components/UserPeek.vue';
import CevexAttachDialog from '@/components/Admin/CevexAttachDialog.vue';

const UButton = resolveComponent('UButton');
const UBadge = resolveComponent('UBadge');

const cevex = useCevex();
const overlay = useOverlay();
const toast = useToast();
const data = ref<CevexInformation | null>(null);

data.value = await cevex.getInformation();

async function match(student: UserInfoMinimal) {
    const modal = overlay.create(CevexAttachDialog);
    const result = await modal.open({
        options: data.value?.available ?? [],
        student,
    });
    if (!result) return;
    await cevex.setMatch(student, result);
    toast.add({
        color: 'success',
        title: 'Zuweisung erfolgreich',
    });
    data.value = await cevex.getInformation();
}

async function remove(student: UserInfoMinimal) {
    await cevex.setMatch(student, {
        id: '00000-0000000000-AAAAAAA',
    });
    toast.add({
        color: 'success',
        title: 'Zuweisung erfolgreich entfernt',
    });
    data.value = await cevex.getInformation();
}

const columns: TableColumn<CevexMatch>[] = [
    {
        header: 'Nutzer',
        cell: ({ row }) => h(UserPeek, { person: row.original.user, showGroup: true }),
    },
    {
        header: 'Cevex-Schüler:in',
        cell: ({ row }) =>
            row.original.cevex == null
                ? h(UBadge, { color: 'warning', label: 'Nicht zugewiesen', variant: 'subtle' })
                : row.original.cevex.firstName + ' ' + row.original.cevex.lastName,
    },
    {
        id: 'action',
        cell: ({ row }) =>
            row.original.cevex == null
                ? h(UButton, {
                      label: 'Zuweisen',
                      size: 'sm',
                      icon: 'i-lucide-arrow-right',
                      onClick: () => match(row.original.user),
                  })
                : h(UButton, {
                      label: 'Lösen',
                      icon: 'i-lucide-x',
                      variant: 'subtle',
                      color: 'neutral',
                      size: 'sm',
                      onClick: () => remove(row.original.user),
                  }),
        meta: {
            class: {
                td: 'text-right',
            },
        },
    },
];
</script>

<template>
    <h1>Cevex Nutzersynchronisierung</h1>
    <UTable :columns="columns" :data="data?.matches ?? []" />
</template>

<style scoped></style>
