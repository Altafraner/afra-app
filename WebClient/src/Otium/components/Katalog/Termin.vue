<script setup>
import {Button, Tag, useDialog, useToast} from "primevue";
import {ref} from "vue";
import {formatDate, formatTime, formatTutor} from "@/helpers/formatters.js";
import {mande} from "mande";
import {useUser} from "@/stores/user.js";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import {useRouter} from "vue-router";
import AfraKategorieTag from "@/Otium/components/Shared/AfraKategorieTag.vue";
import {findPath} from "@/helpers/tree.js";
import SimpleBreadcrumb from "@/components/SimpleBreadcrumb.vue";
import MultipleEnrollmentForm from "@/Otium/components/Katalog/Forms/MultipleEnrollmentForm.vue";

const settings = useOtiumStore();
const user = useUser();
const toast = useToast();
const router = useRouter();
const dialog = useDialog();
const props = defineProps({
  terminId: String,
})
const emit = defineEmits(['update']);

const kategorien = ref(null)
const buttonLoading = ref(true)
const otium = ref(null)
const connection = ref(null)

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
  } finally {
    emit('update');
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
  } finally {
    emit('update');
  }
}

function multiEnroll() {
  buttonLoading.value = true;
  dialog.open(MultipleEnrollmentForm, {
    props: {
      header: "Mehrfach einschreiben",
      modal: true,
      class: "sm:max-w-xl"
    },
    data: {
      options: otium.value.wiederholungen
    },
    onClose: multiEnrollCallback
  })

  async function multiEnrollCallback(options) {
    try {
      if (options.data === undefined || options.data === null) return;
      if (options.data.selected.length === 0) return enroll();
      const response = await mande('/api/otium/' + props.terminId + '/multi-enroll').put(options.data.selected)
      if (response.denied.length > 0) {
        toast.add({
          severity: "warn",
          summary: "Einschreibung teilweise fehlgeschlagen",
          detail: `Die Einschreibung in die folgenden Termine ist fehlgeschlagen: ${response.denied.map(d => formatDate(new Date(d))).join(', ')}`
        });
      }
    } catch (err) {
      if (err.response)
        toast.add({
          severity: "error",
          summary: "Fehler",
          detail: `Es ist ein Fehler beim Einschreiben aufgetreten. Code: ${err.response.status} (${err.response.statusText})`
        })
      else {
        toast.add({
          severity: "error",
          summary: "Fehler",
          detail: "Es ist ein Fehler beim Einschreiben aufgetreten."
        })
        console.error(err);
      }
    } finally {
      await loadTermin()
      buttonLoading.value = false;
    }
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
      <div v-if="otium.einschreibung.kannBearbeiten" class="flex flex-col gap-3 items-end">
        <Button :disabled="buttonLoading"
                :loading="buttonLoading"
                class="justify-end"
                fluid
                icon="pi pi-plus"
                label="Einschreiben"
                variant="text"
                @click="() => enroll()"/>
        <Button v-if="otium.wiederholungen.length > 0"
                :disabled="buttonLoading"
                :loading="buttonLoading"
                icon="pi pi-refresh"
                label="Mehrmals Einschreiben"
                severity="secondary"
                variant="text"
                @click="() => multiEnroll()"/>
      </div>
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
