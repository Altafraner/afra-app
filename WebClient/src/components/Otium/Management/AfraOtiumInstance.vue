<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {formatDate, formatPerson} from "@/helpers/formatters.js";
import Grid from "@/components/Form/Grid/Grid.vue";
import GridEditRow from "@/components/Form/Grid/GridEditRow.vue";
import {FloatLabel, InputNumber, InputText, ToggleSwitch} from "primevue";
import {ref} from "vue";
import AfraPersonSelector from "@/components/Form/AfraPersonSelector.vue";

const props = defineProps({
  otium: Object,
})

const emit = defineEmits(['update-max-enrollments', 'update-ort', 'update-tutor'])
const maxEnrollmentsSetzenSelected = ref(false)
const maxEnrollmentsSelected = ref(props.otium.maxEinschreibungen)
const betreuerZuweisenSelected = ref(false)
const ort = ref(props.otium.ort)
const personSelected = ref(null)

const startEditMaxEnrollments = () => {
  maxEnrollmentsSetzenSelected.value = props.otium.maxEinschreibungen !== null
  maxEnrollmentsSelected.value = props.otium.maxEinschreibungen
}

const startEditTutor = () => {
  betreuerZuweisenSelected.value = props.otium.tutor !== null
  personSelected.value = betreuerZuweisenSelected.value ? props.otium.tutor.id : null
}

const startEditOrt = () => {
  ort.value = props.otium.ort
}

const updateMaxEnrollments = () => {
  emit('update-max-enrollments', maxEnrollmentsSetzenSelected.value ? maxEnrollmentsSelected.value : null)
}

const updateOrt = () => {
  emit('update-ort', ort.value)
}

const updateTutor = () => {
  emit('update-tutor', betreuerZuweisenSelected.value ? personSelected.value : null)
}

</script>

<template>
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
  <h2>Einschreibungen</h2>
  <AfraOtiumEnrollmentTable :enrollments="otium.einschreibungen" show-attendance/>
</template>

<style scoped>

</style>
