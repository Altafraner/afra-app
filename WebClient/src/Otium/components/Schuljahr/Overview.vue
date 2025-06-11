<script setup>

import {useOtiumStore} from "@/Otium/stores/otium.js";
import {Button, Column, DataTable, useConfirm, useDialog, useToast} from "primevue";
import {mande} from "mande";
import CreateSchoolday from "@/Otium/components/Schuljahr/CreateSchoolday.vue";

const settings = useOtiumStore();
const dialog = useDialog();
const confirm = useConfirm();
const toast = useToast();

async function setup() {
  await settings.updateSchuljahr(true);
}

function addDay() {
  dialog.open(CreateSchoolday, {
    props: {
      modal: true,
      header: "Tag hinzufügen",
    },
    emits: {
      onUpdate: () => {
        console.log("Received update event");
        settings.updateSchuljahr(true);
      }
    }
  })
}

function updateDay(data) {
  dialog.open(CreateSchoolday, {
    data: {
      initialValues: data
    },
    props: {
      modal: true,
      header: "Tag bearbeiten",
    },
    emits: {
      onUpdate: () => {
        console.log("Received update event");
        settings.updateSchuljahr(true);
      }
    }
  })
}

function deleteDay(event, data) {
  confirm.require({
    target: event.currentTarget,
    message: 'Möchten Sie den Tag wirklich löschen?',
    header: 'Tag löschen',
    icon: 'pi pi-exclamation-triangle',
    acceptProps: {
      label: 'Ja',
      severity: 'danger'
    },
    rejectProps: {
      label: 'Nein',
      severity: 'secondary'
    },
    accept: async () => {
      const api = mande('/api/management/schuljahr/' + data.datum);
      try {
        await api.delete();

      } catch (error) {
        console.error(error);
        toast.add({
          severity: 'error',
          summary: 'Fehler',
          detail: 'Der Tag konnte nicht gelöscht werden.',
        });
      } finally {
        await settings.updateSchuljahr(true);
      }
    }
  });
}

setup();
</script>

<template>
  <h2>Schultage</h2>
  <p>Hier können Sie die Schultage in diesem Schuljahr verwalten. Sie können auch
    <Button :to="{name: 'Verwaltung-Schuljahr-Neu'}" as="RouterLink" class="p-0 hover:underline"
            variant="link">
      mehrere Termine anlegen.
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
        <Button v-tooltip="'Tag hinzufügen'" icon="pi pi-plus" size="small" @click="addDay"/>
      </template>
      <template #body="{data}">
        <Button v-tooltip="'Bearbeiten'" icon="pi pi-pencil" severity="secondary" size="small"
                variant="text" @click="() => updateDay(data)"/>
        <Button v-tooltip="'Löschen'" icon="pi pi-times" severity="danger" size="small"
                variant="text" @click="(evt) => deleteDay(evt, data)"/>
      </template>
    </Column>
    <template #empty>
      Keine Schultage angelegt.
    </template>
  </DataTable>
</template>

<style scoped>

</style>
