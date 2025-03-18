<script setup>
import {
  Button,
  Skeleton,
  useToast
} from "primevue";
import {ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";
import StudentOverview from "@/components/Otium/Overview/StudentOverview.vue";
import {formatStudent} from "@/helpers/formatters.js";

const props = defineProps({
  studentId: String
})

const loading = ref(true);
const mentee = ref(null)
const user = useUser()
const toast = useToast()
const termine = ref(null);
const all = ref(false);

async function fetchData(getAll = false) {
  loading.value = true;
  const dataGetter = mande("/api/otium/student/" + props.studentId);
  try {
    const result = await (getAll ? dataGetter.get("all") : dataGetter.get());
    termine.value = result.termine;
    mentee.value = result.mentee;
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
  <template v-if="!loading">
    <h1>{{ formatStudent(mentee) }}</h1>
    <h2 v-if="!all">Nächste Veranstaltungen</h2>
    <p v-if="!all">Gezeigt werden die Veranstaltungen der nächsten drei Wochen.</p>
    <h2 v-if="all">Alle Veranstaltungen</h2>
    <StudentOverview :termine="termine"/>
    <Button v-if="!all" class="mt-4" @click="fetchData(true)" label="Alle anzeigen"
            severity="secondary" :loading="loading"/>
  </template>
  <div class="flex gap-3" v-else>
    <h1>
      <Skeleton/>
    </h1>
    <h2>
      <Skeleton/>
    </h2>
    <p>
      <Skeleton/>
    </p>
  </div>
</template>

<style scoped>

</style>
