<script setup>
import {ref} from "vue";
import {useUser} from "@/stores/useUser.js";
import {useToast} from "primevue";
import {mande} from "mande";
import AfraOtiumInstance from "@/components/Otium/Management/AfraOtiumInstance.vue";

const props = defineProps({
  terminId: String
})

const loading = ref(true);
const user = useUser()
const toast = useToast()
const otium = ref(null);

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
  const api = mande(`/api/otium/management/termin/${props.terminId}/maxEinschreibungen`);
  try {
    await api.patch({value: numEnrollments})
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
  <AfraOtiumInstance v-if="!loading" :otium="otium" @update-max-enrollments="updateMaxEnrollments"/>
</template>

<style scoped>

</style>
