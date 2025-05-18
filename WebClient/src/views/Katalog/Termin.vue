<script setup>
import {Badge, Button, Column, DataTable, MeterGroup, Skeleton, Tag, useToast} from "primevue";
import {computed, ref} from "vue";
import AfraKategorieTag from "@/components/Otium/Shared/AfraKategorieTag.vue";
import {chooseColor, formatDate, formatTime, formatTutor} from "@/helpers/formatters.js";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";
import {useSettings} from "@/stores/useSettings.js";
import {useRouter} from "vue-router";
import NavBreadcrumb from "@/components/NavBreadcrumb.vue";

const settings = useSettings();
const user = useUser();
const toast = useToast();
const router = useRouter();
const props = defineProps({
  terminId: String,
})

const kategorien = ref(null)
const loading = ref(true)
const buttonLoading = ref(true)
const otium = ref(null)
const connection = ref(null)

const navItems = computed(() => {
  if (loading.value) return [];
  return [
    {
      label: "Katalog",
      route: {
        name: "Katalog"
      }
    },
    {
      label: formatDate(new Date(otium.value.datum)),
      route: {
        name: "Katalog-Datum",
        params: {
          datum: otium.value.datum.split('T')[0]
        }
      }
    },
    {
      label: otium.value.otium
    }
  ];
})

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
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Es ist ein Fehler beim Laden aufgetreten."
    })
    await router.push('/katalog')
    await user.update()
  }
}

async function unenroll() {
  buttonLoading.value = true
  try {
    otium.value = await connection.value.delete()
    buttonLoading.value = false
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Es ist ein Fehler beim Austragen aufgetreten."
    })
  }
}

async function enroll() {
  buttonLoading.value = true
  try {
    otium.value = await connection.value.put()
    buttonLoading.value = false
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Es ist ein Fehler beim Einschreiben aufgetreten."
    })
  }
}

async function loadKategorien() {
  await settings.updateKategorien()
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
    <NavBreadcrumb :items="navItems"/>
    <h1 class="mb-2">{{ otium.otium }}</h1>
    <span class="inline-flex gap-1 mb-4 text-sm">
      <Tag v-if="otium.istAbgesagt" severity="danger"
           icon="pi pi-exclamation-triangle">Abgesagt</Tag>
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

    <DataTable :value="otium.einschreibungen">
      <Column header="Start">
        <template #body="{data}">
          {{ data.interval.start }}
        </template>
      </Column>
      <Column header="Ende">
        <template #body="{data}">
          {{ data.interval.end }}
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
          <Button v-if="otium.istAbgesagt" severity="danger" disabled variant="text" size="small"
                  icon="pi pi-exclamation-triangle" label="Abgesagt"/>
          <template v-else-if="data.eingeschrieben">
            <Button v-if="data.kannBearbeiten" :disabled="buttonLoading" :loading="buttonLoading"
                    icon="pi pi-times"
                    label="Austragen" severity="danger" size="small"
                    variant="text" @click="() => unenroll()"/>
            <Button v-else v-tooltip.left="data.grund" disabled icon="pi pi-times"
                    label="Austragen" severity="danger" size="small" variant="text"/>
          </template>
          <template v-else>
            <Button v-if="data.kannBearbeiten" :disabled="buttonLoading" :loading="buttonLoading"
                    icon="pi pi-plus"
                    label="Einschreiben" size="small" variant="text"
                    @click="() => enroll()"/>
            <Button v-else v-tooltip.left="data.grund" :loading="buttonLoading" disabled
                    icon="pi pi-plus"
                    label="Einschreiben" size="small" variant="text"/>
          </template>
        </template>
      </Column>
    </DataTable>
    <h3>Beschreibung</h3>
    <p v-for="beschreibung in otium.beschreibung.split('\n').filter(desc => desc)"
       v-if="!props.minimal && otium.beschreibung">
      {{ beschreibung }}
    </p>
  </template>
  <div v-else>
    <h1>
      <Skeleton height="3rem" width="60%"/>
    </h1>
    <p>
      <Skeleton width="40%"/>
    </p>
    <h3 class="mt-[3rem]">
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

</style>
