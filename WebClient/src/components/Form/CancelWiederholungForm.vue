<script setup>
import {useSettings} from "@/stores/useSettings.js";
import {computed, ref} from "vue";
import AfraDateSelector from "@/components/Form/AfraDateSelector.vue";
import {Form} from "@primevue/forms";
import {Button} from "primevue";

const props = defineProps({
  wiederholung: {
    type: Object,
    required: true
  },
})
const emit = defineEmits(['submit']);

const settings = useSettings()
const end = ref(null)
const loading = ref(true)

const datesAvailable = computed(() => {
  const today = new Date()
  const startDate = new Date(props.wiederholung.startDate);
  const endDate = new Date(props.wiederholung.endDate);
  return settings.schuljahr.filter((day) => {
    const datum = new Date(day.datum)
    return datum.getDay() === props.wiederholung.wochentag &&
        day.wochentyp === props.wiederholung.wochentyp &&
        datum >= startDate &&
        datum >= today &&
        datum < endDate
  })
})


const canBeShortened = computed(() => datesAvailable.value.length > 0)

function onSubmit() {
  console.log("Submitting", end.value)
  emit('submit', end.value)
}

async function setup() {
  await settings.updateSchuljahr()
  if (canBeShortened.value) end.value = datesAvailable.value[datesAvailable.value.length - 1]
  loading.value = false
}

setup()
</script>

<template>
  <template v-if="!loading">
    <Form v-if="canBeShortened" class="flex flex-col gap-3" @submit="onSubmit">
      <AfraDateSelector v-model="end" :label="'Ende der Wiederholung'" :options="datesAvailable"
                        hide-today name="date"/>
      <p>
        Durch das Einkürzen der Wiederholung werden alle Termine nach dem neuen Enddatum abgesagt und gelöscht.
      </p>
      <Button label="Wiederholung einkürzen" severity="danger" type="submit"/>
    </Form>
    <p v-else>
      Die Wiederholung kann nicht weiter gekürzt werden. Sie können ggf. den ausstehenden Termin Absagen oder falls es
      keine Termine mit Einschreibungen (mehr) gibt, die Wiederholung löschen.
    </p>
  </template>
</template>

<style scoped>

</style>
