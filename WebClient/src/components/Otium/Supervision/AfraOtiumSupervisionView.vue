<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
  Button,
  useToast
} from "primevue";
import {mande} from "mande";
import {ref} from "vue";
import {useAttendance} from "@/composables/attendanceHubClient.js";

const props = defineProps({
  rooms: Array
})

const inactive = ref(false);
const toast = useToast();

const roomToggleDone = (evt, room) => {
  evt.stopPropagation();
  room.kontrolliert = !room.kontrolliert
}

async function setup() {
  let currentBlock = null;
  try {
    currentBlock = await mande("/api/schuljahr/now").get();
    console.log(currentBlock)
  } catch (e) {
    if (e.response?.status === 404) {
      inactive.value = true;
      return;
    } else {
      console.error("Error fetching current block:", e);
      toast.add({
        severity: "error",
        summary: "Fehler",
        detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
      });
      return;
    }
  }
  inactive.value = false;
  const {attendance: localAttendance, update} = useAttendance('block', currentBlock.id);
  return {attendance: localAttendance, update};
}

const {attendance, update} = await setup();

function updateAttendanceCallback(student, status) {
  update(student.id, status);
}
</script>

<template>
  <accordion>
    <accordion-panel v-for="room of attendance" :key="room.terminId" :value="room.terminId">
      <accordion-header>
        <div class="flex justify-between w-full items-center" style="margin-right: 1rem">
          <span>
            {{ room.ort }} - {{ room.otium }}
          </span>
          <Button v-if="false" :severity="room.kontrolliert ? 'success' : 'danger'"
                  :label="room.kontrolliert ? 'Fertig' : 'Ausstehend'" size="small"
                  class="confirm-button" @click="(evt) => roomToggleDone(evt, room)"/>
        </div>
      </accordion-header>
      <accordion-content>
        <afra-otium-enrollment-table :enrollments="room.einschreibungen"
                                     :update-function="updateAttendanceCallback"
                                     may-edit-attendance/>
      </accordion-content>
    </accordion-panel>
  </accordion>
</template>

<style scoped>
.confirm-button {
  width: 7rem;
}
</style>
