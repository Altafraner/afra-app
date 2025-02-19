<script setup>
import {ref, watch} from "vue";
import {Select} from "primevue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import AfraOtiumKatalogView from "@/components/Otium/AfraOtiumKatalogView.vue";
import {kategorien, otiaDates, blockOptions as testBlockOptions, otia as testOtia} from "@/helpers/testdata.js";

const datesAvailable = ref(otiaDates)
const blockOptions = ref(testBlockOptions)
const kategorieOptionsTree = ref(kategorien)
const otia = ref(testOtia)
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
