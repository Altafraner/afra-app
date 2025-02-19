<script setup>
import {ref, watch} from "vue";
import {Select} from "primevue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import AfraOtiumKatalogView from "@/components/Otium/AfraOtiumKatalogView.vue";
import {kategorien} from "@/helpers/testdata.js";

const datesAvailable = ref([
  {
    label: "Montag, 10.02.2025 | H-Woche",
    code: "2025-02-10",
    disabled: false,
  },
  {
    label: "2025 KW 08 (H): Freitag",
    code: "2025-02-14",
    disabled: true,
  },
  {
    label: "2025 KW 09 (H): Freitag",
    code: "2025-02-19",
    disabled: false,
  }
])
const blockOptions = ref([
  {
    label: "13:30 - 14:45",
    block: 1
  },
  {
    label: "15:00 - 16:15",
    block: 2
  }
])
const kategorieOptionsTree = ref(kategorien)
const otia = ref([
  {
    bezeichnung: "Studienzeit Mathematik",
    beschreibung: "Hallo Welt",
    auslastung: 0.5,
    maxEinschreibungen: 10,
    einschreibungen: [5,4,6,3,7],
    tutor: {
      vorname: "Angela",
      nachname: "Merkel"
    },
    ort: "110",
    kategorien: ["0", "0-1"],
    id: "1"
  },
  {
    bezeichnung: "Schüler unterrichten Schüler",
    auslastung: 0.8,
    maxEinschreibungen: 5,
    einschreibungen: [5,4,6,3,7],
    tutor: {
      vorname: "Greta",
      nachname: "Thunberg"
    },
    ort: "109",
    kategorien: ["0", "0-2"],
    id: "2"
  },
  {
    bezeichnung: "Übungsraum Musik",
    auslastung: 0.5,
    maxEinschreibungen: 10,
    einschreibungen: [5,4,6,3,7],
    tutor: null,
    ort: '323',
    kategorien: ["4"],
    id: "3"
  },
  {
    bezeichnung: "Test",
    auslastung: 0.5,
    maxEinschreibungen: 0,
    einschreibungen: [5,4,6,3,7],
    tutor: {
      vorname: "Angela",
      nachname: "Merkel"
    },
    id: "4",
    kategorien: ["3"],
  },
  {
    bezeichnung: "Test",
    auslastung: 0.5,
    maxEinschreibungen: 0,
    einschreibungen: [5,4,6,3,7],
    tutor: {
      vorname: "Angela",
      nachname: "Merkel"
    },
    id: "5",
    kategorien: ["7"],
  }
])
const date = ref(datesAvailable.value[0]);
const kategorie = ref(null);
const block = ref(blockOptions.value[0].block)
const dateChanged = () => console.info("Date Changed:", date.value)
const categoryChanged = () => console.info("Kategorie Changed:", kategorie.value)
const selectedOtia = ref(otia.value)

function linkGenerator(otium){
  return `/katalog/${date.value.code}/${block.value}/${otium.id}`
}

watch(kategorie, () => {
  if (kategorie.value==null || Object.keys(kategorie.value).length===0){
    selectedOtia.value = otia.value
    return
  }
  const kategorieId = Object.keys(kategorie.value)[0]
  console.log(kategorieId)
  selectedOtia.value = otia.value.filter(e => e.kategorien.includes(kategorieId))
})
</script>

<template>
  <h1>Otia-Katalog</h1>

  <div class="flex gap-3 flex-col">
    <div class="flex gap-3">
      <AfraDateSelector v-model="date" :options="datesAvailable" @dateChanged="dateChanged"/>
      <Select v-model="block" :options="blockOptions" optionLabel="label" optionValue="block" />
    </div>
    <AfraKategorySelector v-model="kategorie" :options="kategorieOptionsTree"
                          @change="categoryChanged"/>

    <AfraOtiumKatalogView :otia="selectedOtia" :link-generator="linkGenerator"/>
  </div>
</template>

<style scoped>


</style>
