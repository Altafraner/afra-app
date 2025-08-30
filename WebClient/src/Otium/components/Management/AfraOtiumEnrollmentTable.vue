<script setup>
import { Badge, Button, Column, DataTable } from 'primevue';
import AfraOtiumAnwesenheit from '@/Otium/components/Shared/AfraOtiumAnwesenheit.vue';
import UserPeek from '@/components/UserPeek.vue';

const props = defineProps({
    enrollments: Array,
    showAttendance: Boolean,
    mayEditAttendance: Boolean,
    updateFunction: Function,
    showTransfer: Boolean,
    showRemove: Boolean,
});

const emit = defineEmits(['initMove', 'remove']);

function initMove(student) {
    emit('initMove', student);
}
</script>

<template>
    <DataTable :data-key="(value) => value.student.id" :value="props.enrollments">
        <Column header="Schüler:in">
            <template #body="{ data }">
                <UserPeek :person="data.student" :showGroup="true" />
            </template>
        </Column>
        <Column
            header="Anwesenheit"
            v-if="props.showAttendance || props.mayEditAttendance"
            class="text-right afra-col-action"
        >
            <template #body="{ data }">
                <span class="flex justify-end items-center gap-2">
                    <afra-otium-anwesenheit
                        v-if="data.student.rolle !== 'Oberstufe'"
                        v-model="data.anwesenheit"
                        :mayEdit="props.mayEditAttendance"
                        @value-changed="(value) => updateFunction(data.student, value)"
                    />
                    <badge
                        v-else
                        v-tooltip="'Oberstufenschüler:innen haben keine Anwesenheit'"
                        severity="secondary"
                    >
                        N/A
                    </badge>
                    <Button
                        v-if="mayEditAttendance && showTransfer"
                        v-tooltip="'In anderes Otium verschieben'"
                        icon="pi pi-forward"
                        severity="secondary"
                        size="small"
                        variant="text"
                        aria-label="Verschieben"
                        @click="() => initMove(data.student)"
                    />
                    <Button
                        v-if="!mayEditAttendance && showRemove"
                        v-tooltip="'Ausschreiben'"
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        aria-label="Ausschreiben"
                        @click="(evt) => emit('remove', evt, data.student)"
                    />
                </span>
            </template>
        </Column>
        <template #empty>
            <div class="flex justify-center">Keine Einschreibungen</div>
        </template>
    </DataTable>
</template>

<style scoped></style>
