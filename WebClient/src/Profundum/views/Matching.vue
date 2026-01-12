<script setup>
import {
    DataTable,
    Checkbox,
    Column,
    Button,
    Message,
    Popover,
    Select,
    useToast,
} from 'primevue';
import { mande } from 'mande';
import { computed, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UserPeek from '@/components/UserPeek.vue';

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);
const profunda = ref([]);
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

async function getProfunda() {
    profunda.value = await mande('/api/profundum/management/profundum').get();
}

const MATCH_DURATION = 60;

const remaining = ref(0);
const fillPct = computed(() => {
    if (!matchingRunning.value) return 0;
    const elapsed = MATCH_DURATION - remaining.value;
    return Math.max(0, Math.min(100, (elapsed / MATCH_DURATION) * 100));
});

let timer = null;

function startCountdown() {
    remaining.value = MATCH_DURATION;
    clearInterval(timer);
    timer = setInterval(() => {
        remaining.value--;
        if (remaining.value <= 0) {
            clearInterval(timer);
            timer = null;
            remaining.value = 0;
        }
    }, 1000);
}

function stopCountdown() {
    clearInterval(timer);
    timer = null;
    remaining.value = 0;
}

async function autoMatching() {
    matchingRunning.value = true;
    startCountdown();

    try {
        const r = await mande('/api/profundum/management/matching').post();
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
        getEnrollments();
        getInstanzen();
        matchingRunning.value = false;
        stopCountdown();
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

    try {
        await updater.put(payload);
        toast.add({
            severity: 'success',
            summary: 'Gespeichert',
            detail: 'Änderung gespeichert.',
            life: 1500,
        });
        await getEnrollments();
        return true;
    } catch (err) {
        console.error(err);

        let message = 'Speichern fehlgeschlagen.';
        if (err?.response?.data) {
            message += ' ' + err.response.data;
        }
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: message,
            life: 4000,
        });
        return false;
    }
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

const wishForSelectedEnrollment = (row, slotId) => {
    const enrollment = enrollmentForSlot(row, slotId);
    if (!enrollment?.profundumInstanzId) return null;

    const instanz = instanzen.value.find((i) => i.id === enrollment.profundumInstanzId);

    if (!instanz) return null;

    return wishForOption(row, instanz);
};

getSlots();
getEnrollments();
getInstanzen();
getProfunda();

const wishForOption = (row, option) => {
    return row.wuensche?.find((w) => w.id === option.id) ?? null;
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

const warningPops = ref([]);
const wuenschePops = ref([]);

const isEditing = (row) => editingPersonId.value === row.person.id;

const wuenscheBySlot = (row) => {
    const map = new Map();

    for (const w of row.wuensche ?? []) {
        for (const slotId of w.slotId ?? []) {
            if (!map.has(slotId)) map.set(slotId, []);
            map.get(slotId).push(w);
        }
    }

    for (const [slotId, list] of map) {
        map.set(
            slotId,
            list.toSorted((a, b) => a.rang - b.rang),
        );
    }

    const slotOrder = slots.value.map((s) => s.id);

    return [...map.entries()].toSorted(
        ([a], [b]) => slotOrder.indexOf(a) - slotOrder.indexOf(b),
    );
};

const slotLabel = (slotId) => {
    const s = slots.value.find((x) => x.id === slotId);
    return s ? `${s.jahr}-${s.quartal}-${s.wochentag}` : 'Unbekannter Slot';
};
</script>
<template>
    <h1>Profunda-Matching</h1>

    <span class="flex gap-1 mb-4">
        <Button :disabled="matchingRunning" class="match-btn" @click="autoMatching">
            <span class="match-btn__bg" :style="{ width: fillPct + '%' }" />
            <span class="match-btn__content">
                <span>
                    {{
                        matchingRunning
                            ? 'Matching läuft…'
                            : 'Automatisches Matching aktualisieren'
                    }}
                </span>
                <span v-if="matchingRunning" class="match-btn__sec">< {{ remaining }}s</span>
            </span>
        </Button>

        <Button label="Matching finalisieren" severity="warn" @click="finalize" />

        <Button
            as="a"
            :href="`/api/profundum/management/matching.csv`"
            icon="pi pi-table"
            download
            label="CSV-Export"
        />
    </span>

    <DataTable
        :value="enrollments"
        value-key="id"
        size="small"
        class="datatable-compact"
        scrollable
        :loading="matchingRunning"
    >
        <Column header="Person">
            <template #body="{ data }">
                <UserPeek :person="data.person" class="w-full" showGroup />
            </template>
        </Column>

        <Column header="Wünsche" style="width: 5rem">
            <template #body="{ data, index }">
                <Button
                    v-if="data.wuensche.length !== 0"
                    icon="pi pi-crown"
                    severity="info"
                    text
                    @click="(e) => wuenschePops[index].toggle(e)"
                />

                <Popover
                    :ref="(el) => (wuenschePops[index] = el)"
                    dismissable
                    showCloseIcon
                    style="min-width: 15rem"
                >
                    <div
                        v-for="[slotId, wishes] of wuenscheBySlot(data)"
                        :key="slotId"
                        class="mb-2"
                    >
                        <b class="block mb-1">{{ slotLabel(slotId) }}</b>

                        <ul class="ml-3">
                            <li v-for="w in wishes" :key="`${slotId}-${w.id}`">
                                {{ w.rang }}.
                                {{
                                    instanzen.find((p) => p.id == w.id)?.profundumInfo
                                        .bezeichnung ?? '—'
                                }}
                            </li>
                        </ul>
                    </div>
                </Popover>
            </template>
        </Column>

        <Column header="Warnungen" style="width: 5rem">
            <template #body="{ data, index }">
                <Button
                    v-if="data.warnings.length !== 0"
                    icon="pi pi-exclamation-triangle"
                    severity="warn"
                    text
                    @click="(e) => warningPops[index].toggle(e)"
                />

                <Popover
                    :ref="(el) => (warningPops[index] = el)"
                    dismissable
                    showCloseIcon
                    style="min-width: 15rem"
                >
                    <ul class="list-disc pl-4">
                        <li v-for="w in data.warnings" :key="w">
                            {{ w.text }}
                        </li>
                    </ul>
                </Popover>
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
                            showClear
                            filter
                            class="w-60 select-compact"
                            :options="sortedInstanzenForSlot(slot.id, data)"
                            option-label="profundumInfo.bezeichnung"
                            option-value="id"
                            v-model="enrollmentForSlot(data, slot.id).profundumInstanzId"
                            :disabled="!enrollmentForSlot(data, slot.id).isFixed"
                        >
                            <template #option="slotProps">
                                <span class="option-row gap-2">
                                    <span v-if="wishForOption(data, slotProps.option)">
                                        ★ {{ wishForOption(data, slotProps.option).rang }}
                                    </span>

                                    <span>{{
                                        slotProps.option.profundumInfo.bezeichnung
                                    }}</span>

                                    <span
                                        >({{
                                            instanzen
                                                .map((i) => {
                                                    return {
                                                        id: i.id,
                                                        value: `${i.numEinschreibungen} / ${i.maxEinschreibungen}`,
                                                    };
                                                })
                                                .find((i) => i.id === slotProps.option.id)
                                                ?.value
                                        }})</span
                                    >
                                </span>
                            </template>
                        </Select>
                    </template>
                    <template v-else>
                        <span class="readonly-value w-60 flex items-center gap-2">
                            <span
                                v-if="wishForSelectedEnrollment(data, slot.id)"
                                class="wish-indicator"
                            >
                                <i class="pi pi-crown" />
                                {{ wishForSelectedEnrollment(data, slot.id).rang }}
                            </span>

                            <template v-if="enrollmentForSlot(data, slot.id)?.isFixed">
                                <div class="text-orange-500 flex gap-1 items-center">
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

    <DataTable :value="instanzen">
        <Column field="profundumInfo.bezeichnung" header="Bezeichnung">
            <template #body="{ data }">
                <Button
                    as="RouterLink"
                    :to="{ name: 'Profundum-Edit', params: { profundumId: data.profundumId } }"
                    text
                >
                    {{ data.profundumInfo.bezeichnung }}
                </Button>
            </template>
        </Column>
        <Column header="pdf">
            <template #body="{ data }">
                <Button
                    as="a"
                    :href="`/api/profundum/management/instanz/${data.id}.pdf`"
                    icon="pi pi-file-pdf"
                    variant="text"
                    size="small"
                    download
                    severity="info"
                    v-tooltip.left="'PDF (experimentell)'"
                    aria-label="PDF (experimentell)'"
                />
            </template>
        </Column>
        <Column header="slots">
            <template #body="{ data }">
                <span v-for="s in data.slots">
                    {{ slotLabel(slots.find((x) => x.id === s).id) }},
                </span>
            </template>
        </Column>
        <Column header="Warnung">
            <template #body="{ data }">
                <Button
                    v-if="data.maxEinschreibungen < data.numEinschreibungen"
                    icon="pi pi-exclamation-triangle"
                    severity="warn"
                    disabled
                />
            </template>
        </Column>
        <Column field="numEinschreibungen" header="Einschreibungen"></Column>
        <Column field="maxEinschreibungen" header="MaxEinschreibungen"></Column>
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
    font-style: italic;
}

.readonly-value {
    display: inline-flex;
}
.readonly-value.fixed {
    font-weight: 800;
    color: orange;
}

.match-btn {
    position: relative;
    overflow: hidden;
}

.match-btn__bg {
    position: absolute;
    inset: 0;
    width: 0%;
    background: color-mix(in srgb, var(--primary-color) 22%, transparent);
    transition: width 1s linear;
    pointer-events: none;
}

.match-btn__content {
    position: relative;
    display: inline-flex;
    align-items: center;
    gap: 0.75rem;
}

.match-btn__sec {
    font-weight: 700;
    font-size: 0.85rem;
    padding-left: 0.75rem;
    border-left: 1px solid color-mix(in srgb, var(--primary-color) 35%, var(--surface-border));
    min-width: 3.2rem;
    text-align: right;
}

.wish-indicator {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    font-weight: 700;
    color: green;
    white-space: nowrap;
}
</style>
