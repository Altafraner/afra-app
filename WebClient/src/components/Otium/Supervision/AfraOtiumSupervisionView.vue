<script setup>
import AfraOtiumEnrollmentTable from "@/components/Otium/Management/AfraOtiumEnrollmentTable.vue";
import {
  Accordion,
  AccordionContent,
  AccordionHeader,
  AccordionPanel,
  Button,
  useDialog,
  useToast
} from "primevue";
import {mande} from "mande";
import {computed, ref} from "vue";
import {useAttendance} from "@/composables/attendanceHubClient.js";
import MoveStudentForm from "@/components/Otium/Supervision/MoveStudentForm.vue";

const props = defineProps({
  rooms: Array
})

const inactive = ref(false);
const toast = useToast();
const dialog = useDialog();


async function setup() {
  let currentBlock = null;
  try {
    currentBlock = await mande("/api/schuljahr/now").get();
    inactive.value = false;
    return useAttendance('block', currentBlock.id, toast);
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
    return {
      attendance: []
    };
  }
}

const attendanceService = await setup();
const attendance = await attendanceService.attendance;

function updateAttendanceCallback(student, status) {
  attendanceService.updateAttendance(student.id, status);
}

function updateStatusCallback(evt, terminId, status) {
  evt.stopPropagation();
  attendanceService.updateStatus(terminId, status);
}

function initMove(student, terminId) {
  dialog.open(MoveStudentForm, {
    props: {
      header: "Schüler:in verschieben",
      modal: true,
      class: "sm:max-w-xl"
    },
    data: {
      student,
      angebote: computed(() => attendance.value.filter(termin => termin.terminId !== '00000000-0000-0000-0000-000000000000'))
    },
    onClose: move
  })

  function move({data}) {
    if (!data) return;
    if (data.all) {
      attendanceService.moveStudentNow(student.id, terminId, data.destination)
      return
    }
    attendanceService.moveStudent(student.id, data.destination)
  }
}
</script>

<template>
  <div v-if="inactive" class="flex justify-center">
    <span>Aktuell findet kein Otium statt. Der / die Otiumsbeauftragte(n) kann / können Anwesenheiten im Nachhinein in der Verwaltungsansicht des Termins ändern.</span>
  </div>
  <accordion v-else>
    <accordion-panel v-for="room of attendance" :key="room.terminId"
                     :value="room.terminId">
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
                                     may-edit-attendance show-transfer
                                     @initMove="(student) => initMove(student, room.terminId)"/>
      </accordion-content>
    </accordion-panel>
  </accordion>
</template>

<style scoped>
.confirm-button {
  width: 7rem;
}
</style>
