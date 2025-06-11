<script setup>
import {ref, watch} from "vue";
import {Button, FloatLabel, InputNumber, InputText, Message, Select, ToggleSwitch} from "primevue";
import {Form} from '@primevue/forms';
import {useOtiumStore} from "@/Otium/stores/otium.js";
import {formatTutor} from "@/helpers/formatters.js";
import AfraPersonSelector from "@/Otium/components/Form/AfraPersonSelector.vue";
import AfraDateSelector from "@/Otium/components/Form/AfraDateSelector.vue";

const emit = defineEmits(["submit"]);

const settings = useOtiumStore();

const dates = ref([])
const ortSelected = ref(null)
const dateSelected = ref(null)
const blockSelected = ref(null)
const personSelected = ref(null)
const maxEnrollmentsSelected = ref(null)
const maxEnrollmentsSetzenSelected = ref(false)
const betreuerZuweisenSelected = ref(false)
const loading = ref(true)
const personen = ref(null)

function resolve({values}) {
  const errors = {}

  if (!values.ort)
    errors.ort = [{message: "Es muss ein Ort gesetzt sein"}]

  return {values, errors}
}

async function getTermine() {
  await settings.updateSchuljahr();
  dates.value = settings.schuljahr
  dateSelected.value = settings.defaultDay
  dateChanged()
}

async function getPersonen() {
  const personenMapper = (person) => {
    return {
      id: person.id,
      name: `${formatTutor(person)} (${person.rolle})`
    }
  }

  await settings.updatePersonen();
  personen.value = settings.personen.map(personenMapper);
}

function dateChanged() {
  if (dateSelected.value.blocks.includes(blockSelected.value)) return;
  blockSelected.value = dateSelected.value.blocks[0]
}

async function setup() {
  loading.value = true
  personSelected.value = null;
  blockSelected.value = null;
  dateSelected.value = null;
  ortSelected.value = null;
  maxEnrollmentsSelected.value = null;
  maxEnrollmentsSetzenSelected.value = false;
  betreuerZuweisenSelected.value = false;
  const personPromise = getPersonen()
  const terminePromise = getTermine()

  await Promise.all([personPromise, terminePromise])
  loading.value = false
}

function submit({valid}) {
  if (!valid) return
  emit("submit", {
    date: dateSelected.value.datum,
    block: blockSelected.value,
    ort: ortSelected.value,
    person: personSelected.value,
    maxEnrollments: maxEnrollmentsSelected.value
  })
}

watch(dateSelected, dateChanged);
watch(betreuerZuweisenSelected, () => {
  if (!betreuerZuweisenSelected.value) {
    personSelected.value = null
  }
})
watch(maxEnrollmentsSetzenSelected, () => {
  if (!maxEnrollmentsSetzenSelected.value) {
    maxEnrollmentsSelected.value = null
  }
})

setup()
</script>

<template>
  <Form v-if="!loading" v-slot="$form" :resolver="resolve" class="flex flex-col gap-3"
        @submit="submit">
    <div class="font-bold">Zeitpunkt</div>
    <AfraDateSelector v-if="!loading" v-model="dateSelected" :options="dates" hide-today
                      name="date"/>
    <Select v-model="blockSelected" :options="dateSelected.blocks" name="block">
      <template #value="{value}">
        {{ value }}. Block
      </template>
      <template #option="{option}">
        {{ option }}. Block
      </template>
    </Select>
    <div class="font-bold mt-4">Details</div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <InputText id="ort" v-model="ortSelected" fluid name="ort"/>
        <label for="ort">Ort</label>
      </FloatLabel>
      <Message v-if="$form.ort?.invalid" severity="error" size="small" variant="simple">
        {{ $form.ort.error.message }}
      </Message>
    </div>
    <div class="flex justify-between mt-4">
      <label for="betreuerSwitch">Betreuer:in zuweisen</label>
      <ToggleSwitch v-model="betreuerZuweisenSelected" if="betreuerSwitch"/>
    </div>
    <AfraPersonSelector id="betreuerSelect" v-model="personSelected"
                        :disabled="!betreuerZuweisenSelected"
                        name="tutor" required/>
    <div class="flex justify-between mt-4">
      <label for="maxEnrollmentSwitch">Teilnehmer:innen-Zahl beschränken</label>
      <ToggleSwitch v-model="maxEnrollmentsSetzenSelected"
                    if="maxEnrollmentSwitch"/>
    </div>
    <FloatLabel class="w-full" variant="on">
      <InputNumber id="maxEnrollmentInput" v-model="maxEnrollmentsSelected"
                   :disabled="!maxEnrollmentsSetzenSelected" fluid name="maxEnrollments"/>
      <label for="maxEnrollmentInput">max. Teilnehmer:innen</label>
    </FloatLabel>
    <Button class="mt-4" label="Erstellen" severity="primary" type="submit"/>
  </Form>
</template>

<style scoped>

</style>
