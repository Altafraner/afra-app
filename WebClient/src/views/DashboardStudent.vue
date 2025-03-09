<script setup>
import {Button, Accordion, AccordionPanel, AccordionHeader, AccordionContent, DataTable, Column, Badge} from "primevue";
import {formatDate} from "../helpers/formatters.js";
import {ref} from "vue";
import {useSettings} from "@/stores/useSettings.js";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";

const settings = useSettings()
const user = useUser()
const termine = ref(null);
const findBlock = startTime => {
  for (const block of settings.blocks) {
    if (startTime >= block.startTime && startTime < block.endTime) return block.id
  }

  console.error("start Time is in no Block", startTime)
}

async function fetchData() {
  const dataGetter = mande("/api/otium/dashboard")
  try {
    termine.value = await dataGetter.get();
  } catch (e) {
    await user.update()
    console.error(e)
  }
}

fetchData()
</script>

<template>
  <h1>Dashboard</h1>
  <!-- TODO: Introduce view for students that are tutors of otia. -->
  <h2>Nächste Veranstaltungen</h2>
  <Accordion v-if="termine != null">
    <AccordionPanel v-for="termin in termine" :key="termin.datum" :value="termin.datum">
      <AccordionHeader>
        <div class="flex w-full justify-between mr-4">
          <span>
            {{ formatDate(new Date(termin.datum)) }}
          </span>
          <span class="flex flex-row gap-3">
            <Badge class="w-[8rem]" v-if="termin.vollstaendig && termin.kategorienErfuellt" severity="secondary">Ok</Badge>
            <Badge class="w-[8rem]" v-else-if="termin.vollstaendig && !termin.kategorienErfuellt" severity="warn">Kategorien fehlen</Badge>
            <Badge class="w-[8rem]" v-else severity="danger">Nicht Ok</Badge>
          </span>
        </div>
      </AccordionHeader>
      <AccordionContent>
        <DataTable :value="termin.einschreibungen">
          <Column header="Otium">
            <template #body="{data}">
              <Button class="w-[8rem]" :label="data.otium" variant="link" as="RouterLink" :to="`/termin/${data.terminId}`"/>
            </template>
          </Column>
          <Column field="ort" header="Ort" />
          <Column field="interval.start" header="Start" />
          <Column field="interval.duration" header="Dauer" />
          <template #empty>
            <div class="flex justify-center">
              Keine Einträge
            </div>
          </template>
          <template #footer>
            <div class="flex flex-row justify-between items-center">
              <Button class="w-[8rem]" size="small" as="RouterLink" :to="'/katalog/' + termin.datum" label="Katalog" />
              <span class="flex flex-row gap-3 mr-[var(--p-icon-size)] flex-wrap justify-end">
                <Badge class="w-[8rem]" v-if="!termin.kategorienErfuellt" severity="warn">Kategorien fehlen</Badge>
                <Badge class="w-[8rem]" v-if="!termin.vollstaendig" severity="danger">Unvollständig</Badge>
              </span>
            </div>
          </template>
        </DataTable>
      </AccordionContent>
    </AccordionPanel>
  </Accordion>

</template>

<style scoped>

</style>
