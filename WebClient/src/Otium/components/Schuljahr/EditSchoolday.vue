<script setup>
import { mande } from 'mande';
import { computed, h, shallowRef } from 'vue';
import { formatStudent } from '@/helpers/formatters.ts';
import EditSupervisorsForm from '@/Otium/components/Schuljahr/EditSupervisorsForm.vue';
import { useRouter } from 'vue-router';
import { useConfirmPopover } from '@/composables/confirmPopover.ts';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';
import UButton from '@nuxt/ui/components/Button.vue';

const props = defineProps({
    date: {
        type: String,
    },
});

const api = mande('/api/schuljahr/' + props.date);
const loading = shallowRef(true);
const result = shallowRef(null);
const newBlock = shallowRef(null);
const router = useRouter();
const overlay = useOverlay();
const otiumStore = useOtiumStore();

const { requireConfirm } = useConfirmPopover();

async function setup() {
    result.value = await api.get();
    await otiumStore.updateBlocks();
    loading.value = false;
}

async function editSupervisors(block) {
    const form = overlay.create(EditSupervisorsForm);
    await form.open({
        date: props.date,
        blockId: block.id,
    });
    await setup();
}

async function supervise(block) {
    await router.push({
        name: 'Aufsicht',
        query: {
            slotId: block.id,
            scope: 'otium',
        },
    });
}

async function remove(block) {
    if (!(await requireConfirm('Wollen Sie den Schultag wirklich löschen?'))) return;
    const api = mande('/api/management/schuljahr/block/' + block.id);
    await api.delete();
    await setup();
}

async function add() {
    const api = mande('/api/management/schuljahr/' + props.date + '/block/');
    await api.post({
        value: newBlock.value,
    });
    newBlock.value = null;
    await setup();
}

setup();

const mappedResult = computed(() => {
    return result.value.map((data) => ({
        original: data,
        name: data.name,
        supervisors:
            data.supervisors.length === 0
                ? 'Keine Aufsichten'
                : data.supervisors.map((s) => formatStudent(s)).join(', '),
    }));
});

const columns = [
    {
        header: 'Block',
        accessorKey: 'name',
    },
    { header: 'Aufsichten', accessorKey: 'supervisors' },
    {
        id: 'actions',
        header: 'Aktionen',
        meta: { class: { td: 'text-right' } },
        cell: ({ row }) =>
            h('div', { class: 'flex justify-end gap-1' }, [
                h(UTooltip, { text: 'Aufsicht' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-eye',
                        variant: 'ghost',
                        size: 'sm',
                        onClick: () => supervise(row.original.original),
                    }),
                ),
                h(UTooltip, { text: 'Aufsichten Bearbeiten' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-pencil',
                        variant: 'ghost',
                        size: 'sm',
                        color: 'neutral',
                        onClick: () => editSupervisors(row.original.original),
                    }),
                ),
                h(UTooltip, { text: 'Löschen' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-x',
                        variant: 'ghost',
                        size: 'sm',
                        color: 'error',
                        onClick: () => remove(row.original.original),
                    }),
                ),
            ]),
    },
];
</script>

<template>
    <template v-if="loading">
        <div class="flex flex-col gap-2">
            <USkeleton class="w-full h-30 mx-2" />
        </div>
    </template>
    <UCard
        v-else
        :ui="{
            body: 'p-2 sm:p-2',
            footer: 'p-2 sm:p-2',
        }"
        variant="soft"
    >
        <UTable
            :columns="columns"
            :data="mappedResult"
            :ui="{
                td: 'whitespace-normal px-2 py-1.5',
                th: 'px-2 py-1.5',
                root: 'overflow-x-visible',
            }"
        />
        <template #footer>
            <UFormField :ui="{ label: 'text-default', root: 'mx-2' }" label="Block hinzufügen">
                <UFieldGroup class="w-full">
                    <USelect
                        v-model="newBlock"
                        :items="
                            otiumStore.blocks.filter(
                                (b) => !result.some((r) => r.schemaId === b.schemaId),
                            )
                        "
                        class="w-full"
                        label-key="bezeichnung"
                        placeholder="Neuen Block wählen"
                        value-key="schemaId"
                    />
                    <UButton icon="i-lucide-plus" @click="add" />
                </UFieldGroup>
            </UFormField>
        </template>
    </UCard>
</template>

<style scoped></style>
