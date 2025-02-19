<script setup>

import {Column, DataTable, DatePicker} from "primevue";
import AfraOtiumAnwesenheit from "@/components/Otium/AfraOtiumAnwesenheit.vue";
import {formatStudent, formatTime} from "@/helpers/formatters.js";
const props = defineProps({
  enrollments: Array,
  showAttendance: Boolean,
  mayEditAttendance: Boolean
})


</script>

<template>
  <DataTable :value="props.enrollments">
    <Column header="Schüler:in">
      <template #body="slotProps">
        {{ formatStudent(slotProps.data.student) }}
      </template>
    </Column>
    <Column v-for="field in ['start', 'ende']" :field="field" :header="field==='start' ? 'Anfang' : 'Ende'">
      <template #body="{data}">
        {{formatTime(data[field])}}
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
