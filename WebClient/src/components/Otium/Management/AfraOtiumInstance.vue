<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {formatDate} from "@/helpers/formatters.js";
import Grid from "@/components/Form/Grid/Grid.vue";
import GridEditRow from "@/components/Form/Grid/GridEditRow.vue";
import {FloatLabel, InputNumber, ToggleSwitch} from "primevue";
import {ref} from "vue";

const props = defineProps({
  otium: Object,
})

const emit = defineEmits(['update-max-enrollments'])
const maxEnrollmentsSetzenSelected = ref(false)
const maxEnrollmentsSelected = ref(props.otium.maxEinschreibungen)

const startEditMaxEnrollments = () => {
  maxEnrollmentsSetzenSelected.value = props.otium.maxEinschreibungen !== null
  maxEnrollmentsSelected.value = props.otium.maxEinschreibungen
}

const updateMaxEnrollments = () => {
  emit('update-max-enrollments', maxEnrollmentsSetzenSelected.value ? maxEnrollmentsSelected.value : null)
}

</script>

<template>
  <h1>
    {{ otium.otium }}
  </h1>
  <grid>
    <GridEditRow header="Datum" hide-edit>
      <template #body>
        {{ formatDate(new Date(otium.datum)) }}
      </template>
    </GridEditRow>
    <GridEditRow header="Block" hide-edit>
      <template #body>
        {{ otium.block + 1 }}. Block
      </template>
    </GridEditRow>
    <GridEditRow header="max. Teilnehner:innen" @edit="startEditMaxEnrollments"
                 @update="updateMaxEnrollments">
      <template #body>
        {{ otium.maxEinschreibungen ? otium.maxEinschreibungen : "Unbegrenzt" }}
      </template>
      <template #edit>
        <div class="w-full flex flex-col gap-3">
          <div class="flex justify-between mt-4">
            <label for="maxEnrollmentSwitch">Teilnehmer:innen-Zahl beschränken</label>
            <ToggleSwitch v-model="maxEnrollmentsSetzenSelected"
                          if="maxEnrollmentSwitch"/>
          </div>
          <FloatLabel class="w-full" variant="on">
            <InputNumber id="maxEnrollmentInput" v-model="maxEnrollmentsSelected"
                         :disabled="!maxEnrollmentsSetzenSelected" fluid name="maxEnrollments"/>
            <label for="maxEnrollmentInput">max. Teilnehmer:innen</label>
          </FloatLabel>
        </div>
      </template>
    </GridEditRow>
  </grid>
  <h2>Einschreibungen</h2>
  <AfraOtiumEnrollmentTable :enrollments="otium.einschreibungen" show-attendance/>
</template>

<style scoped>

</style>
