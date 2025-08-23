<script setup>
import { ref } from 'vue';
import { Button, Column, DataTable, Dialog } from 'primevue';
import { formatDate, formatDayOfWeek, formatTutor } from '@/helpers/formatters.js';
import CreateWiederholungForm from '@/Otium/components/Management/CreateWiederholungForm.vue';
import CancelWiederholungForm from '@/Otium/components/Management/CancelWiederholungForm.vue';

const emits = defineEmits(['create', 'delete', 'cancel']);
const props = defineProps({
    regs: Array,
    allowEnrollment: Boolean,
    allowEdit: Boolean,
});

const createDialogVisible = ref(false);
const cancelDialogVisible = ref(false);
const wiederholungToCancel = ref(null);

function createRepeating(data) {
    createDialogVisible.value = false;
    emits('create', data);
}

function cancelRepeating(data) {
    console.log('Cancelling', data);
    cancelDialogVisible.value = false;
    emits('cancel', wiederholungToCancel.value.id, data);
}

function showCreateDialog() {
    createDialogVisible.value = true;
}

function showCancelDialog(data) {
    wiederholungToCancel.value = data;
    cancelDialogVisible.value = true;
}
</script>

<template>
    <DataTable :value="regs" size="medium">
        <Column field="wochentyp" header="Woche" />
        <Column header="Tag">
            <template #body="{ data }">
                {{ formatDayOfWeek(data.wochentag) }}
            </template>
        </Column>
        <Column header="Block">
            <template #body="{ data }">
                {{ data.block }}
            </template>
        </Column>
        <Column field="tutor" header="Tutor">
            <template #body="slotProps">
                {{ formatTutor(slotProps.data.tutor) }}
            </template>
        </Column>
        <Column field="ort" header="Ort" />
        <Column field="startDate" header="Von">
            <template #body="slotProps">
                {{ formatDate(new Date(slotProps.data.startDate)) }}
            </template>
        </Column>
        <Column field="endDate" header="Bis">
            <template #body="slotProps">
                {{ formatDate(new Date(slotProps.data.endDate)) }}
            </template>
        </Column>
        <Column v-if="allowEdit" class="text-right afra-col-action">
            <template #header>
                <Button
                    aria-label="Neue Regelmäßigkeit"
                    icon="pi pi-plus"
                    size="small"
                    @click="showCreateDialog"
                />
            </template>
            <template #body="{ data }">
                <span class="inline-flex gap-1">
                    <Button
                        v-tooltip="'Löschen'"
                        aria-label="Löschen"
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="() => emits('delete', data.id)"
                    />
                    <Button
                        v-tooltip="'Einkürzen'"
                        aria-label="Löschen"
                        icon="pi pi-stop"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="() => showCancelDialog(data)"
                    />
                </span>
            </template>
        </Column>
        <template #empty>
            <div class="flex justify-center">Keine Regelmäßigkeiten gefunden.</div>
        </template>
    </DataTable>
    <Dialog
        v-model:visible="createDialogVisible"
        :style="{ width: '35rem' }"
        header="Regelmäßigkeit hinzufügen"
        modal
    >
        <CreateWiederholungForm @submit="createRepeating" />
    </Dialog>
    <Dialog
        v-model:visible="cancelDialogVisible"
        :style="{ width: '35rem' }"
        header="Regelmäßigkeit verkürzen"
        modal
    >
        <CancelWiederholungForm
            :wiederholung="wiederholungToCancel"
            @submit="cancelRepeating"
        />
    </Dialog>
</template>

<style scoped></style>
