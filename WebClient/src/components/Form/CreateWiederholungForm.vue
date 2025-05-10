<script setup>
import {ref, watch} from "vue";
import {Button, FloatLabel, InputNumber, InputText, Message, Select, ToggleSwitch} from "primevue";
import {Form} from '@primevue/forms';
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import {useSettings} from "@/stores/useSettings.js";
import {formatDayOfWeek, formatTutor} from "@/helpers/formatters.js";

const emit = defineEmits(["submit"]);

const settings = useSettings();

const dates = ref([])
const datesAvailable = ref([])
const ort = ref(null)
const von = ref(null)
const bis = ref(null)
const wochentyp = ref(null)
const wochentag = ref(null)
const block = ref(null)
const personSelected = ref(null)
const maxEnrollmentsSelected = ref(null)
const maxEnrollmentsSetzenSelected = ref(false)
const betreuerZuweisenSelected = ref(false)
const loading = ref(true)
const personen = ref(null)

function resolve({values}) {
  const errors = {}

  if (!values.wochentyp)
    errors.wochentyp = [{message: "Es muss ein Wochentyp gesetzt sein"}]

  if (!values.wochentag)
    errors.wochentag = [{message: "Es muss ein Wochentag gesetzt sein"}]

  if (!values.block && values.block !== 0)
    errors.block = [{message: "Es muss ein Block gesetzt sein"}]

  if (!values.ort)
    errors.ort = [{message: "Es muss ein Ort gesetzt sein"}]

  return {values, errors}
}

async function getTermine() {
  await settings.updateSchuljahr();
  dates.value = settings.schuljahr
  von.value = settings.defaultDay
  bis.value = settings.schuljahr[settings.schuljahr.length - 1]
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

async function setup() {
  const personPromise = getPersonen()
  const terminePromise = getTermine()

  await Promise.all([personPromise, terminePromise])
  blockOrWochentagChanged()
  loading.value = false
}

function blockOrWochentagChanged() {
  const now = new Date(new Date().toDateString());
  datesAvailable.value = dates.value.filter(date => {
    const datum = new Date(date.datum)
    return (block.value == null || date.blocks.includes(block.value)) &&
      datum >= now &&
      (wochentag.value == null || datum.getDay() === wochentag.value) &&
      (wochentyp.value == null || date.wochentyp === wochentyp.value)
  })
  if (!datesAvailable.value.includes(von.value)) {
    von.value = datesAvailable.value[0]
  }
  if (!datesAvailable.value.includes(bis.value)) {
    bis.value = datesAvailable.value[datesAvailable.value.length - 1]
  }
}

function submit({valid}) {
  if (!valid) {
    console.log("Invalid state")
    return
  }
  emit("submit", {
    wochentyp: wochentyp.value,
    wochentag: wochentag.value,
    von: von.value.datum,
    bis: bis.value.datum,
    block: block.value,
    ort: ort.value,
    person: personSelected.value,
    maxEnrollments: maxEnrollmentsSelected.value
  })
}

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
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <Select id="wochentyp" v-model="wochentyp" :options="['H-Woche', 'N-Woche']"
                fluid
                name="wochentyp" @change="blockOrWochentagChanged"/>
        <label for="wochentyp">Wochentyp</label>
      </FloatLabel>
      <Message v-if="$form.wochentyp?.invalid" severity="error" size="small" variant="simple">
        {{ $form.wochentyp.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <Select id="wochentag" v-model="wochentag" :options="[1, 5]" fluid
                name="wochentag" @change="blockOrWochentagChanged">
          <template #value="{value}">
            <template v-if="value!=null">
              {{ formatDayOfWeek(value) }}
            </template>
          </template>
          <template #option="{option}">
            {{ formatDayOfWeek(option) }}
          </template>
        </Select>
        <label for="wochentag">Wochentag</label>
      </FloatLabel>
      <Message v-if="$form.wochentag?.invalid" severity="error" size="small" variant="simple">
        {{ $form.wochentag.error.message }}
      </Message>
    </div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <Select id="block" v-model="block" :options="['1', '2']" fluid
                name="block" @change="blockOrWochentagChanged">
          <template #value="{value}">
            <template v-if="value || value === 0">
              {{ value }}. Block
            </template>
          </template>
          <template #option="{option}">
            {{ option }}. Block
          </template>
        </Select>
        <label for="block">Block</label>
      </FloatLabel>
      <Message v-if="$form.block?.invalid" severity="error" size="small" variant="simple">
        {{ $form.block.error.message }}
      </Message>
    </div>

    <div class="font-bold">Zeitraum</div>
    <AfraDateSelector v-if="!loading" v-model="von" :options="datesAvailable" hide-today
                      label="Von" name="von" show-label/>
    <AfraDateSelector v-if="!loading" v-model="bis" :options="datesAvailable" hide-today
                      label="Bis" name="bis" show-label/>

    <div class="font-bold mt-4">Details</div>
    <div class="w-full">
      <FloatLabel class="w-full" variant="on">
        <InputText id="ort" v-model="ort" fluid name="ort"/>
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
    <FloatLabel class="w-full" variant="on">
      <Select id="betreuerSelect" v-model="personSelected" :disabled="!betreuerZuweisenSelected"
              :options="personen"
              filter fluid name="tutor" option-label="name" option-value="id"
              required/>
      <label for="betreuerSelect">Betreuer:in</label>
    </FloatLabel>
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
