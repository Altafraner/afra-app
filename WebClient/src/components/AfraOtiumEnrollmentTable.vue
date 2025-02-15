<script setup>

import {Column, DataTable, DatePicker} from "primevue";
import AfraOtiumAnwesenheit from "@/components/AfraOtiumAnwesenheit.vue";
const props = defineProps({
  enrollments: Array,
  showAttendance: Boolean,
  mayEditAttendance: Boolean
})


const padString = (text) => String(text).padStart(2, '0')
const formatTime = date => padString(date.getHours()) + ":" + padString(date.getMinutes());
const formatStudent = student => student.given_name + " " + student.last_name
</script>

<template>
  <DataTable :value="props.enrollments" edit-mode="cell">
    <Column header="Schüler:in">
      <template #body="slotProps">
        {{ formatStudent(slotProps.data.student) }}
      </template>
    </Column>
    <Column v-for="field in ['start', 'end']" :field="field" :header="field==='start' ? 'Anfang' : 'Ende'">
      <template #body="{data}">
        {{formatTime(data[field])}}
      </template>
      <template #editor="editorProps">
        <DatePicker time-only v-model="editorProps.data[field]"/>
      </template>
    </Column>
    <Column header="Anwesenheit" v-if="props.showAttendance || props.mayEditAttendance">
      <template #body="slotProps">
        <afra-otium-anwesenheit :initial-status="slotProps.data.verified" :mayEdit="props.mayEditAttendance"/>
      </template>
    </Column>
  </DataTable>
</template>

<style scoped>

</style>
