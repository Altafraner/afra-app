<script setup>
import {defineProps, ref} from 'vue';
import {DataTable, Column, Button} from "primevue";

const props = defineProps({
  dates: Array,
  allowEnrollment: Boolean,
  allowEdit: Boolean
})

const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "short"
});

const formatTutor = tutor => tutor.last_name + ", " + tutor.given_name

const dates = ref(props.dates)
</script>

<template>
  <DataTable :value="dates" size="small">
    <Column field="date" header="Datum" >
      <template #body="slotProps">
        {{formatDate(slotProps.data.date)}}
      </template>
    </Column>
    <Column field="start" header="Anfang" />
    <Column field="end" header="Ende" />
    <Column field="tutor" header="Tutor" >
      <template #body="slotProps">
        {{formatTutor(slotProps.data.tutor)}}
      </template>
    </Column>
    <Column v-if="allowEnrollment" class="afra-col-action">
      <template #body>
        <Button aria-label="Einschreiben" severity="primary" size="small" variant="text" icon="pi pi-plus"></Button>
      </template>
    </Column>
    <Column v-if="allowEdit" class="afra-col-action">
      <template #header v-if="allowEdit">
        <Button aria-label="Neuer Termin" icon="pi pi-plus" size="small"></Button>
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
