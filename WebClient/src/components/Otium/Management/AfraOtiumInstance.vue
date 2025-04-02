<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {formatDate} from "@/helpers/formatters.js";

const props = defineProps({
  otium: Object,
})

</script>

<template>
  <h1>
    {{ otium.otium }}
  </h1>
  <p>
    {{ formatDate(new Date(otium.datum)) }}, {{ otium.block }}. Block
  </p>
  <h2>Einschreibungen</h2>
  <AfraOtiumEnrollmentTable :enrollments="otium.einschreibungen" show-attendance/>
</template>

<style scoped>

</style>
