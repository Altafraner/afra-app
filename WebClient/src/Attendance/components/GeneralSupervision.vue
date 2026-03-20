<script lang="ts" setup>
import {
    Accordion,
    AccordionContent,
    AccordionHeader,
    AccordionPanel,
    Button,
    InputGroup,
    InputGroupAddon,
    useDialog,
    useToast,
} from 'primevue';
import { computed, onUnmounted, shallowRef, watch } from 'vue';
import PersonSelector from '@/components/PersonSelector.vue';
import { useAttendance } from '../composables/attendanceHubClient';
import type {
    AttendanceSlot,
    AttendanceState,
    AttendanceStudentStatus,
} from '@/Attendance/models/attendance';
import MoveStudentForm from '@/Attendance/components/MoveStudentForm.vue';
import SelectStudentToMoveForm from '@/Attendance/components/SelectStudentToMoveForm.vue';
import Notes from '@/Attendance/components/Notes.vue';
import { useUser } from '@/stores/user';
import EnrollmentTable from '@/Attendance/components/EnrollmentTable.vue';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

const props = defineProps<{
    slot: AttendanceSlot;
}>();

const toast = useToast();
const dialog = useDialog();
const userStore = useUser();

const filterPerson = shallowRef();
const accordionValue = shallowRef(null);

const attendanceService = useAttendance('slot', props.slot.scope, props.slot.slotId, toast);
const attendance = attendanceService.slotAttendance;

onUnmounted(() => attendanceService.stop());

function updateAttendanceCallback(student: UserInfoMinimal, status: AttendanceState) {
    attendanceService.updateAttendance(student.id, status);
}

function updateStatusCallback(evt: Event, terminId: string, status: boolean) {
    evt.stopPropagation();
    attendanceService.updateStatus(terminId, status);
}

async function move(enrollment: AttendanceStudentStatus) {
    dialog.open(MoveStudentForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            student: enrollment.student,
            angebote: attendance,
            canMoveNow: attendanceService.canMoveNowNow(),
        },
        onClose: onCloseMove,
    });

    async function onCloseMove({ data }: { data?: { destination: string; all: boolean } }) {
        if (!data) return;
        if (data.all) {
            await attendanceService.moveStudent(enrollment.student.id, data.destination);
        } else {
            await attendanceService.moveStudentNow(enrollment.student.id, data.destination);
        }
    }
}

function initMoveHere(eventId: string) {
    dialog.open(SelectStudentToMoveForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            canMoveNow: attendanceService.canMoveNowNow(),
        },
        onClose: onCloseMove,
    });

    async function onCloseMove({ data }: { data?: { student: string; all: boolean } }) {
        if (!data) return;
        if (data.all) {
            await attendanceService.moveStudent(data.student, eventId);
        } else {
            await attendanceService.moveStudentNow(data.student, eventId);
        }
    }
}

function openNotes(data: AttendanceStudentStatus) {
    dialog.open(Notes, {
        props: {
            modal: true,
            header: 'Notizen',
        },
        data: {
            notes: computed(() => data.notes),
            myNote: computed(
                () => data.notes.find((n) => n.creator.id === userStore.user.id) ?? null,
            ),
            scope: props.slot.scope,
            slotId: props.slot.slotId,
            studentId: data.student.id,
        },
    });
}

const filterActive = computed(() => filterPerson.value != undefined);

const filteredAttendance = computed(() => {
    if (!filterPerson.value) return attendance.value;
    return attendance.value
        .map((a) => {
            const temp = Object.assign({}, a);
            temp.enrollments = a.enrollments?.filter(
                (e) => e.student.id === filterPerson.value,
            );
            return temp;
        })
        .filter((a) => a.enrollments?.length ?? 0 > 0);
});

watch(filteredAttendance, (newAttendance) => {
    if (newAttendance.length === 1) accordionValue.value = newAttendance[0].eventId;
});
</script>

<template>
    <InputGroup class="mb-6">
        <PersonSelector
            v-model="filterPerson"
            :filter="(s) => s.rolle === 'Mittelstufe'"
            hide-rolle
        >
            <template #label>Schüler:in suchen</template>
        </PersonSelector>
        <InputGroupAddon>
            <Button
                :disabled="filterPerson == undefined"
                aria-label="Filter entfernen"
                icon="pi pi-times"
                severity="secondary"
                variant="text"
                @click="filterPerson = undefined"
            />
        </InputGroupAddon>
    </InputGroup>
    <accordion v-model:value="accordionValue" lazy>
        <accordion-panel
            v-for="event of filteredAttendance"
            :key="event.eventId"
            :value="event.eventId"
        >
            <accordion-header>
                <div
                    class="flex justify-between w-full items-center"
                    style="margin-right: 1rem"
                >
                    <span class="flex-1"> {{ event.location }} - {{ event.name }} </span>
                    <span v-if="!filterActive" class="inline-flex gap-3 items-baseline">
                        {{ event.enrollments.length }} Schüler:innen
                        <Button
                            :label="event.status ? 'Fertig' : 'Ausstehend'"
                            :severity="
                                event.status
                                    ? event.enrollments.some((e) => e.status === 'Fehlend')
                                        ? 'warn'
                                        : 'success'
                                    : 'danger'
                            "
                            class="w-28"
                            size="small"
                            @click="
                                (evt) => updateStatusCallback(evt, event.eventId, !event.status)
                            "
                        />
                    </span>
                </div>
            </accordion-header>
            <accordion-content>
                <EnrollmentTable
                    :enableEdit="true"
                    :enableMove="attendanceService.capabilities.value.enableMove"
                    :enableNotes="attendanceService.capabilities.value.enableNotes"
                    :enrollments="event.enrollments"
                    :showAttendance="true"
                    @move="move"
                    @openNotes="openNotes"
                    @update="updateAttendanceCallback"
                >
                    <template #actions>
                        <Button
                            icon="pi pi-plus"
                            label="Schüler:in hinzufügen"
                            severity="secondary"
                            size="small"
                            @click="() => initMoveHere(event.eventId)"
                        />
                    </template>
                </EnrollmentTable>
            </accordion-content>
        </accordion-panel>
    </accordion>
</template>

<style scoped></style>
