<script setup>
import {ref, watch} from "vue";
import {Select} from "primevue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import AfraOtiumKatalogView from "@/components/Otium/AfraOtiumKatalogView.vue";
import {kategorien, otiaDates, blockOptions as testBlockOptions, otia as testOtia} from "@/helpers/testdata.js";
import {useSettings} from "@/stores/useSettings.js";
import {formatTime} from "../../helpers/formatters.js";

const settings = useSettings();
const datesAvailable = ref(otiaDates)
const blockOptions = ref(settings.blocks)
const kategorieOptionsTree = ref(kategorien)
const otia = ref(testOtia)
const date = ref(datesAvailable.value[0]);
const kategorie = ref(null);
const block = ref(settings.blocks[0])
const dateChanged = () => console.info("Date Changed:", date.value)
const categoryChanged = () => console.info("Kategorie Changed:", kategorie.value)
const selectedOtia = ref(otia.value)

const linkGenerator = otium => `/katalog/${date.value.code}/${block.value.id}/${otium.id}`

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
      <Select v-model="block" :options="blockOptions">
        <template #option="{option}">
          {{formatTime(option.startTime)}} - {{formatTime(option.endTime)}}
        </template>
        <template #value="{value}">
          <div v-if="value!=null">{{formatTime(value.startTime)}} - {{formatTime(value.endTime)}}</div>
          <div v-else>Block</div>
        </template>
      </Select>
    </div>
    <AfraKategorySelector v-model="kategorie" :options="kategorieOptionsTree"
                          @change="categoryChanged"/>

    <AfraOtiumKatalogView :otia="selectedOtia" :link-generator="linkGenerator"/>
  </div>
</template>

<style scoped>


</style>
