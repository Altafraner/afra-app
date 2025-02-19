<script setup>
import {Card, DataTable, Column, Button, DatePicker} from "primevue";
import {ref, defineProps} from "vue";
import AfraOtiumAnwesenheit from "@/components/Otium/AfraOtiumAnwesenheit.vue";
import AfraOtiumEnrollmentTable from "@/components/Otium/AfraOtiumEnrollmentTable.vue";
import {formatDate, formatTutor} from "@/helpers/formatters.js";

const props = defineProps({
  otium: Object,
})

const otium = ref(props.otium)
const isEditing = ref(false)

const padString = (text) => String(text).padStart(2, '0')

</script>

<template>
  <Card>
    <template #title>
      {{ otium.bezeichnung }}
    </template>
    <template #subtitle>
      {{ formatDate(otium.datum) }}, {{ otium.start }} Uhr bis {{ otium.ende }} Uhr
    </template>
    <template #content v-if="!isEditing">
      <p>
        Tutor: {{ formatTutor(otium.tutor) }}
      </p>

      <strong class="text-xl font-bold">Einschreibungen</strong>
      <AfraOtiumEnrollmentTable :enrollments="otium.einschreibungen" show-attendance />
    </template>
  </Card>
</template>

<style scoped>

</style>
