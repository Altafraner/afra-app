<script setup>
import {ref} from 'vue'
import {OtiumsKategorie} from "@/constants/OtiumsKategorie.js";
import {InputGroup, Button, Select} from "primevue";
import InputGroupAddon from "primevue/inputgroupaddon";

const props = defineProps({
  kategorie: String,
})
const kategorie = ref(OtiumsKategorie[props.kategorie])
const datesAvailable = ref([
  {
    label: "Montag, 10.02.2025 | H-Woche",
    code: "17.02.2025",
    disabled: false,
  },
  {
    label: "2025 KW 08 (H): Freitag",
    code: "14.02.2025",
    disabled: true,
  },
  {
    label: "2025 KW 09 (H): Freitag",
    code: "19.02.2025",
    disabled: false,
  }
])
const date = ref(datesAvailable.value[0])

function change_date(next){
  let n = datesAvailable.value.findIndex((element) => element.label === date.value.label)
  n = next(n)
  while (n < datesAvailable.value.length && n>=0){
    if (datesAvailable.value[n].disabled){
      n = next(n)
      continue
    }
    date.value=datesAvailable.value[n]
    return
  }
}

const increment_date = () => change_date((n) => n + 1);
const decrement_date = () => change_date((n) => n - 1);


</script>

<template>
  <h1>Katalog</h1>
  <h2>Kategorie: {{kategorie.label}}</h2>

  <InputGroup>
    <input-group-addon>
      <Button severity="secondary" rounded icon="pi pi-chevron-left" variant="text" @click="decrement_date"/>
    </input-group-addon>
    <Select fluid filter v-model="date" option-label="label" option-disabled="disabled" :options="datesAvailable"/>
    <input-group-addon>
      <Button severity="secondary" rounded icon="pi pi-chevron-right" variant="text" @click="increment_date"/>
    </input-group-addon>
  </InputGroup>

  {{date}}

</template>

<style scoped>

</style>
