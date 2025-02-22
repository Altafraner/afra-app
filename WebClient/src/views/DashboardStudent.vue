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
  <!-- TODO: Introduce view for students that are tutors of otia. -->
  <div class="flex justify-between align-center">
    <h2>Nächste Veranstaltungen</h2>
    <Button as="RouterLink" to="/katalog" label="Katalog"/>
  </div>
  <Accordion>
    <AccordionPanel v-for="termin in termine" :key="termin.datum">
      <AccordionHeader>
        <div class="flex width-fill justify-between" style="margin-right: 1rem">
          <span>
            {{ formatDate(termin.datum) }}
          </span>
          <Badge v-if="termin.okay" severity="success">Okay</Badge>
          <Badge v-else severity="danger">Unvollständig</Badge>
        </div>
      </AccordionHeader>
      <AccordionContent>
        <DataTable :value="termin.einschreibungen">
          <Column header="Otium">
            <template #body="{data}">
              <Button :label="data.bezeichnung" variant="link" as="RouterLink" :to="otiumKatalogLinkGenerator(data.otiumId, termin.datum, findBlock(data.start))"/>
            </template>
          </Column>
          <Column field="ort" header="Ort" />
          <Column v-for="zeit in [{field: 'start', header: 'Start'}, {field: 'ende', header: 'Ende'}]" :header="zeit.header">
            <template #body="{data}">
              {{formatTime(data[zeit.field])}}
            </template>
          </Column>
        </DataTable>
      </AccordionContent>
    </AccordionPanel>
  </Accordion>

</template>

<style scoped>

</style>
