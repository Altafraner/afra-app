<script setup>
import {DataTable, Column, Badge, MeterGroup, Button} from "primevue";
import {ref} from "vue";
import {blockzeiten, kategorien, termin} from "@/helpers/testdata.js";
import AfraKategorieTag from "@/components/Otium/AfraKategorieTag.vue";
import {formatDate} from "@/helpers/formatters.js";

const props = defineProps({
  otiumId: String,
  date: String,
  block: Number
})

const times = ref(blockzeiten)
const otium = ref(termin)

function findKategorie(id, kategorien){
  const index = kategorien.findIndex((e) => e.id === id);
  if (index!==-1) {
    return kategorien[index]
  }

  for (const kategorie of kategorien ?? []) {
    const childResult = findKategorie(id, kategorie.children)
    if (childResult != null) return childResult
  }

  return null
}


const chooseColor = (now, max) => {
  if (max===0 || now <= 0.7) return 'var(--p-button-success-background)'
  if (now < 1) return 'var(--p-button-warn-background)'
  return 'var(--p-button-danger-background)'
}
</script>

<template>
  <h1>{{otium.bezeichnung}}</h1>
  <h3>{{props.block}}. Block, {{formatDate(new Date(props.date))}}</h3>
  <span class="inline-flex gap-1">
    <AfraKategorieTag v-for="tag in otium.kategorien" :value="findKategorie(tag, kategorien)" />
  </span>
  <p v-if="!props.minimal" v-for="beschreibung in otium.beschreibung.split('\n').filter(desc => desc)">
    {{ beschreibung }}</p>
  <DataTable :value="otium.einschreibungen">
    <Column header="Start">
      <template #body="{index}">
        {{times[index]}}
      </template>
    </Column>
    <Column header="Auslastung">
      <template #body="{data}">
        <div class="enrollmentGrid align-center gap-3">
          <Badge severity="secondary" :value="data.anzahl" />
          <MeterGroup v-if="otium.maxEinschreibungen!==0" :value="[{value: data.anzahl, color: chooseColor(data.anzahl / otium.maxEinschreibungen, otium.maxEinschreibungen), label: null}]" :max="otium.maxEinschreibungen" label-position="none" />
        </div>
      </template>
    </Column>
    <Column>
      <template #body="{data}">
        <Button v-if="data.eingeschrieben" icon="pi pi-times" severity="danger" size="small" variant="text" label="Austragen" />
        <Button v-else icon="pi pi-plus" size="small" variant="text" label="Einschreiben" :disabled="!data.kannBearbeiten"/>
      </template>
    </Column>
  </DataTable>
</template>

<style scoped>
.enrollmentGrid{
  display: inline-grid;
  grid-template-columns: auto 7rem;
  justify-content: center;
}
</style>
