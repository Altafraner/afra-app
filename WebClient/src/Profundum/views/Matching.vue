<script setup>
import { DataTable, Checkbox, Column, Button, Message, Select, useToast } from 'primevue';
import { mande } from 'mande';
import { computed, ref } from 'vue';

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);
const matchingRunning = ref(false);
const toast = useToast();

async function getSlots() {
    slots.value = await mande('/api/profundum/management/slot').get();
}

async function getEnrollments() {
    enrollments.value = await mande('/api/profundum/management/enrollments').get();
}

async function getInstanzen() {
    instanzen.value = await mande('/api/profundum/management/instanz').get();
}

async function autoMatching() {
    matchingRunning.value = true;
    try {
        const r = await mande('/api/profundum/management/matching').post();
        toast.add({
            severity: 'success',
            summary: 'Erfolg',
            detail: 'Matching',
        });
        console.error(r);
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Matching aufgetreten. ' + e,
        });
        console.error(e);
    } finally {
        enrollments.value = await mande('/api/profundum/management/enrollments').get();
        matchingRunning.value = false;
    }
}

const enrollmentForSlot = (row, slotId) =>
    row.enrollments?.find((e) => e.profundumSlotId === slotId);

async function updateEnrollment(row) {
    const updater = mande(`/api/profundum/management/enrollment/${row.person.id}`);

    const payload = row.enrollments
        .filter((e) => e.profundumInstanzId || e.isFixed)
        .map((e) => ({
            profundumInstanzId: e.profundumInstanzId,
            profundumSlotId: e.profundumSlotId,
            isFixed: e.isFixed,
        }));

    await updater.put(payload);
    toast.add({
        severity: 'success',
        summary: 'Gespeichert.',
        detail: 'Änderung Gepeichert.',
    });
}

const instanzenBySlot = computed(() => {
    const map = new Map();
    for (const instanz of instanzen.value) {
        for (const slotId of instanz.slots ?? []) {
            if (!map.has(slotId)) map.set(slotId, []);
            map.get(slotId).push(instanz);
        }
    }
    return map;
});

const instanzenForSlot = (slotId) => instanzenBySlot.value.get(slotId) ?? [];

getSlots();
getEnrollments();
getInstanzen();

const wishForOption = (row, option) => {
    return row.wuensche?.find((w) => w.id === option.profundumInfo.id) ?? null;
};

const sortedInstanzenForSlot = (slotId, row) => {
    const options = instanzenForSlot(slotId);
    const selectedId = enrollmentForSlot(row, slotId)?.profundumInstanzId;

    return [...options].sort((a, b) => {
        const wishA = wishForOption(row, a);
        const wishB = wishForOption(row, b);

        const score = (opt, wish) => {
            if (opt.id === selectedId) return 0;
            if (wish) return 10 + wish.rang;
            return 100;
        };

        return score(a, wishA) - score(b, wishB);
    });
};
</script>
<template>
    <h1>Profunda-Matching</h1>

    <Button
        :disabled="matchingRunning"
        label="Automatisches Matching aktualisieren"
        @click="autoMatching"
    />

    <DataTable
        :value="enrollments"
        value-key="id"
        size="small"
        class="datatable-compact"
        scrollable
        virtualScrollerOptions
    >
        <Column header="Person">
            <template #body="{ data }">
                {{ data.person.vorname }} {{ data.person.nachname }} ({{ data.person.gruppe }})
            </template>
        </Column>

        <Column header="Speichern" style="width: 3rem">
            <template #body="{ data }">
                <Button
                    icon="pi pi-save"
                    severity="success"
                    rounded
                    text
                    @click="updateEnrollment(data)"
                    :disabled="!data.enrollments || data.enrollments.length === 0"
                />
            </template>
        </Column>

        <Column
            v-for="slot of slots"
            :key="slot.id"
            :header="`${slot.jahr}-${slot.quartal}-${slot.wochentag}`"
        >
            <template #body="{ data }">
                <span class="flex gap-1 items-center">
                    <Checkbox binary v-model="enrollmentForSlot(data, slot.id).isFixed" />

                    <Select
                        filter
                        class="w-60 select-compact"
                        :options="sortedInstanzenForSlot(slot.id, data)"
                        option-label="profundumInfo.bezeichnung"
                        option-value="id"
                        v-model="enrollmentForSlot(data, slot.id).profundumInstanzId"
                        :disabled="!enrollmentForSlot(data, slot.id).isFixed"
                    >
                        <template #option="slotProps">
                            <div class="option-row">
                                <span>{{ slotProps.option.profundumInfo.bezeichnung }}</span>

                                <span v-if="wishForOption(data, slotProps.option)">
                                    ★ {{ wishForOption(data, slotProps.option).rang }}
                                </span>
                            </div>
                        </template>
                    </Select>
                </span>
            </template>
        </Column>
    </DataTable>

    <Button label="Matching finalisieren" severity="warn" />
</template>

<style scoped>
.datatable-compact :deep(.p-datatable-thead > tr > th),
.datatable-compact :deep(.p-datatable-tbody > tr > td) {
    padding: 0.25rem 0.5rem;
    font-size: 0.75rem;
}

.datatable-compact :deep(.p-select) {
    font-size: 0.7rem;
}
:deep(.select-compact .p-select-label) {
    font-size: 0.8rem;
}

:deep(.select-compact .p-select-trigger) {
    width: 1.75rem;
}

.option-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.option-row :last-child {
    font-weight: 600;
    color: var(--primary-color);
}
</style>
