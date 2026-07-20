<script lang="ts" setup>
import type { AttendanceState } from '@/Attendance/models/attendance';

defineProps<{
    mayEdit: Boolean;
    minimal?: Boolean;
    status: AttendanceState;
}>();

const emit = defineEmits<{
    update: [value: AttendanceState];
}>();

const toggle = (value: number) => {
    emit('update', stati[value]);
};

const buttonColors = {
    Fehlend: 'error',
    Entschuldigt: 'warning',
    Anwesend: 'success',
};

const icons = {
    Fehlend: 'i-lucide-x',
    Entschuldigt: 'i-lucide-clipboard',
    Anwesend: 'i-lucide-check',
};

const stati: AttendanceState[] = ['Fehlend', 'Entschuldigt', 'Anwesend'];
</script>

<template>
    <UFieldGroup v-if="mayEdit">
        <UButton
            :label="stati[0]"
            :color="status === stati[0] ? 'error' : 'neutral'"
            @click="() => toggle(0)"
            :variant="status === stati[0] ? 'solid' : 'soft'"
        />
        <UButton
            :label="stati[1]"
            :color="status === stati[1] ? 'warning' : 'neutral'"
            @click="() => toggle(1)"
            :variant="status === stati[1] ? 'solid' : 'soft'"
        />
        <UButton
            :label="stati[2]"
            :color="status === stati[2] ? 'success' : 'neutral'"
            @click="() => toggle(2)"
            :variant="status === stati[2] ? 'solid' : 'soft'"
        />
    </UFieldGroup>
    <UBadge v-else-if="!minimal" :color="buttonColors[status]">{{ status }}</UBadge>
    <UTooltip v-else :text="status">
        <UIcon :class="'text-' + buttonColors[status]" :name="icons[status]" class="size-5" />
    </UTooltip>
</template>

<style scoped></style>
