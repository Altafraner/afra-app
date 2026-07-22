<script lang="ts" setup>
import { useManagement } from '@/Profundum/composables/verwaltung.ts';
import { formatCalendarDateTime, formatSlot } from '@/helpers/formatters.ts';
import HybridAttendanceTable from '@/Attendance/components/HybridAttendanceTable.vue';
import { shallowRef } from 'vue';
import { AttendanceStudentStatus } from '@/Attendance/models/attendance.ts';
import { parseDateTime } from '@internationalized/date';

const props = defineProps<{
    terminId: string;
    instanceId: string;
}>();

const management = useManagement();
const data = await management.getTerminInstanceInfo(props.terminId, props.instanceId);
const attendance = shallowRef(data?.enrollments ?? []);
const startTime = data ? parseDateTime(data.start) : null;
const niceStartTime = startTime ? formatCalendarDateTime(startTime) : null;

const attendanceActive = shallowRef(false);

function updateAttendance(data: AttendanceStudentStatus[]) {
    attendance.value = data;
}
</script>

<template>
    <h1>{{ data?.label }}</h1>
    <p v-if="data">{{ niceStartTime }} · {{ formatSlot(data.slot) }}</p>
    <UCard v-if="data" class="mt-8">
        <template #header>
            <div class="flex gap-2 justify-between items-center">
                <div>
                    <div class="text-highlighted font-semibold">Einschreibungen</div>
                    <div class="mt-1 text-muted text-sm">
                        Zum Zeitpunkt des Profundums können Sie hier die Anwesenheit
                        kontrollieren
                    </div>
                </div>
                <UButton
                    v-if="data.isAttendanceEditable && !attendanceActive"
                    icon="i-lucide-play"
                    label="Anwesenheitskontrolle starten"
                    @click="attendanceActive = true"
                />
                <UButton
                    v-else-if="data.isAttendanceEditable && attendanceActive"
                    icon="i-lucide-square"
                    label="Anwesenheitskontrolle beenden"
                    @click="attendanceActive = false"
                />
            </div>
        </template>
        <HybridAttendanceTable
            :enable-supervision="attendanceActive"
            :enrollments="attendance"
            :event-id="instanceId"
            :show-attendance="data.isDoneOrStarted"
            :slot-id="terminId"
            scope="Profundum"
            @updateAttendance="updateAttendance"
        />
    </UCard>
</template>

<style scoped></style>
