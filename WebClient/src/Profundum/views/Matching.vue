<script setup>
import { DataTable, Checkbox, Column, Button, Message, Select, useToast } from 'primevue';
import { mande } from 'mande';
import { computed, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover.js';
import UserPeek from '@/components/UserPeek.vue';

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);
const matchingRunning = ref(false);
const toast = useToast();
const confirm = useConfirmPopover();

async function getSlots() {
    slots.value = await mande('/api/profundum/management/slot').get();
}

async function getEnrollments() {
    enrollments.value = await mande('/api/profundum/management/enrollments').get();
}

async function getInstanzen() {
    instanzen.value = await mande('/api/profundum/management/instanz').get();
}

async function pollMatching(id) {
    const client = mande(`/api/profundum/management/matching/${id}`);

    while (true) {
        await new Promise((r) => setTimeout(r, 1000));

        try {
            return await client.get();
        } catch (e) {
            if (e?.response?.status === 404) {
                continue;
            }

            throw e;
        }
    }
}

async function autoMatching() {
    matchingRunning.value = true;

    try {
        const id = await mande('/api/profundum/management/matching').post();

        if (!id) throw new Error('Keine Matching-ID erhalten');

        const r = await pollMatching(id);
        toast.add({
            severity: 'success',
            summary: 'Erfolg',
            detail: r.result,
        });
        console.log(r);
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Matching aufgetreten. ' + e,
        });
        console.error(e);
    } finally {
        await getEnrollments();
        matchingRunning.value = false;
    }
}

async function finalize() {
    confirm.openConfirmDialog(
        event,
        async () => {
            await mande('/api/profundum/management/finalize').post();
            enrollments.value = await mande('/api/profundum/management/enrollments').get();
        },
        'Matching finalisieren',
        'Alle Einschreibungen werden fixiert. Automatisches Matching ist hiernach nichtmehr sinnvoll.',
        'danger',
    );
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

    return options.toSorted((a, b) => {
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

const editingPersonId = ref(null);

const startEdit = (row) => {
    editingPersonId.value = row.person.id;
};

const stopEdit = () => {
    editingPersonId.value = null;
};

const isEditing = (row) => editingPersonId.value === row.person.id;
</script>
<template>
    <h1>Profunda-Matching</h1>

    <span class="flex gap-1 mb-4">
        <Button
            :disabled="matchingRunning"
            label="Automatisches Matching aktualisieren"
            @click="autoMatching"
        />

        <Button label="Matching finalisieren" severity="warn" @click="finalize" />
    </span>

    <DataTable
        :value="enrollments"
        value-key="id"
        size="small"
        class="datatable-compact"
        scrollable
    >
        <Column header="Person">
            <template #body="{ data }">
                <UserPeek :person="data.person" :showGroup="true" />
            </template>
        </Column>

        <Column header="Aktionen" style="width: 5rem">
            <template #body="{ data }">
                <span class="flex">
                    <Button
                        v-if="!isEditing(data)"
                        icon="pi pi-pencil"
                        text
                        @click="startEdit(data)"
                    />

                    <Button
                        v-else
                        icon="pi pi-check"
                        severity="success"
                        text
                        @click="
                            () => {
                                updateEnrollment(data);
                                stopEdit();
                            }
                        "
                    />
                </span>
            </template>
        </Column>

        <Column
            v-for="slot of slots"
            :key="slot.id"
            :header="`${slot.jahr}-${slot.quartal}-${slot.wochentag}`"
        >
            <template #body="{ data }">
                <span class="flex gap-1 items-center">
                    <template v-if="isEditing(data)">
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
                                    <span>{{
                                        slotProps.option.profundumInfo.bezeichnung
                                    }}</span>

                                    <span v-if="wishForOption(data, slotProps.option)">
                                        ★ {{ wishForOption(data, slotProps.option).rang }}
                                    </span>
                                </div>
                            </template>
                        </Select>
                    </template>
                    <template v-else>
                        <span class="readonly-value w-60">
                            <template v-if="enrollmentForSlot(data, slot.id)?.isFixed">
                                <div class="text-orange-500 flex gap-1">
                                    <i class="pi pi-lock" />
                                    <b>
                                        {{
                                            instanzen.find(
                                                (i) =>
                                                    i.id ===
                                                    enrollmentForSlot(data, slot.id)
                                                        ?.profundumInstanzId,
                                            )?.profundumInfo.bezeichnung ?? '—'
                                        }}
                                    </b>
                                </div>
                            </template>
                            <template v-else>
                                {{
                                    instanzen.find(
                                        (i) =>
                                            i.id ===
                                            enrollmentForSlot(data, slot.id)
                                                ?.profundumInstanzId,
                                    )?.profundumInfo.bezeichnung ?? '—'
                                }}
                            </template>
                        </span>
                    </template>
                </span>
            </template>
        </Column>
    </DataTable>
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

.readonly-value {
    display: inline-flex;
}
.readonly-value.fixed {
    font-weight: 800;
    color: orange;
}
</style>
