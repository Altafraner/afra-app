<script setup>

import {DataTable, Column, Button, Tag} from "primevue";
import MeterGroup from "primevue/metergroup";
import {chooseColor, chooseSeverity, formatTutor} from "@/helpers/formatters.js"

const props = defineProps({
  otia: Array,
  linkGenerator: Function
})

console.log(props.otia)

</script>

<template>
  <DataTable :value="props.otia">
    <Column header="Bezeichnung">
      <template #body="{data}">
        <Button variant="link" as="RouterLink" :to="linkGenerator(data)" :label="data.otium" />
      </template>
    </Column>
    <Column header="Raum" field="ort" />
    <Column header="Lehrer:in">
      <template #body="{data}">
        {{formatTutor(data.tutor)}}
      </template>
    </Column>
    <Column header="Auslastung">
      <template #body="{data}">
          <Tag class="w-full" v-if="data.maxEinschreibungen && data.maxEinschreibungen !== 0" :severity="chooseSeverity(data.auslastung, data.maxEinschreibungen)" >{{data.auslastung*100}} %</Tag>
          <Tag class="w-full" v-else severity="success">&infin;</Tag>
      </template>
    </Column>
    <template #empty>
      <div class="flex justify-center">
        Keine Angebote verfügbar.
      </div>
    </template>
  </DataTable>
</template>

<style scoped>
</style>
