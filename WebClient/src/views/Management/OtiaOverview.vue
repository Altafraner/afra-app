<script setup>
import {useUser} from "@/stores/useUser.js";
import {useSettings} from "@/stores/useSettings.js";
import {Button, Column, DataTable, Skeleton, useToast} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import AfraKategorieTag from "@/components/Otium/Shared/AfraKategorieTag.vue";
import {findPath} from "@/helpers/tree.js";
import SimpleBreadcrumb from "@/components/SimpleBreadcrumb.vue";
import {RouterLink} from "vue-router";

const user = useUser();
const settings = useSettings();
const toast = useToast();

const loading = ref(true);

const otia = ref([]);

async function getOtia() {
  const getter = mande('/api/otium/management/otium');
  otia.value = await getter.get();
}

async function setup() {
  try {
    await settings.updateKategorien();
    await getOtia();
    loading.value = false;
  } catch {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
    });
  }
}

setup();
</script>

<template>
  <template v-if="user.user.rolle !== 'Tutor'">
    <h1>Sie sind nicht Autorisiert, diese Seite zu nutzen.</h1>
  </template>
  <template v-else-if="!loading">
    <h1>Alle Otia</h1>
    <p>Klicken sie auf ein Otium, um Details zu sehen oder es zu Bearbeiten.</p>
    <DataTable :value="otia">
      <Column header="Bezeichnung">
        <template #body="{data}">
          <Button :as="RouterLink" :label="data.bezeichnung"
                  :to="{name: 'Verwaltung-Otium', params: {otiumId: data.id}}" variant="text"/>
        </template>
      </Column>
      <Column header="Kategorie">
        <template #body="{data}">
          <SimpleBreadcrumb :model="findPath(settings.kategorien, data.kategorie)" wrap>
            <template #item="{item}">
              <AfraKategorieTag :value="item" minimal/>
            </template>
          </SimpleBreadcrumb>
        </template>
      </Column>
      <Column header="Termine">
        <template #body="{data}">
          {{ data.termine.length }}
        </template>
      </Column>
    </DataTable>
  </template>
  <template v-else>
    <Skeleton class="mb-6" height="3rem"/>
    <Skeleton class="mb-4"/>
    <DataTable :value="new Array(10)">
      <Column v-for="_ in new Array(3)">
        <template #body>
          <Skeleton/>
        </template>
        <template #header>
          <Skeleton height="1.5em"/>
        </template>
      </Column>
    </DataTable>
  </template>
</template>

<style scoped>

</style>
