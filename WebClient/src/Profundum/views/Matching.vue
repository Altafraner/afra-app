<script setup>
import { DataTable, Checkbox, Column, Button, Message, Select } from 'primevue';
import { mande } from 'mande';
import { computed, ref } from 'vue';

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);

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
    await mande('/api/profundum/management/matching').post();
}

const enrollmentForSlot = (row, slotId) => {
    return row.enrollments.find((e) => e.profundumSlotId === slotId);
};

const getOrCreateEnrollmentForSlot = (row, slotId) => {
    if (!Array.isArray(row.enrollments)) {
        row.enrollments = [];
    }

    let enr = row.enrollments.find((e) => e.profundumSlotId === slotId);

    if (!enr) {
        enr = {
            profundumSlotId: slotId,
            profundumInstanzId: null,
            isFixed: false,
        };
        row.enrollments.push(enr);
    }

    return enr;
};


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

    Message.success('Änderung gespeichert');
}


const instanzenBySlot = computed(() => {
    const map = new Map();
    for (const instanz of instanzen.value) {
        for (const slotId of instanz.slots ?? []) {
            if (!map.has(slotId)) {
                map.set(slotId, []);
            }
            map.get(slotId).push(instanz);
        }
    }
    return map;
});

const instanzenForSlot = (slotId) => instanzenBySlot.value.get(slotId) ?? [];

getSlots();
getEnrollments();
getInstanzen();
</script>
<template>
    <h1>Profunda-Matching</h1>

    <Button label="Automatisches Matching aktualisieren" @click="autoMatching" />

    <DataTable :value="enrollments" value-key="id" size="small" class="datatable-compact">
        <Column header="Person">
            <template #body="{ data }">
                {{ data.person.vorname }} {{ data.person.nachname }}
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
                    <Checkbox
                        binary
                        v-model="getOrCreateEnrollmentForSlot(data, slot.id).isFixed"
                    />

                    <Select
                        filter
                        class="w-80 select-compact"
                        :options="instanzenForSlot(slot.id)"
                        option-label="profundumInfo.bezeichnung"
                        option-value="id"
                        v-model="getOrCreateEnrollmentForSlot(data, slot.id).profundumInstanzId"
                        :disabled="!getOrCreateEnrollmentForSlot(data, slot.id).isFixed"
                    />
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
</style>
