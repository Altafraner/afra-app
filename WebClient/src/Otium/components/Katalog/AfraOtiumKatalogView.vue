﻿<script setup>

import {Button, Column, DataTable, Skeleton} from "primevue";
import {formatPerson} from "@/helpers/formatters.js"
import AuslastungsTag from "@/Otium/components/Shared/AuslastungsTag.vue";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import AfraKategorieTag from "@/Otium/components/Shared/AfraKategorieTag.vue";
import Termin from "@/Otium/components/Katalog/Termin.vue";
import {ref} from "vue";

const props = defineProps({
  otia: Array,
  terminId: {
    type: String,
    required: false,
    default: undefined
  }
})

const settings = useOtiumStore();
const rowsExpanded = ref({});
if (props.terminId) {
  rowsExpanded.value[props.terminId] = true;
}

function findKategorie(otium) {
  return settings.kategorien.find(k => otium.kategorien.includes(k.id));
}

function expand(id) {
  let isOpen = rowsExpanded.value[id] ?? false;
  let temp = {};
  if (!isOpen)
    temp[id] = true;
  rowsExpanded.value = temp;
}

await settings.updateKategorien();

</script>

<template>
  <DataTable v-model:expanded-rows="rowsExpanded" :pt="{ rowGroupHeaderCell: { colspan: 4 }}"
             :value="props.otia"
             data-key="id"
             group-rows-by="block" row-group-mode="subheader">
    <Column header="Bezeichnung">
      <template #body="{data}">
        <Button v-if="data.istAbgesagt" disabled variant="text">
          <afra-kategorie-tag v-if="findKategorie(data)" :value="findKategorie(data)"
                              hide-name minimal/>
          <span class="font-semibold text-left">{{ data.otium }}</span>
        </Button>
        <Button v-else :disabled="data.istAbgesagt" :label="data.otium" variant="text"
                @click="() => expand(data.id)">
          <i :class="(rowsExpanded[data.id] ?? false) ? 'pi pi-angle-down text-lg' : 'pi pi-angle-right text-lg'"/>
          <afra-kategorie-tag v-if="findKategorie(data)" :value="findKategorie(data)"
                              hide-name minimal/>
          <span class="font-semibold text-left">{{ data.otium }}</span>
        </Button>
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
        <div class="w-[6rem]">
          <AuslastungsTag :auslastung="data.auslastung" :ist-abgesagt="data.istAbgesagt"/>
        </div>
      </template>
    </Column>
    <template #expansion="{data}">
      <Suspense>
        <Termin :termin-id="data.id"/>
        <template #fallback>
          <div>
            <h1>
              <Skeleton height="3rem" width="60%"/>
            </h1>
            <p>
              <Skeleton width="40%"/>
            </p>
            <h3 class="mt-[3rem]">
              <Skeleton height="2rem" width="55%"/>
            </h3>
          </div>
        </template>
      </Suspense>
    </template>
    <template #groupheader="{data}">
      <h3 class="text-base ml-4">Block {{ data.block }}</h3>
    </template>
    <template #empty>
      <div class="flex justify-center">
        Keine Angebote verfügbar.
      </div>
    </template>
  </DataTable>
</template>

<style scoped>
</style>
