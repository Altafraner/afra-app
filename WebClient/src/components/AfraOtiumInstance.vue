<script setup>
import {Card, DataTable, Column, Button, DatePicker} from "primevue";
import {ref, defineProps} from "vue";
import AfraOtiumAnwesenheit from "@/components/AfraOtiumAnwesenheit.vue";
import AfraOtiumEnrollmentTable from "@/components/AfraOtiumEnrollmentTable.vue";

const props = defineProps({
  otium: Object,
})

const otium = ref(props.otium)
const isEditing = ref(false)

const formatDate = date => date.toLocaleDateString('de-DE', {
  weekday: "short",
  day: "2-digit",
  month: "2-digit",
});

const padString = (text) => String(text).padStart(2, '0')

const formatTime = date => padString(date.getHours()) + ":" + padString(date.getMinutes());
const formatTutor = tutor => tutor.last_name + ", " + tutor.given_name
const formatStudent = student => student.given_name + " " + student.last_name
</script>

<template>
  <Card>
    <template #title>
      {{ otium.title }}
    </template>
    <template #subtitle>
      {{ formatDate(otium.date) }}, {{ otium.start }} Uhr bis {{ otium.end }} Uhr
    </template>
    <template #content v-if="!isEditing">
      <p>
        Tutor: {{ formatTutor(otium.tutor) }}
      </p>

      <strong class="text-xl font-bold">Einschreibungen</strong>
      <AfraOtiumEnrollmentTable :enrollments="otium.enrollments" show-attendance />
    </template>
  </Card>
</template>

<style scoped>

</style>
