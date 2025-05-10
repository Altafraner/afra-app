<script setup>
import {computed, ref} from "vue";
import {useUser} from "@/stores/useUser.js";
import {useToast} from "primevue";
import {mande} from "mande";
import AfraOtiumInstance from "@/components/Otium/Management/AfraOtiumInstance.vue";
import NavBreadcrumb from "@/components/NavBreadcrumb.vue";
import {formatDate} from "@/helpers/formatters.js";

const props = defineProps({
  terminId: String
})

const loading = ref(true);
const user = useUser()
const toast = useToast()
const otium = ref(null);
const navItems = computed(() => [
  {
    label: "Verwaltung",
    route: {
      name: "Verwaltung"
    }
  },
  {
    label: otium.value != null ? otium.value.otium : "",
    route: otium.value != null ? {
      name: "Verwaltung-Otium",
      params: {
        otiumId: otium.value.otiumId
      }
    } : null
  },
  {
    label: otium.value != null ? `${formatDate(new Date(otium.value.datum))} ${otium.value.block}. Block` : ""
  }
])

async function fetchData() {
  loading.value = true;
  const dataGetter = mande("/api/otium/management/termin/" + props.terminId);
  try {
    otium.value = await dataGetter.get();
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

async function updateMaxEnrollments(numEnrollments) {
  await simpleUpdate('maxEinschreibungen', numEnrollments)
}

async function updateTutor(tutor) {
  await simpleUpdate('tutor', tutor)
}

async function updateOrt(ort) {
  await simpleUpdate('ort', ort)
}

async function simpleUpdate(name, value) {
  const api = mande(`/api/otium/management/termin/${props.terminId}/${name}`);
  try {
    await api.patch({value})
  } catch (e) {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Es ist ein Fehler beim Aktualisieren der maximalen Teilnehmerzahl aufgetreten."
    })
    console.error(e)
  } finally {
    await fetchData();
  }
}

fetchData()
</script>

<template>
  <NavBreadcrumb :items="navItems"/>
  <AfraOtiumInstance v-if="!loading" :otium="otium" @update-max-enrollments="updateMaxEnrollments"
                     @update-ort="updateOrt" @update-tutor="updateTutor"/>
</template>

<style scoped>

</style>
