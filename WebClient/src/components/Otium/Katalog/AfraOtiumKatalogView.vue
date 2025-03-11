<script setup>

import {DataTable, Column, Button, Tag} from "primevue";
import {chooseSeverity, formatPerson, formatTutor} from "@/helpers/formatters.js"

const props = defineProps({
  otia: Array,
  linkGenerator: Function
})

</script>

<template>
  <DataTable :value="props.otia">
    <Column header="Bezeichnung">
      <template #body="{data}">
        <Button v-if="data.istAbgesagt" variant="link" :label="data.otium" disabled/>
        <Button v-else variant="link" as="RouterLink" :to="linkGenerator(data)" :label="data.otium" :disabled="data.istAbgesagt"/>
      </template>
    </Column>
    <Column header="Raum" field="ort" />
    <Column header="Lehrer:in">
      <template #body="{data}">
        {{data.tutor ? formatPerson(data.tutor) : ''}}
      </template>
    </Column>
    <Column header="Auslastung">
      <template #body="{data}">
          <Tag class="w-full" v-if="data.istAbgesagt" severity="danger">Abgesagt</Tag>
          <Tag class="w-full" v-else-if="data.maxEinschreibungen && data.maxEinschreibungen !== 0" :severity="chooseSeverity(data.auslastung, data.maxEinschreibungen)" >{{data.auslastung*100}} %</Tag>
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
