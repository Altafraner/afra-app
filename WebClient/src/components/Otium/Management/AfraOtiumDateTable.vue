<script setup>
import {Button, Column, DataTable, Dialog, useConfirm} from "primevue";
import {formatDate, formatPerson} from "@/helpers/formatters.js";
import {RouterLink} from "vue-router";
import {defineAsyncComponent, ref} from "vue";

const CreateTerminForm = defineAsyncComponent(() => import("@/components/Form/CreateTerminForm.vue"));

const confirm = useConfirm();

const props = defineProps({
  dates: Array,
  allowEdit: Boolean
})

const emit = defineEmits(['delete', 'cancel', 'create'])

const createDialogVisible = ref(false);

const openConfirmDialog = (event, callback, message) => {
  confirm.require({
    target: event.currentTarget,
    message: message,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: {
      label: 'Ja',
      severity: 'danger'
    },
    rejectProps: {
      label: 'Nein',
      severity: 'secondary'
    },
    accept: callback
  });
}

const confirmCancel = (event, id) => {
  const onConfirm = () => emit('cancel', id);
  openConfirmDialog(event, onConfirm, "Termin absagen und Schüler:innen benachrichtigen?")
}

const confirmDelete = (event, id) => {
  const onConfirm = () => emit('delete', id);
  openConfirmDialog(event, onConfirm, "Termin löschen?")
}

const triggerCreateDialog = () => {
  createDialogVisible.value = true;
}

const create = (data) => {
  createDialogVisible.value = false;
  emit('create', data);
}
</script>

<template>
  <DataTable :value="dates" size="small">
    <Column header="Termin">
      <template #body="{data}">
        <Button v-if="!data.istAbgesagt" :as="RouterLink" :label="formatDate(new Date(data.datum))"
                :to="{name: 'Verwaltung-Termin', params: {terminId: data.id}}" variant="text"/>
        <Button v-else :label="formatDate(new Date(data.datum))" disabled severity="danger"
                variant="text"/>
      </template>
    </Column>
    <Column field="block" header="Block">
      <template #body="{data}">
        {{ data.block + 1 }}
      </template>
    </Column>
    <Column field="tutor" header="Tutor">
      <template #body="{data}">
        <template v-if="data.tutor">
          {{ formatPerson(data.tutor) }}
        </template>
      </template>
    </Column>
    <Column v-if="allowEdit" class="text-right afra-col-action">
      <template #header v-if="allowEdit">
        <Button v-tooltip="'Termin hinzufügen'" aria-label="Neuer Termin" icon="pi pi-plus"
                size="small" @click="triggerCreateDialog"/>
      </template>
      <template #body="{data}">
        <span v-if="!data.istAbgesagt" class="inline-flex gap-1">
          <Button v-tooltip="'Absagen'" aria-label="Absagen" icon="pi pi-stop" severity="danger"
                  size="small" variant="text"
                  @click="(event) => confirmCancel(event, data.id)"/>
        </span>
        <span v-else v-if="data.istAbgesagt" class="inline-flex gap-1">
          <Button v-tooltip="'Absagen beenden'" aria-label="Absagen beenden"
                  icon="pi pi-caret-right" severity="primary"
                  size="small" variant="text"/>
          <Button v-if="data.wiederholungId===null" v-tooltip="'Löschen'" aria-label="Löschen" icon="pi pi-times" severity="danger"
                  size="small" variant="text"
                  @click="(event) => confirmDelete(event, data.id)"/>
        </span>
      </template>
    </Column>
  </DataTable>
  <Dialog v-model:visible="createDialogVisible" :style="{width: '35rem'}" header="Termin hinzufügen"
          modal>
    <CreateTerminForm @submit="create"/>
  </Dialog>
</template>

<style scoped>

</style>
