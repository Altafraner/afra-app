<script setup>
import {formatDate} from "@/helpers/formatters.js";
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
  Badge,
  Button,
  Column,
  DataTable
} from "primevue";

const props = defineProps({
  termine: Array,
  showKatalog: Boolean
})

</script>

<template>
  <Accordion v-if="props.termine != null">
    <AccordionPanel v-for="termin in props.termine" :key="termin.datum" :value="termin.datum">
      <AccordionHeader>
        <div class="flex w-full justify-between mr-4">
          <span>
            {{ formatDate(new Date(termin.datum)) }}
          </span>
          <span class="flex flex-row gap-3">
            <Badge class="w-[8rem]" v-if="termin.vollstaendig && termin.kategorienErfuellt"
                   severity="secondary">Ok</Badge>
            <Badge class="w-[8rem]" v-else-if="termin.vollstaendig && !termin.kategorienErfuellt"
                   severity="warn">Kategorien fehlen</Badge>
            <Badge v-else class="w-[8rem]" severity="danger">Auffällig</Badge>
          </span>
        </div>
      </AccordionHeader>
      <AccordionContent>
        <DataTable :value="termin.einschreibungen">
          <Column header="Otium">
            <template #body="{data}">
              <Button :label="data.otium" as="RouterLink" class="w-[8rem]" variant="text"
                      :to="`/termin/${data.terminId}`"/>
            </template>
          </Column>
          <Column field="ort" header="Ort"/>
          <Column field="interval.start" header="Start"/>
          <Column field="interval.duration" header="Dauer"/>
          <template #empty>
            <div class="flex justify-center">
              Keine Einträge
            </div>
          </template>
          <template #footer>
            <div class="flex flex-row justify-between items-center">
              <Button v-if="props.showKatalog" class="w-[8rem]" size="small" as="RouterLink"
                      :to="'/katalog/' + termin.datum" label="Katalog"/>
              <span v-else/>
              <span class="flex flex-row gap-3 mr-[var(--p-icon-size)] flex-wrap justify-end">
                <Badge class="w-[8rem]" v-if="!termin.kategorienErfuellt" severity="warn">Kategorien fehlen</Badge>
                <Badge class="w-[8rem]" v-if="!termin.vollstaendig"
                       severity="danger">Unvollständig</Badge>
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
