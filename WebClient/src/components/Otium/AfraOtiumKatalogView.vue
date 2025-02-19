<script setup>

import {DataTable, Column, Button, Badge} from "primevue";
import MeterGroup from "primevue/metergroup";
import {formatTutor} from "@/helpers/formatters.js"

const props = defineProps({
  otia: Array,
  linkGenerator: Function
})

const chooseColor = (now, max) => {
  if (max===0 || now <= 0.7) return 'var(--p-button-success-background)'
  if (now < 1) return 'var(--p-button-warn-background)'
  return 'var(--p-button-danger-background)'
}

console.log(props.otia)

</script>

<template>
  <DataTable :value="props.otia">
    <Column header="Bezeichnung">
      <template #body="{data}">
        <Button variant="link" as="RouterLink" :to="linkGenerator(data)" :label="data.bezeichnung" />
      </template>
    </Column>
    <Column header="Raum" field="ort" />
    <Column header="Leher:in">
      <template #body="{data}">
        {{formatTutor(data.tutor)}}
      </template>
    </Column>
    <Column header="Auslastung">
      <template #body="{data}">
        <div class="enrollmentGrid align-center gap-3">
          <Badge v-if="data.maxEinschreibungen !== 0" severity="secondary" :value="`${data.auslastung*100} %`" />
          <span v-else></span>
          <MeterGroup :value="[{value: data.auslastung, color: chooseColor(data.auslastung, data.maxEinschreibungen), label: null}]" :max="data.maxEinschreibungen === 0 ? 0 : 1" label-position="none" />
        </div>
      </template>
    </Column>
  </DataTable>
</template>

<style scoped>
  .enrollmentGrid{
    display: grid;
    grid-template-columns: 3.5rem 7rem;
    justify-content: center;
  }
</style>
