<script setup>
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
import {computed, onUnmounted, ref} from "vue";
import MoveStudentForm from "@/Otium/components/Supervision/MoveStudentForm.vue";
import AfraOtiumEnrollmentTable from "@/Otium/components/Management/AfraOtiumEnrollmentTable.vue";
import {useAttendance} from "@/Otium/composables/attendanceHubClient.js";
import {useRoute} from "vue-router";

const props = defineProps({
  rooms: Array,
  useQueryBlock: Boolean
})

const inactive = ref(false);
const toast = useToast();
const dialog = useDialog();
const route = props.useQueryBlock ? useRoute() : undefined;

const useDataFromQuery = computed(() => props.useQueryBlock && route.query.blockId !== undefined)


async function setup() {
  if (useDataFromQuery.value) {
    inactive.value = false;
    return useAttendance('block', route.query.blockId, toast);
  }

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

onUnmounted(() => attendanceService.stop())

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
      angebote: attendance.value,
      canMoveNow: !useDataFromQuery.value
    },
    onClose: move
  })

  function move({data}) {
    if (!data) return;
    if (data.all && data.destination === "00000000-0000-0000-0000-000000000000") {
      attendanceService.unenroll(student.id, terminId)
      return
    }
    if (data.all) {
      attendanceService.moveStudent(student.id, data.destination)
      return
    }
    attendanceService.moveStudentNow(student.id, terminId, data.destination)
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
