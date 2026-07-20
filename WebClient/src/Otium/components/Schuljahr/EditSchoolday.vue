<script setup>
import { mande } from 'mande';
import { shallowRef } from 'vue';
import { Button, FloatLabel, Select, Skeleton } from 'primevue';
import { formatStudent } from '@/helpers/formatters.ts';
import EditSupervisorsForm from '@/Otium/components/Schuljahr/EditSupervisorsForm.vue';
import { useRouter } from 'vue-router';
import { useConfirmPopover } from '@/composables/confirmPopover.ts';
import { useOtiumStore } from '@/Otium/stores/otium.js';

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

const { openConfirmDialog } = useConfirmPopover();

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

function remove(evt, block) {
    openConfirmDialog(evt, callback, 'Wirklich löschen?');
    async function callback() {
        const api = mande('/api/management/schuljahr/block/' + block.id);
        await api.delete();
        await setup();
    }
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
</script>

<template>
    <template v-if="loading">
        <Skeleton class="w-full h-4" />
        <Skeleton class="w-full h-4" />
        <Skeleton class="w-full h-4" />
        <Skeleton class="w-full h-4" />
        <Skeleton class="w-full h-4" />
        <Skeleton class="w-full h-4" />
    </template>
    <template v-else>
        <div class="grid grid-cols-[auto_1fr_auto] w-full gap-2 items-center mb-4 mx-2">
            <span class="font-bold">Name</span>
            <span class="font-bold">Aufsicht</span>
            <span></span>
            <template v-for="data in result">
                <span>{{ data.name }}</span>
                <span v-if="data.supervisors.length === 0">Keine Aufsichten</span>
                <span v-else>{{
                    data.supervisors.map((s) => formatStudent(s)).join(', ')
                }}</span>
                <span class="inline-flex justify-end">
                    <Button
                        aria-label="Aufsicht"
                        icon="pi pi-eye"
                        size="small"
                        variant="text"
                        @click="() => supervise(data)"
                    />
                    <Button
                        aria-label="Bearbeiten"
                        icon="pi pi-pencil"
                        severity="secondary"
                        size="small"
                        variant="text"
                        @click="() => editSupervisors(data)"
                    />
                    <Button
                        aria-label="Löschen"
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="(evt) => remove(evt, data)"
                    />
                </span>
            </template>
        </div>
        <div class="grid grid-cols-[1fr_auto] gap-2">
            <FloatLabel variant="on">
                <Select
                    id="newBlock"
                    v-model="newBlock"
                    :options="
                        otiumStore.blocks.filter(
                            (b) => !result.some((r) => r.schemaId === b.schemaId),
                        )
                    "
                    fluid
                    option-label="bezeichnung"
                    option-value="schemaId"
                    size="small"
                ></Select>
                <label for="newBlock">Block hinzufügen</label>
            </FloatLabel>
            <Button aria-label="Hinzufügen" icon="pi pi-plus" size="small" @click="add" />
        </div>
    </template>
</template>

<style scoped></style>
