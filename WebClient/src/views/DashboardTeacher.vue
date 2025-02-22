<script setup>
import {Button, Accordion, AccordionPanel, AccordionHeader, AccordionContent, DataTable, Column, Badge} from "primevue";
import {formatDate, formatTime, otiumKatalogLinkGenerator} from "../helpers/formatters.js";
import {personalOtiaEnrollments} from "@/helpers/testdata.js";
import {ref} from "vue";
import {useSettings} from "@/stores/useSettings.js";

const settings = useSettings()
const termine = ref(personalOtiaEnrollments);
const findBlock = startTime => {
  for (const block of settings.blocks) {
    if (startTime >= block.startTime && startTime < block.endTime) return block.id
  }

  console.error("start Time is in no Block", startTime)
}
</script>

<template>
  <h1>Dashboard</h1>
  <h2>Betreute Otia</h2>
  <DataTable :value="termine">
    <Column header="Otium">
      <template #body="{data}">
        <Button variant="link" as="RouterLink" :to="otiumKatalogLinkGenerator()">{{data.bezeichnung}}</Button>

      </template>
    </Column>
  </DataTable>

</template>

<style scoped>

</style>
