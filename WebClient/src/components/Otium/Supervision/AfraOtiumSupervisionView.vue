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
    inactive.value = false;
    const {
      attendance: localAttendance,
      updateAttendance,
      updateStatus
    } = useAttendance('block', currentBlock.id);
    return {attendance: localAttendance, updateAttendance, updateStatus};
  } catch (e) {
    if (e.response?.status === 404) {
      inactive.value = true;
    } else {
      console.error("Error fetching current block:", e);
      toast.add({
        severity: "error",
        summary: "Fehler",
        detail: "Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten"
      });
    }
  }
  return {
    attendance: [], updateAttendance: () => {
    }, updateStatus: () => {
    }
  };
}

const {attendance, updateAttendance, updateStatus} = await setup();

function updateAttendanceCallback(student, status) {
  updateAttendance(student.id, status);
}

function updateStatusCallback(evt, terminId, status) {
  evt.stopPropagation();
  updateStatus(terminId, status);
}
</script>

<template>
  <div v-if="inactive" class="flex justify-center">
    <span>Aktuell findet kein Otium statt. Der / die Otiumsbeauftragte(n) kann / können Anwesenheiten im Nachhinein in der Verwaltungsansicht des Termins ändern.</span>
  </div>
  <accordion v-else>
    <accordion-panel v-for="room of attendance" :key="room.terminId" :value="room.terminId">
      <accordion-header>
        <div class="flex justify-between w-full items-center" style="margin-right: 1rem">
          <span>
            {{ room.ort }} - {{ room.otium }}
          </span>
          <Button :label="room.sindAnwesenheitenErfasst ? 'Fertig' : 'Ausstehend'"
                  :severity="room.sindAnwesenheitenErfasst ? 'success' : 'danger'"
                  class="confirm-button"
                  size="small"
                  @click="(evt) => updateStatusCallback(evt, room.terminId, !room.sindAnwesenheitenErfasst)"/>
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
