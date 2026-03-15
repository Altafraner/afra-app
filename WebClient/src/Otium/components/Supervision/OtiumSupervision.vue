<script setup>
import {
    Accordion,
    AccordionContent,
    AccordionHeader,
    AccordionPanel,
    Button,
    useDialog,
    useToast,
} from 'primevue';
import { computed, onUnmounted } from 'vue';
import MoveStudentForm from '@/Otium/components/Supervision/MoveStudentForm.vue';
import OtiumEnrollmentTable from '@/Otium/components/Management/OtiumEnrollmentTable.vue';
import { useAttendance } from '@/Otium/composables/attendanceHubClient.js';
import { useRoute } from 'vue-router';
import { isNowInInterval } from '@/helpers/time.js';
import SelectStudentToMoveForm from '@/Otium/components/Supervision/SelectStudentToMoveForm.vue';

const props = defineProps({
    rooms: Array,
    useQueryBlock: Boolean,
    block: Object,
});

const toast = useToast();
const dialog = useDialog();
const route = props.useQueryBlock ? useRoute() : undefined;

const useDataFromQuery = computed(
    () => props.useQueryBlock && route.query.blockId !== undefined,
);

const blockId = computed(() => (useDataFromQuery.value ? route.query.blockId : props.block.id));

async function setup() {
    return useAttendance('block', blockId.value, toast);
}

const attendanceService = await setup();
const attendance = await attendanceService.attendance;

onUnmounted(() => attendanceService.stop());

function updateAttendanceCallback(student, status) {
    attendanceService.updateAttendance(student.id, status);
}

function updateStatusCallback(evt, terminId, status) {
    evt.stopPropagation();
    attendanceService.updateStatus(terminId, status);
}

function initMove(student) {
    dialog.open(MoveStudentForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            student,
            angebote: attendance.value,
            canMoveNow:
                !useDataFromQuery.value &&
                isNowInInterval(props.block.datum, props.block.uhrzeit),
        },
        onClose: move,
    });

    function move({ data }) {
        if (!data) return;
        if (data.all && data.destination === '00000000-0000-0000-0000-000000000000') {
            attendanceService.unenroll(student.id, blockId.value);
            return;
        }
        if (data.all) {
            attendanceService.moveStudent(student.id, data.destination);
            return;
        }
        attendanceService.moveStudentNow(student.id, blockId.value, data.destination);
    }
}

function initMoveHere(terminId) {
    dialog.open(SelectStudentToMoveForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            canMoveNow:
                !useDataFromQuery.value &&
                isNowInInterval(props.block.datum, props.block.uhrzeit),
        },
        onClose: move,
    });

    function move({ data }) {
        if (!data) return;
        if (data.all && terminId === '00000000-0000-0000-0000-000000000000') {
            attendanceService.unenroll(data.student, blockId.value);
            return;
        }
        if (data.all) {
            attendanceService.moveStudent(data.student, terminId);
            return;
        }
        attendanceService.moveStudentNow(data.student, blockId.value, terminId);
    }
}
</script>

<template>
    <accordion lazy>
        <accordion-panel v-for="room of attendance" :key="room.terminId" :value="room.terminId">
            <accordion-header>
                <div
                    class="flex justify-between w-full items-center"
                    style="margin-right: 1rem"
                >
                    <span class="flex-1"> {{ room.ort }} - {{ room.otium }} </span>
                    <span class="inline-flex gap-3 items-baseline">
                        {{ room.einschreibungen.length }} Schüler:innen
                        <Button
                            :label="room.sindAnwesenheitenErfasst ? 'Fertig' : 'Ausstehend'"
                            :severity="
                                room.sindAnwesenheitenErfasst
                                    ? room.einschreibungen.some(
                                          (e) => e.anwesenheit === 'Fehlend',
                                      )
                                        ? 'warn'
                                        : 'success'
                                    : 'danger'
                            "
                            class="w-28"
                            size="small"
                            @click="
                                (evt) =>
                                    updateStatusCallback(
                                        evt,
                                        room.terminId,
                                        !room.sindAnwesenheitenErfasst,
                                    )
                            "
                        />
                    </span>
                </div>
            </accordion-header>
            <accordion-content>
                <OtiumEnrollmentTable
                    :enrollments="room.einschreibungen"
                    :update-function="updateAttendanceCallback"
                    :block-id="blockId"
                    may-edit-attendance
                    show-transfer
                    @initMove="(student) => initMove(student)"
                    @init-move-here="() => initMoveHere(room.terminId)"
                />
            </accordion-content>
        </accordion-panel>
    </accordion>
</template>

<style scoped></style>
