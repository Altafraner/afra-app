<script setup>
import {Button, useToast} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/user.js";
import StudentOverview from "@/Otium/components/Overview/StudentOverview.vue";

const loading = ref(true);
const user = useUser()
const toast = useToast()
const termine = ref(null);
const all = ref(false);

async function fetchData(getAll = false) {
  loading.value = true;
  const dataGetter = mande("/api/otium/student")
  try {
    termine.value = await (getAll ? dataGetter.get("all") : dataGetter.get());
    console.log(termine.value)
    all.value = getAll;
  } catch (e) {
    await user.update()
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Es ist ein Fehler beim Laden der Daten aufgetreten."
    })
    console.error(e)
  } finally {
    loading.value = false;
  }
}

fetchData()
</script>

<template>
  <h1>Hallo {{ user.user.vorname }}</h1>
  <!-- TODO: Introduce view for students that are tutors of otia. -->
  <h2 v-if="!all">Deine nächsten Veranstaltungen</h2>
  <p v-if="!all">Gezeigt werden die Veranstaltungen der nächsten drei Wochen.</p>
  <h2 v-if="all">Alle Veranstaltungen</h2>
  <StudentOverview :termine="termine" show-katalog/>
  <Button v-if="!all" class="mt-4" @click="fetchData(true)" label="Alle anzeigen"
          severity="secondary" :loading="loading"/>

</template>

<style scoped>

</style>
