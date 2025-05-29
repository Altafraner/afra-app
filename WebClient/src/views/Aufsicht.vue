<script setup>
import {ref, Suspense} from "vue";
import {Button} from "primevue";
import AfraOtiumSupervisionView from "@/components/Otium/Supervision/AfraOtiumSupervisionView.vue";

const status = ref(false)

function start() {
  status.value = true
}

function stop() {
  status.value = false
}
</script>

<template>
  <div class="flex justify-between items-center">
    <h1>Aufsicht</h1>
    <Button v-if="!status" @click="start" icon="pi pi-play" label="Start"/>
    <!--Button v-else @click="stop" severity="danger" icon="pi pi-stop" label="Stop"/-->
  </div>

  <div v-if="!status">
    <p>Um ihre Aufsicht zu starten, drücken Sie auf Start.</p>
    <p>Mit dem Drücken auf Start bestätigen Sie, dass sie eingeteilte Aufsicht für den laufenden
      Otiums-Slot sind. Alle Änderungen, die Sie vornehmen, werden protokolliert.</p>
  </div>
  <div v-else>
    <Suspense>
      <afra-otium-supervision-view/>
      <template #fallback>
        <div class="flex justify-center">
          <span class="p-3">Lade Aufsicht...</span>
        </div>
      </template>
    </Suspense>
  </div>
</template>

<style scoped>

</style>
