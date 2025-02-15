<script setup>
import AfraOtiumEnrollmentTable from "@/components/AfraOtiumEnrollmentTable.vue";
import {Accordion, AccordionPanel, AccordionHeader, AccordionContent} from "primevue";

const props = defineProps({
  rooms: Array
})
</script>

<template>
  <accordion>
    <accordion-panel v-for="room of props.rooms" :key="room.id" :value="room.id">
      <accordion-header>
        {{room.location}} - {{room.title}}
      </accordion-header>
      <accordion-content>
        <p v-if="room.enrollments.length===0">
          Keine Einschreibungen
        </p>
        <afra-otium-enrollment-table v-else :enrollments="room.enrollments" may-edit-attendance />
      </accordion-content>
    </accordion-panel>
  </accordion>
</template>

<style scoped>

</style>
