<script setup>
import { Button, Column, DataTable, Dialog } from 'primevue';
import { formatDate, formatPerson } from '@/helpers/formatters.js';
import { RouterLink } from 'vue-router';
import { defineAsyncComponent, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover.js';

const CreateTerminForm = defineAsyncComponent(
    () => import('@/Otium/components/Management/CreateTerminForm.vue'),
);

const { openConfirmDialog } = useConfirmPopover();

const props = defineProps({
    dates: Array,
    allowEdit: Boolean,
});

const emit = defineEmits(['delete', 'cancel', 'create', 'continue']);

const createDialogVisible = ref(false);

const confirmCancel = (event, id) => {
    const onConfirm = () => emit('cancel', id);
    openConfirmDialog(event, onConfirm, 'Termin absagen und Schüler:innen benachrichtigen?');
};

const confirmDelete = (event, id) => {
    const onConfirm = () => emit('delete', id);
    openConfirmDialog(event, onConfirm, 'Termin löschen?');
};

const confirmContinue = (event, id) => {
    const onConfirm = () => emit('continue', id);
    openConfirmDialog(event, onConfirm, 'Termin nicht mehr abbrechen?', null, 'success');
};

const triggerCreateDialog = () => {
    createDialogVisible.value = true;
};

const create = (data) => {
    createDialogVisible.value = false;
    emit('create', data);
};
</script>

<template>
    <DataTable :value="dates" size="small">
        <Column header="Termin">
            <template #body="{ data }">
                <Button
                    v-if="!data.istAbgesagt"
                    :as="RouterLink"
                    :label="formatDate(new Date(data.datum))"
                    :icon="data.wiederholungId === null ? '' : 'pi pi-refresh'"
                    :to="{ name: 'Verwaltung-Termin', params: { terminId: data.id } }"
                    variant="text"
                />
                <Button
                    v-else
                    :label="formatDate(new Date(data.datum))"
                    disabled
                    severity="danger"
                    :icon="data.wiederholungId === null ? '' : 'pi pi-refresh'"
                    variant="text"
                />
            </template>
        </Column>
        <Column field="block" header="Block">
            <template #body="{ data }">
                {{ data.block }}
            </template>
        </Column>
        <Column field="tutor" header="Tutor">
            <template #body="{ data }">
                <template v-if="data.tutor">
                    {{ formatPerson(data.tutor) }}
                </template>
            </template>
        </Column>
        <Column v-if="allowEdit" class="text-right afra-col-action">
            <template #header v-if="allowEdit">
                <Button
                    v-tooltip="'Termin hinzufügen'"
                    aria-label="Neuer Termin"
                    icon="pi pi-plus"
                    size="small"
                    @click="triggerCreateDialog"
                />
            </template>
            <template #body="{ data }">
                <span v-if="!data.istAbgesagt" class="inline-flex gap-1">
                    <Button
                        v-tooltip="'Absagen'"
                        aria-label="Absagen"
                        icon="pi pi-stop"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="(event) => confirmCancel(event, data.id)"
                    />
                </span>
                <span v-else v-if="data.istAbgesagt" class="inline-flex gap-1">
                    <Button
                        v-tooltip="'Absagen beenden'"
                        aria-label="Absagen beenden"
                        icon="pi pi-caret-right"
                        severity="success"
                        size="small"
                        variant="text"
                        @click="(event) => confirmContinue(event, data.id)"
                    />
                    <Button
                        v-if="data.wiederholungId === null"
                        v-tooltip="'Löschen'"
                        aria-label="Löschen"
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="(event) => confirmDelete(event, data.id)"
                    />
                    <Button
                        v-else
                        v-tooltip="'Regelmäßige Termine können nicht gelöscht werden.'"
                        aria-label="Löschen"
                        icon="pi pi-times"
                        severity="secondary"
                        disabled
                        size="small"
                        variant="text"
                    />
                </span>
            </template>
        </Column>
        <template #empty>
            <div class="flex justify-center">Keine Termine angelegt.</div>
        </template>
    </DataTable>
    <Dialog
        v-model:visible="createDialogVisible"
        :style="{ width: '35rem' }"
        header="Termin hinzufügen"
        modal
    >
        <CreateTerminForm @submit="create" />
    </Dialog>
</template>

<style scoped></style>
