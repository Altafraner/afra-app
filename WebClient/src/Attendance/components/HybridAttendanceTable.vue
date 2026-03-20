<script lang="ts" setup>
import { computed, onUnmounted, shallowReactive, shallowRef, toValue, watch } from 'vue';
import type {
    AttendanceEvent,
    AttendanceState,
    AttendanceStudentStatus,
} from '@/Attendance/models/attendance';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import { Button, useDialog, useToast } from 'primevue';
import { useUser } from '@/stores/user';
import { useAttendance } from '@/Attendance/composables/attendanceHubClient';
import EnrollmentTable from '@/Attendance/components/EnrollmentTable.vue';
import Notes from '@/Attendance/components/Notes.vue';
import MoveStudentForm from '@/Attendance/components/MoveStudentForm.vue';
import SelectStudentToMoveForm from '@/Attendance/components/SelectStudentToMoveForm.vue';

const props = defineProps<{
    enableSupervision: boolean;
    showAttendance: boolean;
    continueSyncAfterSupervisionStop?: boolean;
    scope: string;
    slotId: string;
    eventId: string;
    enrollments: AttendanceStudentStatus[];
}>();

const emit = defineEmits<{
    updateAttendance: [data: AttendanceStudentStatus[]];
}>();

const toast = useToast();
const dialog = useDialog();
const userStore = useUser();

const showMove = shallowRef<boolean>(false);
const showNotes = shallowRef<boolean>(false);
const stopEventSync = shallowRef<(() => void) | null>(null);
const stopServiceWatchers = shallowRef<Array<() => void>>([]);

const attendanceFunctions = shallowReactive<{
    close?: () => Promise<void>;
    updateAttendance?: (studentId: string, status: AttendanceState) => Promise<void>;
    updateStatus?: (eventId: string, status: boolean) => Promise<void>;
    updateAlternatives?: () => Promise<void>;
    moveStudent?: (studentId: string, terminId: string) => Promise<void>;
    moveStudentNow?: (studentId: string, terminId: string) => Promise<void>;
    canMoveNowNow?: () => boolean;
}>({});

const alternatives = shallowRef<AttendanceEvent[]>([]);

watch(
    () => props.enableSupervision,
    (newValue) => {
        if (newValue) {
            startSupervision();
        } else {
            stopSupervision();
        }
    },
    { immediate: true },
);

async function startSupervision() {
    const attendanceService = useAttendance(
        'event',
        props.scope,
        props.slotId,
        toast,
        props.eventId,
    );
    stopEventSync.value?.();
    for (const stopWatcher of stopServiceWatchers.value) stopWatcher();
    stopServiceWatchers.value = [];

    // one way sync from hub to parent
    stopEventSync.value = watch(
        attendanceService.eventAttendance,
        (newValue) => {
            if (newValue !== null) emit('updateAttendance', toValue(newValue));
        },
        { deep: true, immediate: true },
    );

    stopServiceWatchers.value.push(
        watch(
            attendanceService.alternatives,
            (newValue) => {
                alternatives.value = newValue ?? [];
            },
            { immediate: true },
        ),
    );

    stopServiceWatchers.value.push(
        watch(
            attendanceService.capabilities,
            (newValue) => {
                showMove.value = Boolean(newValue?.enableMove);
                showNotes.value = Boolean(newValue?.enableNotes);
            },
            { immediate: true },
        ),
    );

    attendanceFunctions.close = attendanceService.stop;
    attendanceFunctions.updateAttendance = attendanceService.updateAttendance;
    attendanceFunctions.updateStatus = attendanceService.updateStatus;
    attendanceFunctions.updateAlternatives = attendanceService.updateAlternatives;
    attendanceFunctions.moveStudent = attendanceService.moveStudent;
    attendanceFunctions.moveStudentNow = attendanceService.moveStudentNow;
    attendanceFunctions.canMoveNowNow = attendanceService.canMoveNowNow;
}

async function stopSupervision() {
    await attendanceFunctions.updateStatus?.(props.eventId, true);
    if (!props.continueSyncAfterSupervisionStop) {
        stopEventSync.value?.();
        stopEventSync.value = null;
        for (const stopWatcher of stopServiceWatchers.value) stopWatcher();
        stopServiceWatchers.value = [];
        await attendanceFunctions.close?.();
    }
}

onUnmounted(async () => {
    stopEventSync.value?.();
    for (const stopWatcher of stopServiceWatchers.value) stopWatcher();
    stopServiceWatchers.value = [];
    await attendanceFunctions.close?.();
});

async function attendanceUpdate(student: UserInfoMinimal, value: AttendanceState) {
    await attendanceFunctions.updateAttendance?.(student.id, value);
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
            scope: props.scope,
            slotId: props.slotId,
            studentId: data.student.id,
        },
    });
}

async function move(enrollment: AttendanceStudentStatus) {
    await attendanceFunctions.updateAlternatives?.();

    dialog.open(MoveStudentForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            student: enrollment.student,
            angebote: alternatives.value,
            canMoveNow: attendanceFunctions.canMoveNowNow?.() ?? false,
        },
        onClose: onCloseMove,
    });

    async function onCloseMove({ data }: { data?: { destination: string; all: boolean } }) {
        if (!data) return;
        if (data.all) {
            await attendanceFunctions.moveStudent(enrollment.student.id, data.destination);
        } else {
            await attendanceFunctions.moveStudentNow(enrollment.student.id, data.destination);
        }
    }
}

function openMoveHere() {
    dialog.open(SelectStudentToMoveForm, {
        props: {
            header: 'Schüler:in verschieben',
            modal: true,
            class: 'sm:max-w-xl',
        },
        data: {
            canMoveNow: attendanceFunctions.canMoveNowNow?.() ?? false,
        },
        onClose: onCloseMove,
    });

    async function onCloseMove({ data }: { data?: { student: string; all: boolean } }) {
        if (!data) return;
        if (data.all) {
            await attendanceFunctions.moveStudent(data.student, props.eventId);
        } else {
            await attendanceFunctions.moveStudentNow(data.student, props.eventId);
        }
    }
}
</script>

<template>
    <EnrollmentTable
        :enableEdit="enableSupervision"
        :enableMove="showMove"
        :enableNotes="showNotes"
        :enrollments="enrollments"
        :showAttendance="showAttendance"
        @move="move"
        @openNotes="openNotes"
        @update="attendanceUpdate"
    >
        <template v-if="showMove || $slots.actions" #actions>
            <Button
                v-if="showMove"
                icon="pi pi-plus"
                label="Schüler:in hinzufügen"
                severity="secondary"
                size="small"
                @click="openMoveHere"
            />
            <slot v-if="$slots.actions" name="actions"></slot>
        </template>
        <template v-if="$slots.studentActions" #studentActions="{ data }">
            <slot :data="data" name="studentActions"></slot>
        </template>
    </EnrollmentTable>
</template>

<style scoped></style>
