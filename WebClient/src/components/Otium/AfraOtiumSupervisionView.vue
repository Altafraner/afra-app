<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/AfraOtiumEnrollmentTable.vue";
import {Accordion, AccordionPanel, AccordionHeader, AccordionContent, Button} from "primevue";

const props = defineProps({
  rooms: Array
})

const roomToggleDone = (evt, room) => {
  evt.stopPropagation();
  room.kontrolliert = !room.kontrolliert
}
</script>

<template>
  <accordion>
    <accordion-panel v-for="room of props.rooms" :key="room.id" :value="room.id">
      <accordion-header>
        <div class="flex justify-between width-fill align-center" style="margin-right: 1rem">
          <span>
            {{room.ort}} - {{room.bezeichnung}}
          </span>
          <Button :severity="room.kontrolliert ? 'success' : 'danger'" :label="room.kontrolliert ? 'Fertig' : 'Ausstehend'" size="small" class="confirm-button" @click="(evt) => roomToggleDone(evt, room)"/>
        </div>
      </accordion-header>
      <accordion-content>
        <p v-if="room.einschreibungen.length===0">
          Keine Einschreibungen
        </p>
        <afra-otium-enrollment-table v-else :enrollments="room.einschreibungen" may-edit-attendance />
      </accordion-content>
    </accordion-panel>
  </accordion>
</template>

<style scoped>
.confirm-button{
  width: 7rem;
}
</style>
