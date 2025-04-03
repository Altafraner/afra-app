<script setup>
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
  InputText,
  Textarea,
  useToast
} from "primevue";
import {useSettings} from "@/stores/useSettings.js";
import {useUser} from "@/stores/useUser.js";
import {mande} from "mande";
import {ref} from "vue";
import SimpleBreadcrumb from "@/components/SimpleBreadcrumb.vue";
import {findChildren, findPath} from "@/helpers/tree.js";
import AfraKategorieTag from "@/components/Otium/Shared/AfraKategorieTag.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import Grid from "@/components/Form/Grid/Grid.vue";
import GridEditRow from "@/components/Form/Grid/GridEditRow.vue";
import AfraOtiumRegTable from "@/components/Otium/Management/AfraOtiumRegTable.vue";
import AfraOtiumDateTable from "@/components/Otium/Management/AfraOtiumDateTable.vue";

const props = defineProps({
  otiumId: String
})

const user = useUser();
const settings = useSettings();
const toast = useToast();

const loading = ref(true);
const otium = ref({});

const bezeichnung = ref('')
const beschreibung = ref('')
const kategorie = ref(null)

async function getOtium(setInternal = true) {
  const getter = mande('/api/otium/management/otium/' + props.otiumId);
  otium.value = await getter.get();
  if (setInternal) {
    bezeichnung.value = otium.value.bezeichnung;
    const kategorieId = findChildren(settings.kategorien, otium.value.kategorie).id;
    kategorie.value = {[kategorieId]: true}
    beschreibung.value = otium.value.beschreibung.replaceAll('\n', '\n\n').trim()
  }
}

async function setup() {
  try {
    await settings.updateKategorien();
    await getOtium();
    loading.value = false;
  } catch {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
    });
  }
}

async function updateBezeichnung() {
  if (bezeichnung.value === otium.value.bezeichnung) return;
  try {
    await simpleUpdate('bezeichnung', bezeichnung.value)
  } finally {
    bezeichnung.value = otium.value.bezeichnung;
  }
}

async function updateKategorie() {
  let kategorieId
  try {
    kategorieId = Object.keys(kategorie.value)[0];
  } catch {
    return;
  }
  if (otium.value.kategorie === kategorieId) return;

  try {
    await simpleUpdate('kategorie', kategorieId)
  } finally {
    const kategorieId = findChildren(settings.kategorien, otium.value.kategorie).id;
    kategorie.value = {[kategorieId]: true}
  }
}

async function updateBeschreibung() {
  if (beschreibung.value.replaceAll('\n\n', '\n') === otium.value.bezeichnung) return;
  try {
    await simpleUpdate('beschreibung', beschreibung.value)
  } finally {
    beschreibung.value = otium.value.beschreibung.replaceAll('\n', '\n\n').trim()
  }
}

async function simpleUpdate(type, value) {
  const api = mande(`/api/otium/management/otium/${otium.value.id}/${type}`);
  try {
    await api.put({value: value})
    await getOtium(false)
  } catch {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Ein unerwarteter Fehler ist beim Speichern der Daten aufgetreten"
    })
  }
}

async function cancelTermin(id) {
  const api = mande(`/api/otium/management/termin/${id}/cancel`);
  try {
    await api.put()
    await getOtium(false)
  } catch {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Der Termin konnte nicht abgesagt werden."
    })
  }
}

async function deleteTermin(id) {
  const api = mande(`/api/otium/management/termin/${id}`);
  try {
    await api.delete()
    await getOtium(false)
  } catch (e) {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Der Termin konnte nicht gelöscht werden."
    })
  }
}

async function createTermin(data) {
  console.log(data)
  const api = mande(`/api/otium/management/termin`);
  try {
    await api.post({
      otiumId: otium.value.id,
      ort: data.ort,
      datum: data.date,
      block: data.block,
      tutor: data.person,
      maxEinschreibungen: data.maxEnrollments
    })
  } catch (e) {
    console.log(e.response)
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Der Termin konnte nicht erstellt werden. \n" + await e.body.split('(')[0]
    })
  } finally {
    await getOtium(false)
  }
}

async function createReg(data) {
  console.log(data)
  const api = mande(`/api/otium/management/wiederholung`);
  try {
    await api.post({
      otiumId: otium.value.id,
      wochentyp: data.wochentyp,
      wochentag: data.wochentag,
      startDate: data.von,
      endDate: data.bis,
      ort: data.ort,
      block: data.block,
      tutor: data.person,
      maxEinschreibungen: data.maxEnrollments
    })
  } catch (e) {
    console.log(e.response)
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Die Regelmäßigkeit konnte nicht erstellt werden. \n" + (e.body ? e.body.split('(')[0] : '')
    })
  } finally {
    await getOtium(false)
  }
}

setup();
</script>

<template>
  <template v-if="user.user.rolle !== 'Tutor'">
    <h1>Sie sind nicht Autorisiert, diese Seite zu nutzen.</h1>
  </template>
  <template v-else-if="!loading">
    <h1>{{ otium.bezeichnung }}</h1>
    <h2>Stammdaten</h2>
    <Grid>
      <GridEditRow header="Bezeichnung" @update="updateBezeichnung">
        <template #body>
          <span>{{ otium.bezeichnung }}</span>
        </template>
        <template #edit>
          <InputText v-model="bezeichnung" type="text"/>
        </template>
      </GridEditRow>
      <GridEditRow header="Kategorie" @update="updateKategorie">
        <template #body>
          <SimpleBreadcrumb :model="findPath(settings.kategorien, otium.kategorie)">
            <template #item="{item}">
              <AfraKategorieTag :value="item" minimal/>
            </template>
          </SimpleBreadcrumb>
        </template>
        <template #edit>
          <AfraKategorySelector v-model="kategorie" :options="settings.kategorien" fluid
                                hide-clear/>
        </template>
      </GridEditRow>
      <GridEditRow header="Beschreibung" header-class="self-start" @update="updateBeschreibung">
        <template #body>
          <p v-for="line in otium.beschreibung.split('\n')" :key="line" class="first:mt-0">
            {{ line }}
          </p>
        </template>
        <template #edit>
          <Textarea v-model="beschreibung" auto-resize fluid rows="2"/>
        </template>
      </GridEditRow>
    </Grid>
    <Accordion multiple value="">
      <AccordionPanel value="0">
        <AccordionHeader>Termine</AccordionHeader>
        <AccordionContent>
          <afra-otium-date-table :dates="otium.termine" allowEdit @cancel="cancelTermin"
                                 @create="createTermin" @delete="deleteTermin"/>
        </AccordionContent>
      </AccordionPanel>
      <AccordionPanel value="1">
        <AccordionHeader>Regelmäßigkeiten</AccordionHeader>
        <AccordionContent>
          <afra-otium-reg-table :regs="otium.wiederholungen"
                                allowEdit @create="createReg"/>
        </AccordionContent>
      </AccordionPanel>
      <!--AccordionPanel value="2">
        <AccordionHeader>Verwaltende</AccordionHeader>
        <AccordionContent>
          <afra-otium-manager-table v-if="!props.hideRegularities" :managers="otium.verwaltende"/>
        </AccordionContent>
      </AccordionPanel-->
    </Accordion>
  </template>
  <template v-else>Lade...</template>
</template>

<style scoped>

</style>
