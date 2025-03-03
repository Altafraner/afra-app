<script setup>
import {ref} from 'vue';
import {DataTable, Column, Button} from "primevue";
import {formatTutor} from "@/helpers/formatters.js";

const props = defineProps({
  regs: Array,
  allowEnrollment: Boolean,
  allowEdit: Boolean
})

const regs = ref(props.regs)
</script>

<template>
  <DataTable :value="regs" size="medium">

    <Column field="wochentyp" header="Woche" />
    <Column field="wochentag" header="Tag" />
    <Column field="block" header="Block" />
    <Column field="tutor" header="Tutor">
      <template #body="slotProps">
        {{formatTutor(slotProps.data.tutor)}}
      </template>
    </Column>
    <Column v-if="allowEdit" class="afra-col-action">
      <template #header>
        <Button aria-label="Neue Regelmäßigkeit" icon="pi pi-plus" size="small"></Button>
      </template>
      <template #body>
        <span class="inline-flex gap-1">
          <Button aria-label="Ansehen" severity="primary" size="small" variant="text" icon="pi pi-eye"></Button>
          <Button aria-label="Bearbeiten" severity="secondary" variant="text" size="small" icon="pi pi-pencil"></Button>
        </span>
      </template>
    </Column>
  </DataTable>
</template>

<style scoped>

</style>
