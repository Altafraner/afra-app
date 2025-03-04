<script setup>
import {ref, watch} from "vue";
import {Select, Skeleton, DataTable, Column} from "primevue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import AfraOtiumKatalogView from "@/components/Otium/AfraOtiumKatalogView.vue";
import {kategorien} from "@/helpers/testdata.js";
import {useSettings} from "@/stores/useSettings.js";
import {formatTime} from "@/helpers/formatters.js";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";

const loading = ref(true)
const user = useUser();
const settings = useSettings();
const datesAvailable = ref([])
const blockOptions = ref(settings.blocks)
const kategorieOptionsTree = ref(kategorien)
const otia = ref([])
const date = ref(null);
const kategorie = ref(null);
const block = ref(settings.blocks[0])
const categoryChanged = () => console.info("Kategorie Changed:", kategorie.value)
const selectedOtia = ref(otia.value)

const linkGenerator = otium => `/katalog/${otium.id}`

watch(kategorie, filterOtiaByKategorie)

function filterOtiaByKategorie(){
  if (kategorie.value==null || Object.keys(kategorie.value).length===0){
    selectedOtia.value = otia.value
    return
  }
  const kategorieId = Object.keys(kategorie.value)[0]
  console.log(kategorieId)
  selectedOtia.value = otia.value.filter(e => e.kategorien.includes(kategorieId))
}

function filterBlockOptions(){
  const filtered = [];
  for(let k in date.value.otiumsBlock){
    if(date.value.otiumsBlock[k]){
      filtered.push(settings.blocks[k]);
    }
  }
  if (!filtered.includes(block.value)){
    block.value = filtered[0]
  }
  blockOptions.value = filtered;
}

async function startup(){
  loading.value = true
  const terminePromise = getTermine()
  const kategoriesPromise = getKategories()
  try {
    await terminePromise;
    await kategoriesPromise;
    await dateChanged()
  } catch (error) {
    console.log(error)
    await user.update()
  }
  loading.value = false
}

async function getTermine(){
  loading.value = true
  const termineGetter = mande("/api/schuljahr")
  datesAvailable.value = await termineGetter.get();
  date.value = datesAvailable.value[0]
}

async function getAngebote(){
  const angeboteGetter = mande("/api/otium")
  otia.value = await angeboteGetter.get(`${date.value.datum}/${block.value.id}`);
  filterOtiaByKategorie()
}

async function getKategories() {
  const kategoriesGetter = mande("/api/otium/kategorie")
  kategorieOptionsTree.value = await kategoriesGetter.get();
}

async function dateChanged(){
  filterBlockOptions()
  await getAngebote()
}

startup()

</script>

<template>
  <h1>Otia-Katalog</h1>

  <div class="flex gap-3 flex-col">
    <template v-if="!loading">
      <div class="flex gap-3">
        <AfraDateSelector v-model="date" :options="datesAvailable" @dateChanged="dateChanged"/>
        <Select v-model="block" :options="blockOptions" @change="dateChanged">
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
    </template>
    <div v-else class="flex gap-5 flex-col">
      <div class="flex gap-3 justify-between">
        <Skeleton width="65%" height="3rem" />
        <Skeleton width="33%" height="3rem" />
      </div>
      <Skeleton width="100%" height="3rem" />
      <DataTable :value="new Array(4)">
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
  </div>
</template>

<style scoped>


</style>
