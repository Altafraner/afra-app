<script lang="ts" setup>
import { Badge, Button, Column, DataTable } from 'primevue';
import UserPeek from '@/components/UserPeek.vue';
import AttendanceButton from '@/Attendance/components/AttendanceButton.vue';
import type { AttendanceState, AttendanceStudentStatus } from '@/Attendance/models/attendance';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';

defineProps<{
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
</script>

<template>
    <DataTable :data-key="(value) => value.student.id" :value="enrollments">
        <Column header="Schüler:in">
            <template #body="{ data }">
                <UserPeek :person="data.student" :showGroup="true" />
            </template>
        </Column>
        <Column
            v-if="showAttendance || enableEdit || $slots.studentActions"
            class="text-right afra-col-action"
        >
            <template #body="{ data }">
                <span class="flex justify-end items-center gap-2">
                    <AttendanceButton
                        v-if="
                            data.student.rolle === 'Mittelstufe' &&
                            (showAttendance || enableEdit)
                        "
                        :mayEdit="enableEdit"
                        :status="data.status"
                        @update="(value) => emit('update', data.student, value)"
                    />
                    <badge
                        v-else-if="showAttendance || enableEdit"
                        v-tooltip="
                            'Für Schüler:innen außerhalb der Mittelstufe werden keine Anwesenheiten erfasst'
                        "
                        severity="secondary"
                    >
                        N/A
                    </badge>
                    <Button
                        v-if="enableMove"
                        v-tooltip="'In anderes Otium verschieben'"
                        aria-label="Verschieben"
                        icon="pi pi-forward"
                        severity="secondary"
                        size="small"
                        variant="text"
                        @click="() => emit('move', data)"
                    />
                    <Button
                        v-if="enableNotes"
                        v-tooltip="'Notizen'"
                        :severity="data.notes.length !== 0 ? 'warn' : 'secondary'"
                        aria-label="Notizen"
                        icon="pi pi-clipboard"
                        size="small"
                        variant="text"
                        @click="() => emit('openNotes', data)"
                    />
                    <slot
                        v-if="$slots.studentActions"
                        :data="data"
                        name="studentActions"
                    ></slot>
                </span>
            </template>
        </Column>
        <template v-if="$slots.actions" #footer>
            <slot v-if="$slots.actions" name="actions"></slot>
        </template>
        <template #empty>
            <div class="flex justify-center">Keine Einschreibungen</div>
        </template>
    </DataTable>
</template>

<style scoped></style>
