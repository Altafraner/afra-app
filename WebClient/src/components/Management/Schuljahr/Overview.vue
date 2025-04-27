<script setup>

import {useSettings} from "@/stores/useSettings.js";
import {Column, DataTable} from "primevue";

const settings = useSettings();

async function setup() {
  await settings.updateSchuljahr();
}

setup();
</script>

<template>
  <h2>Schuljahr</h2>
  <DataTable :value="settings.schuljahr">
    <Column header="Datum">
      <template #body="{data}">{{
          new Date(data.datum).toLocaleDateString('de-DE', {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
          })
        }}
      </template>
    </Column>
    <Column header="Tag">
      <template #body="{data}">{{
          new Date(data.datum).toLocaleDateString('de-DE', {
            weekday: "long",
          })
        }}
      </template>
    </Column>
    <Column field="wochentyp" header="Wochentyp"/>
    <Column header="Blöcke">
      <template #body="{data}">
        {{ data.blocks.sort().join(', ') }}
      </template>
    </Column>
    <template #empty>
      Keine Schultage angelegt.
    </template>
  </DataTable>
</template>

<style scoped>

</style>
