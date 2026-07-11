<script lang="ts" setup>
import UserPeek from '@/components/UserPeek.vue';
import AttendanceButton from '@/Attendance/components/AttendanceButton.vue';
import type { AttendanceState, AttendanceStudentStatus } from '@/Attendance/models/attendance';
import type { UserInfoMinimal } from '@/models/user/user';
import type { TableColumn } from '@nuxt/ui/components/Table.d.vue.ts';
import { computed, h, useSlots } from 'vue';

const props = defineProps<{
    showAttendance: boolean;
    enableEdit: boolean;
    enableMove: boolean;
    enableNotes: boolean;
    enrollments: AttendanceStudentStatus[];
}>();

const emit = defineEmits<{
    update: [student: UserInfoMinimal, value: AttendanceState];
    move: [student: AttendanceStudentStatus];
    openNotes: [student: AttendanceStudentStatus];
}>();

const slots = useSlots();

const studentColumn: TableColumn<AttendanceStudentStatus> = {
    id: 'student',
    header: 'Schüler:in',
    cell: ({ row }) => h(UserPeek, { person: row.original.student, showGroup: true }),
    meta: {
        class: {
            td: 'w-full',
        },
    },
};

const attendanceColumn: TableColumn<AttendanceStudentStatus> = {
    id: 'attendance',
    header: 'Anwesenheit',
    meta: {
        class: {
            th: 'text-right',
        },
    },
};

const actionColumn: TableColumn<AttendanceStudentStatus> = {
    id: 'action',
    header: 'Aktionen',
    meta: {
        class: {
            th: 'text-right',
        },
    },
};

const columns = computed<TableColumn<AttendanceStudentStatus>[]>(() => {
    const array = [studentColumn];
    if (props.showAttendance) array.push(attendanceColumn);
    if (props.enableMove || props.enableNotes || slots.studentActions) array.push(actionColumn);

    return array;
});
</script>

<template>
    <div>
        <UTable :columns="columns" :data="enrollments">
            <template #empty>Keine Einschreibungen</template>
            <template #attendance-cell="{ row }">
                <div class="flex gap-2 items-center justify-end">
                    <UTooltip text="Diese Anwesenheit wurde automatisch festgestellt">
                        <UBadge
                            v-if="
                                row.original.type == 'Automatic' &&
                                row.original.student.rolle === 'Mittelstufe' &&
                                (showAttendance || enableEdit)
                            "
                            color="secondary"
                            label="A"
                        />
                    </UTooltip>
                    <AttendanceButton
                        class="w-auto"
                        v-if="
                            row.original.student.rolle === 'Mittelstufe' &&
                            (showAttendance || enableEdit)
                        "
                        :mayEdit="enableEdit"
                        :status="row.original.status"
                        @update="(value) => emit('update', row.original.student, value)"
                    />
                    <UBadge
                        v-else-if="showAttendance || enableEdit"
                        color="secondary"
                        label="N/A"
                    />
                </div>
            </template>
            <template #action-cell="{ row }">
                <div class="flex gap-2 items-end justify-end">
                    <UTooltip text="In ein anderes Otium verschieben">
                        <UButton
                            v-if="enableMove"
                            aria-label="Verschieben"
                            color="secondary"
                            icon="i-lucide-fast-forward"
                            variant="ghost"
                            @click="() => emit('move', row.original)"
                        />
                    </UTooltip>
                    <UTooltip text="Notizen">
                        <UButton
                            v-if="enableNotes"
                            :color="row.original.notes.length !== 0 ? 'warning' : 'secondary'"
                            :variant="row.original.notes.length !== 0 ? 'solid' : 'ghost'"
                            aria-label="Notizen"
                            icon="i-lucide-clipboard"
                            @click="() => emit('openNotes', row.original)"
                        />
                    </UTooltip>
                    <slot
                        v-if="$slots.studentActions"
                        :data="row.original"
                        name="studentActions"
                    ></slot>
                </div>
            </template>
        </UTable>
        <template v-if="$slots.actions">
            <USeparator />
            <div class="p-4">
                <slot name="actions"></slot>
            </div>
        </template>
    </div>
</template>

<style scoped></style>
