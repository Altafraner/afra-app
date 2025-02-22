<script setup>

import {DataTable, Column, Button, Tag} from "primevue";
import MeterGroup from "primevue/metergroup";
import {chooseColor, chooseSeverity, formatTutor} from "@/helpers/formatters.js"

const props = defineProps({
  otia: Array,
  linkGenerator: Function
})

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
          <Tag class="width-fill" v-if="data.maxEinschreibungen !== 0" :severity="chooseSeverity(data.auslastung, data.maxEinschreibungen)" >{{data.auslastung*100}} %</Tag>
          <Tag class="width-fill" v-else severity="success">&infin;</Tag>
      </template>
    </Column>
  </DataTable>
</template>

<style scoped>
</style>
