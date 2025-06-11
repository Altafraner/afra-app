<script setup>
import {computed, ref, watch} from "vue";
import {useUser} from "@/stores/user.js";
import {Button, FloatLabel, InputNumber, InputText, ToggleSwitch, useToast} from "primevue";
import {mande} from "mande";
import NavBreadcrumb from "@/components/NavBreadcrumb.vue";
import {formatDate, formatPerson} from "@/helpers/formatters.js";
import Grid from "@/components/Form/Grid.vue";
import GridEditRow from "@/components/Form/GridEditRow.vue";
import AfraPersonSelector from "@/Otium/components/Form/AfraPersonSelector.vue";
import AfraOtiumEnrollmentTable from "@/Otium/components/Management/AfraOtiumEnrollmentTable.vue";
import {useAttendance} from "@/Otium/composables/attendanceHubClient.js";

const props = defineProps({
  terminId: String
})

const loading = ref(true);
const user = useUser()
const toast = useToast()
const otium = ref(null);

const aufsichtRunning = ref(false);

const maxEnrollmentsSetzenSelected = ref(false)
const maxEnrollmentsSelected = ref(null)
const betreuerZuweisenSelected = ref(false)
const ort = ref()
const personSelected = ref(null)
const updateStatusFunction = ref((studentId, status) => undefined);
const stopAufsicht = ref(() => undefined);

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

async function updateMaxEnrollments() {
  await simpleUpdate('maxEinschreibungen', maxEnrollmentsSetzenSelected.value ? maxEnrollmentsSelected.value : null)
}

async function updateTutor() {
  await simpleUpdate('tutor', betreuerZuweisenSelected.value ? personSelected.value : null)
}

async function updateOrt() {
  await simpleUpdate('ort', ort.value)
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

async function startAufsicht() {
  if (aufsichtRunning.value) return;
  aufsichtRunning.value = true;

  const aufsicht = useAttendance('termin', props.terminId, toast);
  const watcher = watch(aufsicht.attendance, (newValue) => {
    otium.value.einschreibungen = newValue;
  });
  updateStatusFunction.value = aufsicht.updateAttendance;
  stopAufsicht.value = async () => {
    if (!aufsichtRunning.value) return;
    aufsichtRunning.value = false;
    watcher.stop();
    await aufsicht.updateStatus(otium.value.blockId, true)
    await aufsicht.stop();
    await fetchData()

    updateStatusFunction.value = (studentId, status) => undefined;
  }
}

async function updateAttendanceCallback(student, status) {
  updateStatusFunction.value(student.id, status);
}

const startEditMaxEnrollments = () => {
  maxEnrollmentsSetzenSelected.value = otium.value.maxEinschreibungen !== null
  maxEnrollmentsSelected.value = otium.value.maxEinschreibungen
}

const startEditTutor = () => {
  betreuerZuweisenSelected.value = otium.value.tutor !== null
  personSelected.value = betreuerZuweisenSelected.value ? otium.value.tutor.id : null
}

const startEditOrt = () => {
  ort.value = otium.value.ort
}

await fetchData()
</script>

<template>
  <NavBreadcrumb :items="navItems"/>
  <h1>
    {{ otium.otium }}
  </h1>
  <grid>
    <GridEditRow header="Datum" hide-edit>
      <template #body>
        {{ formatDate(new Date(otium.datum)) }}
      </template>
    </GridEditRow>
    <GridEditRow header="Block" hide-edit>
      <template #body>
        {{ otium.block }}. Block
      </template>
    </GridEditRow>
    <GridEditRow header="Ort" @edit="startEditOrt" @update="updateOrt">
      <template #body>
        {{ otium.ort }}
      </template>
      <template #edit>
        <FloatLabel class="w-full" variant="on">
          <InputText id="ort" v-model="ort" fluid name="ort"/>
          <label for="ort">Ort</label>
        </FloatLabel>
      </template>
    </GridEditRow>
    <GridEditRow header="Betreuer:in" @edit="startEditTutor" @update="updateTutor">
      <template #body>
        <template v-if="otium.tutor===null">
          Kein:e Betreuer:in
        </template>
        <template v-else>
          {{ formatPerson(otium.tutor) }}
        </template>
      </template>
      <template #edit>
        <div class="w-full flex flex-col gap-3">
          <div class="flex justify-between">
            <label for="betreuerSwitch">Betreuer:in zuweisen</label>
            <ToggleSwitch v-model="betreuerZuweisenSelected" if="betreuerSwitch"/>
          </div>
          <AfraPersonSelector v-model="personSelected" :disabled="!betreuerZuweisenSelected"
                              name="tutor" required/>
        </div>
      </template>
    </GridEditRow>
    <GridEditRow header="max. Teilnehner:innen" @edit="startEditMaxEnrollments"
                 @update="updateMaxEnrollments">
      <template #body>
        {{ otium.maxEinschreibungen ? otium.maxEinschreibungen : "Unbegrenzt" }}
      </template>
      <template #edit>
        <div class="w-full flex flex-col gap-3">
          <div class="flex justify-between">
            <label for="maxEnrollmentSwitch">Teilnehmer:innen-Zahl beschränken</label>
            <ToggleSwitch v-model="maxEnrollmentsSetzenSelected"
                          if="maxEnrollmentSwitch"/>
          </div>
          <FloatLabel class="w-full" variant="on">
            <InputNumber id="maxEnrollmentInput" v-model="maxEnrollmentsSelected"
                         :disabled="!maxEnrollmentsSetzenSelected" fluid name="maxEnrollments"/>
            <label for="maxEnrollmentInput">max. Teilnehmer:innen</label>
          </FloatLabel>
        </div>
      </template>
    </GridEditRow>
  </grid>
  <div class="flex justify-between items-end gap-3 flex-wrap mt-3">
    <h2>Einschreibungen</h2>
    <Button v-if="!aufsichtRunning" icon="pi pi-eye" label="Aufsicht" severity="secondary"
            @click="startAufsicht"/>
    <Button v-else icon="pi pi-stop" label="Aufsicht beenden" severity="success"
            @click="stopAufsicht"/>
  </div>
  <AfraOtiumEnrollmentTable :enrollments="otium.einschreibungen"
                            :may-edit-attendance="aufsichtRunning"
                            :update-function="updateAttendanceCallback"
                            show-attendance/>
</template>

<style scoped>

</style>
