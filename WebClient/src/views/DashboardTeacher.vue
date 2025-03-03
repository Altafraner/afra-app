<script setup>
import {Button, DataTable, Column, Tag} from "primevue";
import {formatDate, formatStudent, otiumTutorLinkGenerator} from "../helpers/formatters.js";
import {teacherOtiaView, mentees} from "@/helpers/testdata.js";
import {ref} from "vue";
import {useSettings} from "@/stores/useSettings.js";

const settings = useSettings()
const termine = ref(teacherOtiaView);
const findBlock = startTime => {
  for (const block of settings.blocks) {
    if (startTime >= block.startTime && startTime < block.endTime) return block.id
  }

  console.error("start Time is in no Block", startTime)
}

const severity = [
  {
    severity: "success",
    label: "Ok"
  },
  {
    severity: "warn",
    label: "Auffällig"
  },
  {
    severity: "danger",
    label: "Unvollständig"
  }
]
</script>

<template>
  <h1>Dashboard</h1>
  <h2>Betreute Otia</h2>
  <DataTable :value="termine">
    <Column header="Otium">
      <template #body="{data}">
        <Button variant="link" as="RouterLink" :to="otiumTutorLinkGenerator(data.id, data.datum, data.block)">{{data.bezeichnung}}</Button>
      </template>
    </Column>
    <Column field="block" header="Block"/>
    <Column header="Datum">
      <template #body="{data}">
        {{formatDate(data.datum)}}
      </template>
    </Column>
  </DataTable>
  <h2>Ihre Mentees</h2>
  <DataTable :value="mentees">
    <Column header="Name">
      <template #body="{data}">
        <Button variant="link" as="RouterLink" :to="`/student/${data.student.id}`">{{formatStudent(data.student)}}</Button>
      </template>
    </Column>
    <Column v-for="field in [{field: 'letzte', header: 'Letzte'}, {field: 'diese', header: 'Diese'}, {field: 'naechste', header: 'Nächste'}]" :key="field.field" :header="field.header">
      <template #body="{data}">
        <Tag :severity="severity[data[field.field]].severity">{{severity[data[field.field]].label}}</Tag>
      </template>
    </Column>
  </DataTable>

</template>

<style scoped>

</style>
