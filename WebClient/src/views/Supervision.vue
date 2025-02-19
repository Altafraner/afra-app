<script setup>

import {ref} from "vue";
import {Button} from "primevue";
import AfraOtiumSupervisionView from "@/components/Otium/AfraOtiumSupervisionView.vue";

const status = ref(false)

const rooms=ref([{
  id: "1",
  bezeichnung: "Studienzeit Mathematik",
  ort: "110",
  einschreibungen: [
    {
      start: new Date(0,0,0,12,0),
      ende: new Date(0,0,0,12,20),
      student: {
        vorname: "Homer",
        nachname: "Simpson"
      },
      verified: 1
    },
    {
      start: new Date(0,0,0,12,0),
      ende: new Date(0,0,0,12,20),
      student: {
        vorname: "Maggie",
        nachname: "Simpson"
      },
      verified: 0
    }
  ]
}])

function start(){
  status.value = true
}

function stop(){
  status.value = false
}
</script>

<template>
  <div class="flex justify-between align-center">
    <h1>Aufsicht</h1>
    <Button v-if="!status" @click="start" icon="pi pi-play" label="Start"/>
    <Button v-else @click="stop" severity="danger" icon="pi pi-stop" label="Stop"/>
  </div>

  <div v-if="!status">
    <p>Um ihre Aufsicht zu starten, drücken Sie auf Start.</p>
    <p>Mit dem Drücken auf Start bestätigen Sie, dass sie eingeteilte Aufsicht für den laufenden Otiums-Slot sind. Alle Änderungen, die Sie vornehmen, werden protokolliert.</p>
  </div>
  <div v-else>
    <afra-otium-supervision-view :rooms="rooms" />
  </div>
</template>

<style scoped>

</style>
