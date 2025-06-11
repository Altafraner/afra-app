<script setup>
import {Button, Skeleton, useToast} from "primevue";
import {computed, ref} from "vue";
import {mande} from "mande";
import {useUser} from "@/stores/user.js";
import StudentOverview from "@/Otium/components/Overview/StudentOverview.vue";
import {formatStudent} from "@/helpers/formatters.js";
import NavBreadcrumb from "@/components/NavBreadcrumb.vue";

const props = defineProps({
  studentId: String
})

const loading = ref(true);
const mentee = ref(null)
const user = useUser()
const toast = useToast()
const termine = ref(null);
const all = ref(false);

const username = computed(() => {
  if (user.user) {
    return formatStudent(mentee.value);
  } else {
    return null;
  }
});

const navItems = ref([
  {
    label: "Mentees"
  }, {
    label: username
  }
])

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
    <NavBreadcrumb :items="navItems"/>
    <h1>{{ formatStudent(mentee) }}</h1>
    <h2 v-if="!all">Nächste Veranstaltungen</h2>
    <p v-if="!all">Gezeigt werden die Veranstaltungen der nächsten drei Wochen.</p>
    <h2 v-if="all">Alle Veranstaltungen</h2>
    <StudentOverview :student="mentee" :termine="termine"/>
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
