<script setup>
import {Button, Column, DataTable, Dialog} from "primevue";
import {formatDayOfWeek, formatTutor} from "@/helpers/formatters.js";
import CreateWiederholungForm from "@/components/Form/CreateWiederholungForm.vue";
import {ref} from "vue";

const emits = defineEmits(['create', 'delete'])
const props = defineProps({
  regs: Array,
  allowEnrollment: Boolean,
  allowEdit: Boolean
})

const createDialogVisible = ref(false);

function createRepeating(data) {
  createDialogVisible.value = false;
  emits('create', data);
}

function showCreateDialog() {
  createDialogVisible.value = true;
}

</script>

<template>
  <DataTable :value="regs" size="medium">

    <Column field="wochentyp" header="Woche"/>
    <Column header="Tag">
      <template #body="{data}">
        {{ formatDayOfWeek(data.wochentag) }}
      </template>
    </Column>
    <Column header="Block">
      <template #body="{data}">
        {{ data.block + 1 }}
      </template>
    </Column>
    <Column field="tutor" header="Tutor">
      <template #body="slotProps">
        {{ formatTutor(slotProps.data.tutor) }}
      </template>
    </Column>
    <Column field="ort" header="Ort"/>
    <Column v-if="allowEdit" class="text-right afra-col-action">
      <template #header>
        <Button aria-label="Neue Regelmäßigkeit" icon="pi pi-plus" size="small"
                @click="showCreateDialog"/>
      </template>
      <template #body="{data}">
        <span class="inline-flex gap-1">
          <Button v-tooltip="'Löschen'" aria-label="Löschen" icon="pi pi-times" severity="danger"
                  size="small" variant="text" @click="() => emits('delete', data.id)"/>
        </span>
      </template>
    </Column>
    <template #empty>
      <div class="flex justify-center">
        Keine Regelmäßigkeiten gefunden.
      </div>
    </template>
  </DataTable>
  <Dialog v-model:visible="createDialogVisible" :style="{width: '35rem'}" header="Termin hinzufügen"
          modal>
    <CreateWiederholungForm @submit="createRepeating"/>
  </Dialog>
</template>

<style scoped>

</style>
