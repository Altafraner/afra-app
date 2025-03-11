<script setup>
import {Card} from "primevue";
import {ref} from "vue";
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {formatDate, formatTutor} from "@/helpers/formatters.js";

const props = defineProps({
  otium: Object,
})

const otium = ref(props.otium)
const isEditing = ref(false)

</script>

<template>
  <Card>
    <template #title>
      {{ otium.bezeichnung }}
    </template>
    <template #subtitle>
      {{ formatDate(otium.datum) }}, {{ otium.block }}. Block
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
