<script lang="ts" setup>
import { computed, onUnmounted, shallowRef, toValue, watch } from 'vue';
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
import type { UserInfoMinimal } from '@/models/user/user';
import { formatStudent } from '@/helpers/formatters';

const props = defineProps<{
    slot: AttendanceSlot;
}>();

const toast = useToast();
const overlay = useOverlay();
const userStore = useUser();

const filterPerson = shallowRef();
const accordionValue = shallowRef<string | undefined>(undefined);

const attendanceService = useAttendance('slot', props.slot.scope, props.slot.slotId, toast);
const attendance = attendanceService.slotAttendance;

onUnmounted(() => attendanceService.stop());

function updateAttendanceCallback(student: UserInfoMinimal, status: AttendanceState) {
    attendanceService.updateAttendance(student.id, status);
}

async function move(enrollment: AttendanceStudentStatus) {
    const modal = overlay.create(MoveStudentForm);
    const data: { all: boolean; destination: string | undefined } = await modal.open({
        student: enrollment.student,
        angebote: toValue(attendance),
        canMoveNow: attendanceService.canMoveNowNow(),
    });
    if (data == undefined || data.destination == undefined) return;
    if (data.all) {
        await attendanceService.moveStudent(enrollment.student.id, data.destination);
    } else {
        await attendanceService.moveStudentNow(enrollment.student.id, data.destination);
    }
}

async function initMoveHere(eventId: string) {
    const modal = overlay.create(SelectStudentToMoveForm);
    const data = await modal.open({ canMoveNow: attendanceService.canMoveNowNow() });

    if (!data || !data.student || data.all === undefined) return;
    if (data.all) {
        await attendanceService.moveStudent(data.student, eventId);
    } else {
        await attendanceService.moveStudentNow(data.student, eventId);
    }
}

function openNotes(data: AttendanceStudentStatus) {
    const modal = overlay.create(Notes);

    modal.open({
        notes: computed(() => data.notes),
        myNote: computed(
            () => data.notes.find((n) => n.creator.id === userStore.user!.id) ?? null,
        ),
        scope: props.slot.scope,
        slotId: props.slot.slotId,
        studentId: data.student.id,
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
    <div v-if="attendanceService.metadata.value?.supervisors" class="mb-8">
        Angekündigte Aufsichten:
        {{
            attendanceService.metadata.value.supervisors.length > 0
                ? attendanceService.metadata.value.supervisors
                      .map((e) => formatStudent(e))
                      .join(', ')
                : 'keine'
        }}
    </div>
    <UFieldGroup class="mb-6 w-full" size="lg">
        <PersonSelectorNuxt
            class="w-full"
            v-model="filterPerson"
            :filter="(s: UserInfoMinimal) => s.rolle === 'Mittelstufe'"
            hide-rolle
            placeholder="Schüler:in suchen"
        />
        <UButton
            :disabled="filterPerson == undefined"
            aria-label="Filter entfernen"
            color="neutral"
            icon="i-lucide-x"
            variant="outline"
            @click="filterPerson = undefined"
        />
    </UFieldGroup>
    <UAccordion
        v-model="accordionValue"
        :items="filteredAttendance"
        :ui="{
            label: 'flex justify-between w-full items-center mr-1',
        }"
        value-key="eventId"
        value-label="name"
    >
        <template #content="{ item }">
            <EnrollmentTable
                :enableEdit="true"
                :enableMove="attendanceService.metadata.value?.enableMove ?? false"
                :enableNotes="attendanceService.metadata.value?.enableNotes ?? false"
                :enrollments="item.enrollments"
                :showAttendance="true"
                @move="move"
                @openNotes="openNotes"
                @update="updateAttendanceCallback"
            >
                <template v-if="attendanceService.metadata.value?.enableMove ?? false" #actions>
                    <UButton
                        color="neutral"
                        icon="i-lucide-plus"
                        label="Schüler:in hinzufügen"
                        variant="subtle"
                        @click="() => initMoveHere(item.eventId)"
                    />
                </template>
            </EnrollmentTable>
        </template>
        <template #default="{ item }">
            <span class="flex-1"> {{ item.location }} - {{ item.name }} </span>
            <span v-if="!filterActive" class="inline-flex gap-3 items-baseline">
                {{ item.enrollments.length }} Schüler:innen
                <UButton
                    :color="
                        item.status
                            ? item.enrollments.some(
                                  (e: AttendanceStudentStatus) => e.status === 'Fehlend',
                              )
                                ? 'warning'
                                : 'success'
                            : 'error'
                    "
                    :label="item.status ? 'Fertig' : 'Ausstehend'"
                    :ui="{
                        base: 'justify-center',
                    }"
                    class="w-28"
                    size="md"
                    @click.stop="
                        () => attendanceService.updateStatus(item.eventId, !item.status)
                    "
                />
            </span>
        </template>
    </UAccordion>
</template>

<style scoped></style>
