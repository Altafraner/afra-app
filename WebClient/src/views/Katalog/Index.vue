<script setup>
import {ref, watch} from "vue";
import {Column, DataTable, Skeleton, useToast} from "primevue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import AfraKategorySelector from "@/components/Form/AfraKategorySelector.vue";
import AfraOtiumKatalogView from "@/components/Otium/Katalog/AfraOtiumKatalogView.vue";
import {mande} from "mande";
import {useUser} from "@/stores/useUser.js";
import {useRoute, useRouter} from "vue-router";
import {useSettings} from "@/stores/useSettings.js";

const props = defineProps({
  datum: {
    type: String,
    required: false,
    default: ""
  }
})
const router = useRouter();
const location = useRoute();
const toast = useToast();
const settings = useSettings();

const loading = ref(true)
const user = useUser();
const datesAvailable = ref([])
const dateDefault = ref(null)
const kategorieOptionsTree = ref()
const otia = ref([])
const date = ref(null);
const kategorie = ref(null);
const categoryChanged = () => {
}
const selectedOtia = ref(otia.value)

const linkGenerator = otium => `/termin/${otium.id}`

watch(kategorie, filterOtiaByKategorie)

function filterOtiaByKategorie() {
  if (kategorie.value == null || Object.keys(kategorie.value).length === 0) {
    selectedOtia.value = otia.value
    return
  }
  const kategorieId = Object.keys(kategorie.value)[0]
  selectedOtia.value = otia.value.filter(e => e.kategorien.includes(kategorieId))
}

async function startup() {
  loading.value = true
  const terminePromise = getTermine()
  const kategoriesPromise = getKategories()
  try {
    await terminePromise;
    if (props.datum && props.datum !== "") {
      const propDate = datesAvailable.value.find(e => e.datum === props.datum)
      if (propDate !== undefined) date.value = datesAvailable.value.find(e => e.datum === props.datum)
      else {
        date.value = dateDefault.value
        await router.replace('/katalog')
      }
    }
    await kategoriesPromise;
    await dateChanged()
  } catch (error) {
    console.error(error)
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
    })
    await user.update()
  }
  loading.value = false
}

async function getTermine() {
  loading.value = true
  await settings.updateSchuljahr();
  datesAvailable.value = settings.schuljahr
  dateDefault.value = settings.defaultDay
  date.value = settings.defaultDay
}

async function getAngebote() {
  const angeboteGetter = mande("/api/otium")
  otia.value = await angeboteGetter.get(`${date.value.datum}`);
  filterOtiaByKategorie()
}

async function getKategories() {
  const kategoriesGetter = mande("/api/otium/kategorie")
  kategorieOptionsTree.value = await kategoriesGetter.get();
}

async function dateChanged() {
  try {
    await getAngebote()
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Fehler",
      detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
    });
    if (location.fullPath !== "/katalog") {
      await router.push("/katalog")
      await dateChanged()
    }
  }
}

function selectToday() {
  date.value = dateDefault.value
  dateChanged()
}

watch([date], () => {
  if (!loading.value && date.value != null) router.push('/katalog/' + date.value.datum)
})

startup()

</script>

<template>
  <h1>Otia-Katalog</h1>

  <div class="flex gap-3 flex-col">
    <template v-if="!loading">
      <div class="flex gap-3">
        <AfraDateSelector v-model="date" :options="datesAvailable" @dateChanged="dateChanged"
                          @today="selectToday"/>
      </div>
      <AfraKategorySelector v-model="kategorie" :options="kategorieOptionsTree"
                            @change="categoryChanged"/>

      <AfraOtiumKatalogView :otia="selectedOtia" :link-generator="linkGenerator"/>
    </template>
    <div v-else class="flex gap-5 flex-col">
      <div class="flex gap-3 justify-between">
        <Skeleton width="65%" height="3rem"/>
        <Skeleton width="33%" height="3rem"/>
      </div>
      <Skeleton width="100%" height="3rem"/>
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
