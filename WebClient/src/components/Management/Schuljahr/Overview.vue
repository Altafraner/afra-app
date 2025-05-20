<script setup>

import {useSettings} from "@/stores/useSettings.js";
import {Button, Column, DataTable} from "primevue";

const settings = useSettings();

async function setup() {
  await settings.updateSchuljahr();
}

setup();
</script>

<template>
  <h2>Schultage</h2>
  <p>Hier können Sie die Schultage in diesem Schuljahr verwalten. Sie können auch
    <Button :to="{name: 'Verwaltung-Schuljahr-Neu'}" as="RouterLink" class="p-0 hover:underline"
            variant="link">
      mehrere Termine
      anlegen.
    </Button>
  </p>
  <DataTable :value="settings.schuljahr">
    <Column header="Datum">
      <template #body="{data}">{{
          new Date(data.datum).toLocaleDateString('de-DE', {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
          })
        }}
        ({{
          new Date(data.datum).toLocaleDateString('de-DE', {
            weekday: "short",
          })
        }})
      </template>
    </Column>
    <Column field="wochentyp" header="Wochentyp"/>
    <Column header="Blöcke">
      <template #body="{data}">
        {{ data.blocks.sort().join(', ') }}
      </template>
    </Column>
    <Column class="afra-col-action text-right">
      <template #header>
        <Button v-tooltip="'Tag hinzufügen'" icon="pi pi-plus" size="small"/>
      </template>
      <template #body>
        <Button v-tooltip="'bearbeiten'" icon="pi pi-pencil" severity="secondary" size="small"
                variant="text"/>
      </template>
    </Column>
    <template #empty>
      Keine Schultage angelegt.
    </template>
  </DataTable>
</template>

<style scoped>

</style>
