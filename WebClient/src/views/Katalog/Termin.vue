<script setup>
import {DataTable, Column, Badge, MeterGroup, Button, Skeleton} from "primevue";
import {ref} from "vue";
import AfraKategorieTag from "@/components/Otium/AfraKategorieTag.vue";
import {chooseColor, formatDate, formatTime, formatTutor} from "@/helpers/formatters.js";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";
import {useSettings} from "@/stores/useSettings.js";

const settings = useSettings();
const user = useUser();
const props = defineProps({
  terminId: String,
})

const kategorien = ref(null)
const loading = ref(true)
const buttonLoading = ref(true)
const times = ref(null)
const otium = ref(null)
const connection = ref(null)

function findKategorie(id, kategorien) {
  const index = kategorien.findIndex((e) => e.id === id);
  if (index !== -1) {
    return kategorien[index]
  }

  for (const kategorie of kategorien ?? []) {
    const childResult = findKategorie(id, kategorie.children)
    if (childResult != null) return childResult
  }

  return null
}

async function loadTermin() {
  buttonLoading.value = true
  try {
    otium.value = await connection.value.get();
    buttonLoading.value = false
  } catch (error) {
    await useUser().update()
  }
}

async function unenroll(start, evt) {
  buttonLoading.value = true
  try {
    otium.value = await connection.value.delete(start.toString())
    buttonLoading.value = false
  } catch (error) {
    // TODO Notify user
    console.error(error)
  }
}

async function enroll(start, evt) {
  buttonLoading.value = true
  try {
    otium.value = await connection.value.put(start.toString())
    buttonLoading.value = false
  } catch (error) {
    // TODO Notify user
    console.error(error)
  }
}

async function loadKategorien() {
  await settings.updateKategorien()
  console.log(settings.kategorien)
  kategorien.value = settings.kategorien;
}

function setup() {
  connection.value = mande("/api/otium/" + props.terminId)
  loading.value = true
  const terminPromise = loadTermin();
  const kategorienPromise = loadKategorien();

  Promise.all([terminPromise, kategorienPromise]).then(() => {
    loading.value = false
  })
}

setup();

</script>

<template>
  <template v-if="!loading">
    <h1 class="mb-2">{{ otium.otium }}</h1>
    <span class="inline-flex gap-1 mb-4 text-sm">
      <AfraKategorieTag v-for="tag in otium.kategorien" :value="findKategorie(tag, kategorien)"/>
    </span>
    <p class="flex flex-row gap-4 ml-2">
      <span v-if="otium.tutor">
        <i class="pi pi-user"/>
        {{ formatTutor(otium.tutor) }}
      </span>
      <span v-if="otium.ort">
        <i class="pi pi-map-marker"/> {{ otium.ort }}
      </span>
      <span v-if="otium.datum">
        <i class="pi pi-clock"/>
        {{ formatDate(new Date(otium.datum)) }}, {{ formatTime(new Date(otium.datum)) }} Uhr
      </span>
    </p>
    <!--p v-if="!props.minimal" v-for="beschreibung in otium.beschreibung.split('\n').filter(desc => desc)">
      {{ beschreibung }}</p-->
    <DataTable :value="otium.einschreibungen">
      <Column header="Start">
        <template #body="{data}">
          {{ data.interval.start }}
        </template>
      </Column>
      <Column header="Einschreibungen">
        <template #body="{data}">
          <div class="inline-grid grid-cols-[auto_7rem] justify-center items-center gap-3">
            <Badge severity="secondary" :value="data.anzahl"/>
            <MeterGroup v-if="otium.maxEinschreibungen !== null && otium.maxEinschreibungen!==0"
                        :value="[{value: data.anzahl, color: chooseColor(data.anzahl / otium.maxEinschreibungen, otium.maxEinschreibungen), label: null}]"
                        :max="otium.maxEinschreibungen" label-position="none"/>
          </div>
        </template>
      </Column>
      <Column class="text-right afra-col-action">
        <template #body="{data}">
          <Button v-if="data.eingeschrieben" icon="pi pi-times" severity="danger" size="small"
                  variant="text" label="Austragen" :disabled="!data.kannBearbeiten || buttonLoading"
                  @click="(evt) => unenroll(data.interval.start, evt)" :loading="buttonLoading"/>
          <Button v-else icon="pi pi-plus" size="small" variant="text" label="Einschreiben"
                  :disabled="!data.kannBearbeiten || buttonLoading"
                  @click="(evt) => enroll(data.interval.start, evt)" :loading="buttonLoading"/>
        </template>
      </Column>
    </DataTable>
  </template>
  <div v-else>
    <h1>
      <Skeleton height="3rem" width="60%"/>
    </h1>
    <p>
      <Skeleton width="40%"/>
    </p>
    <h3 style="margin-top: 3rem">
      <Skeleton height="2rem" width="55%"/>
    </h3>
    <DataTable :value="new Array(3)">
      <Column>
        <template #header>
          <Skeleton/>
        </template>
        <template #body>
          <Skeleton/>
        </template>
      </Column>
      <Column>
        <template #header>
          <Skeleton/>
        </template>
        <template #body>
          <Skeleton/>
        </template>
      </Column>
      <Column>
        <template #header>
          <Skeleton/>
        </template>
        <template #body>
          <Skeleton/>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<style scoped>

.enrollmentGrid {
  display: inline-grid;
  grid-template-columns: auto 7rem;
  justify-content: center;
}
</style>
