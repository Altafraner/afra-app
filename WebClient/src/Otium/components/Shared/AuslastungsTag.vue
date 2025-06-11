<script setup>
import {chooseSeverity} from "@/helpers/formatters.js";
import {Tag} from "primevue";

const props = defineProps({
  istAbgesagt: Boolean,
  auslastung: Number
})
</script>

<template>
  <Tag class="w-full" v-if="istAbgesagt" severity="danger">Abgesagt</Tag>
  <Tag class="w-full" v-else-if="auslastung!=null" :severity="chooseSeverity(auslastung)">
    {{ auslastung }} %
  </Tag>
  <Tag class="w-full" v-else severity="success">&mdash;</Tag>
</template>

<style scoped>

</style>
