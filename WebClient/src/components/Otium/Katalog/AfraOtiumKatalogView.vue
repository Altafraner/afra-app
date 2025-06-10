<script setup>

import {Button, Column, DataTable} from "primevue";
import {formatPerson} from "@/helpers/formatters.js"
import AuslastungsTag from "@/components/Otium/Shared/AuslastungsTag.vue";
import {useSettings} from "@/stores/useSettings.js";
import AfraKategorieTag from "@/components/Otium/Shared/AfraKategorieTag.vue";

const props = defineProps({
  otia: Array,
  linkGenerator: Function
})

const settings = useSettings();

function findKategorie(otium) {
  return settings.kategorien.find(k => otium.kategorien.includes(k.id));
}

await settings.updateKategorien();

</script>

<template>
  <DataTable :value="props.otia">
    <Column header="Bezeichnung">
      <template #body="{data}">
        <afra-kategorie-tag v-if="findKategorie(data)" :value="findKategorie(data)"
                            hide-name minimal/>
        <Button v-if="data.istAbgesagt" :label="data.otium" disabled variant="text"/>
        <Button v-else :label="data.otium" :to="linkGenerator(data)" as="RouterLink" variant="text"
                :disabled="data.istAbgesagt"/>
      </template>
    </Column>
    <Column header="Block">
      <template #body="{data}">
        {{ data.block }}
      </template>
    </Column>
    <Column header="Raum" field="ort"/>
    <Column header="Betreuer:in">
      <template #body="{data}">
        {{ data.tutor ? formatPerson(data.tutor) : '' }}
      </template>
    </Column>
    <Column header="Auslastung">
      <template #body="{data}">
        <AuslastungsTag :ist-abgesagt="data.istAbgesagt" :auslastung="data.auslastung"/>
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
