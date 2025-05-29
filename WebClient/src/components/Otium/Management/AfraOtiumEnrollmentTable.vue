<script setup>

import {Badge, Column, DataTable} from "primevue";
import AfraOtiumAnwesenheit from "@/components/Otium/Shared/AfraOtiumAnwesenheit.vue";
import {formatStudent} from "@/helpers/formatters.js";

const props = defineProps({
  enrollments: Array,
  showAttendance: Boolean,
  mayEditAttendance: Boolean,
  updateFunction: Function
})


</script>

<template>
  <DataTable :value="props.enrollments">
    <Column header="Schüler:in">
      <template #body="{data}">
        {{ formatStudent(data.student) }}
      </template>
    </Column>
    <Column header="Anwesenheit" v-if="props.showAttendance || props.mayEditAttendance"
            :class="props.mayEditAttendance ? 'text-right afra-col-action' : ''">
      <template #body="{data}">
        <afra-otium-anwesenheit v-if="data.student.rolle!=='Oberstufe'" v-model="data.anwesenheit"
                                :mayEdit="props.mayEditAttendance"
                                @value-changed="(value) => updateFunction(data.student, value)"/>
        <badge v-else v-tooltip="'Oberstufenschüler:innen haben keine Anwesenheit'"
               severity="secondary">
          N/A
        </badge>
      </template>
    </Column>
    <template #empty>
      <div class="flex justify-center">Keine Einschreibungen</div>
    </template>
  </DataTable>
</template>

<style scoped>

</style>
