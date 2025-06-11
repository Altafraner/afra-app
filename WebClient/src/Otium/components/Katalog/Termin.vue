<script setup>
import {Button, Tag, useToast} from "primevue";
import {ref} from "vue";
import {formatDate, formatTime, formatTutor} from "@/helpers/formatters.js";
import {mande} from "mande";
import {useUser} from "@/stores/user.js";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import {useRouter} from "vue-router";
import AfraKategorieTag from "@/Otium/components/Shared/AfraKategorieTag.vue";
import {findPath} from "@/helpers/tree.js";
import SimpleBreadcrumb from "@/components/SimpleBreadcrumb.vue";

const settings = useOtiumStore();
const user = useUser();
const toast = useToast();
const router = useRouter();
const props = defineProps({
  terminId: String,
})

const kategorien = ref(null)
const buttonLoading = ref(true)
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

async function setup() {
  connection.value = mande("/api/otium/" + props.terminId)
  const terminPromise = loadTermin();
  const kategorienPromise = loadKategorien();

  await Promise.all([terminPromise, kategorienPromise])
}

await setup();

</script>

<template>
  <div class="flex justify-between flex-wrap">
    <p class="flex flex-row gap-4 ml-2">
      <Tag v-if="otium.istAbgesagt" icon="pi pi-exclamation-triangle"
           severity="danger">Abgesagt
      </Tag>
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
    <Button v-if="otium.istAbgesagt" disabled icon="pi pi-exclamation-triangle" label="Abgesagt"
            severity="danger" variant="text"/>
    <template v-else-if="otium.einschreibung.eingeschrieben">
      <Button v-if="otium.einschreibung.kannBearbeiten" :disabled="buttonLoading"
              :loading="buttonLoading"
              icon="pi pi-times"
              label="Austragen" severity="danger"
              variant="text" @click="() => unenroll()"/>
      <Button v-else v-tooltip.left="otium.einschreibung.grund" disabled icon="pi pi-times"
              label="Austragen" severity="danger" variant="text"/>
    </template>
    <template v-else>
      <Button v-if="otium.einschreibung.kannBearbeiten" :disabled="buttonLoading"
              :loading="buttonLoading"
              icon="pi pi-plus"
              label="Einschreiben" variant="text"
              @click="() => enroll()"/>
      <Button v-else v-tooltip.left="otium.einschreibung.grund" :loading="buttonLoading" disabled
              icon="pi pi-plus"
              label="Einschreiben" variant="text"/>
    </template>
  </div>

  <h3 class="font-bold mt-4 text-lg">Beschreibung</h3>
  <SimpleBreadcrumb :model="findPath(settings.kategorien, otium.kategorie)" wrap>
    <template #item="{item}">
      <AfraKategorieTag :value="item" minimal/>
    </template>
  </SimpleBreadcrumb>
  <p v-for="beschreibung in otium.beschreibung.split('\n').filter(desc => desc)"
     v-if="!props.minimal && otium.beschreibung">
    {{ beschreibung }}
  </p>
</template>

<style scoped>

</style>
