<script lang="ts" setup>
import { mande } from 'mande';
import { computed, h, resolveComponent, shallowRef } from 'vue';
import { usePeople } from '@/stores/people';
import { UserInfoMinimal } from '@/models/user/user';
import type { TableColumn } from '@nuxt/ui/components/Table.d.vue.ts';
import { formatStudent } from '@/helpers/formatters';
import PersonSelector from '@/components/PersonSelector.vue';

const UButton = resolveComponent('UButton');

const props = defineProps<{
    date: string;
    blockId: string;
}>();

const store = usePeople();

const result = shallowRef([]);
const loading = shallowRef(true);

const selectedPerson = shallowRef<string | undefined>(undefined);

const requestApi = mande('/api/schuljahr/' + props.date);
const block = computed<any>(() => result.value.find((r: any) => r.id === props.blockId));

async function setup() {
    result.value = await requestApi.get();
    await store.updatePersonen();
    loading.value = false;
}

async function remove(supervisor: any) {
    const api = mande(
        '/api/management/schuljahr/block/' + props.blockId + '/supervisors/' + supervisor.id,
    );
    await api.delete();
    result.value = await requestApi.get();
}

async function add() {
    if (selectedPerson.value == null) return;
    const api = mande('/api/management/schuljahr/block/' + props.blockId + '/supervisors');
    await api.post({
        value: selectedPerson.value,
    });
    result.value = await requestApi.get();
    selectedPerson.value = undefined;
}

setup();

const columns: TableColumn<any>[] = [
    {
        header: 'Name',
        cell: ({ row }) => formatStudent(row.original),
    },
    {
        header: 'Löschen',
        cell: ({ row }) =>
            h(UButton, {
                variant: 'ghost',
                icon: 'i-lucide-x',
                color: 'error',
                onClick: () => remove(row.original),
            }),
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
    },
];
</script>

<template>
    <UModal title="Aufsichten bearbeiten">
        <template #body>
            <div v-if="loading" class="w-full flex flex-col gap-2">
                <USkeleton class="h-4 w-full" />
                <USkeleton class="h-4 w-full" />
                <USkeleton class="h-4 w-[60%]" />
            </div>
            <UTable
                v-else
                :columns="columns"
                :data="block.supervisors"
                :ui="{
                    td: 'px-2 py-1',
                    th: 'px-2 py-1',
                }"
            >
                <template #empty>Keine Aufsichten eingeteilt</template>
            </UTable>
        </template>
        <template #footer="{ close }">
            <div class="flex flex-col gap-2 w-full">
                <UFormField v-if="!loading" label="Neue Aufsicht">
                    <PersonSelector
                        v-model="selectedPerson"
                        :filter="(p: UserInfoMinimal) => p.rolle === 'Tutor'"
                        class="w-full"
                        hide-rolle
                        placeholder="Person auswählen"
                    />
                </UFormField>
                <UButton
                    v-if="!loading"
                    color="primary"
                    icon="i-lucide-plus"
                    label="Hinzufügen"
                    @click="add"
                />
                <UButton
                    class="mt-2"
                    color="secondary"
                    icon="i-lucide-x"
                    label="Schließen"
                    variant="subtle"
                    @click="close"
                />
            </div>
        </template>
    </UModal>
</template>

<style scoped></style>
