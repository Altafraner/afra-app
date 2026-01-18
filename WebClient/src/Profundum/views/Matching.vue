<script setup>
import {
    Button,
    Checkbox,
    Column,
    DataTable,
    InputText,
    MultiSelect,
    Popover,
    Select,
    Tag,
    useToast,
} from 'primevue';
import { mande } from 'mande';
import { computed, nextTick, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UserPeek from '@/components/UserPeek.vue';
import { FilterMatchMode, FilterService } from '@primevue/core/api';
import { formatSlot } from '@/helpers/formatters.ts';

const instanzenFilters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },

    'profundumInfo.bezeichnung': {
        value: null,
        matchMode: FilterMatchMode.CONTAINS,
    },
    slots: {
        value: null,
        matchMode: 'slotsAny',
    },
});

FilterService.register('slotsAny', (rowSlots, selectedSlots) => {
    if (!selectedSlots || selectedSlots.length === 0) return true;
    if (!Array.isArray(rowSlots)) return false;
    return selectedSlots.some((s) => rowSlots.includes(s));
});

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);
const profunda = ref([]);
const matchingRunning = ref(false);
const toast = useToast();
const confirm = useConfirmPopover();

const selectedItem = ref();
const warningsRef = ref();
const wuenschePopoverRef = ref();

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

const MATCH_DURATION = 240;

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
        if (e?.response?.status === 429) {
            toast.add({
                severity: 'warn',
                summary: 'Matching läuft bereits',
                detail: 'Das Matching wird gerade von einer anderen Sitzung ausgeführt. Bitte warten.',
            });
        } else {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Es ist ein Fehler beim Matching aufgetreten. ' + e,
            });
        }
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
        return true;
    } catch (err) {
        console.error(err);
        if (err?.response?.status === 429) {
            toast.add({
                severity: 'error',
                summary: 'Matching läuft.',
                detail: 'Das Matching wird gerade von einer anderen Sitzung ausgeführt. Bitte warten und erneut eintragen..',
            });
        } else {
            let message = 'Speichern fehlgeschlagen.';
            if (err?.response?.data) {
                message += ' ' + err.response.data;
            }
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: message,
            });
        }
        return false;
    } finally {
        getEnrollments();
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

const wishMovedForSelectedEnrollment = (row, slotId) => {
    const enrollment = enrollmentForSlot(row, slotId);
    if (!enrollment?.profundumInstanzId) return null;

    const instanz = instanzen.value.find((i) => i.id === enrollment.profundumInstanzId);

    if (!instanz) return null;

    return wishMovedForOption(row, instanz);
};

getSlots();
getEnrollments();
getInstanzen();
getProfunda();

const wishForOption = (row, option) => {
    return row.wuensche?.find((w) => w.id === option.id) ?? null;
};

const wishMovedForOption = (row, option) => {
    if (!option?.profundumId) return null;

    const matches =
        row.wuensche
            ?.map((w) => {
                const wishedInstanz = instanzen.value.find((i) => i.id === w.id);
                if (!wishedInstanz) return null;

                const isSameProfundum = wishedInstanz.profundumId === option.profundumId;
                const isDifferentInstanz = wishedInstanz.id !== option.id;

                return isSameProfundum && isDifferentInstanz ? w : null;
            })
            .filter(Boolean) ?? [];

    if (matches.length === 0) return null;

    return matches.reduce((best, w) => (w.rang < best.rang ? w : best), matches[0]);
};

const wishInfoForSlot = (row, slotId) => {
    const normal = wishForSelectedEnrollment(row, slotId);
    const moved = wishMovedForSelectedEnrollment(row, slotId);
    return { normal, moved };
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
    return s ? formatSlot(s) : 'Unbekannter Slot';
};

function showWarnings(evt, data) {
    warningsRef.value.hide();
    selectedItem.value = data;
    nextTick(() => warningsRef.value.show(evt));
}

function showWishes(evt, data) {
    wuenschePopoverRef.value.hide();
    selectedItem.value = data;
    nextTick(() => wuenschePopoverRef.value.show(evt));
}
</script>
<template>
    <h1>Profunda-Matching</h1>

    <span class="flex flex-col gap-6">
        <span class="flex gap-1">
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
                    <span v-if="matchingRunning" class="match-btn__sec">
                        < {{ remaining }}s</span
                    >
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
            <Column frozen class="z-0 border-r">
                <template #header>
                    <span class="inline-flex justify-between w-full font-semibold">
                        <span>Person</span>
                        <span>Aktion</span>
                    </span>
                </template>
                <template #body="{ data, index }">
                    <span class="grid grid-cols-[16em_1fr_1fr_1fr] gap-1">
                        <UserPeek :person="data.person" class="w-full" showGroup />
                        <Button
                            v-if="data.wuensche.length !== 0"
                            icon="pi pi-crown"
                            severity="info"
                            text
                            size="small"
                            @click="showWishes($event, data)"
                        />
                        <span v-else></span>
                        <Button
                            v-if="data.warnings.length !== 0"
                            icon="pi pi-exclamation-triangle"
                            severity="warn"
                            text
                            size="small"
                            @click="showWarnings($event, data)"
                        />
                        <span v-else></span>
                        <Button
                            v-if="!isEditing(data)"
                            icon="pi pi-pencil"
                            text
                            size="small"
                            @click="startEdit(data)"
                        />
                        <Button
                            v-else
                            icon="pi pi-check"
                            severity="success"
                            text
                            size="small"
                            @click="
                                async () => {
                                    if (await updateEnrollment(data)) {
                                        stopEdit();
                                    }
                                }
                            "
                        />
                    </span>
                </template>
            </Column>

            <Column v-for="slot of slots" :key="slot.id" :header="formatSlot(slot)">
                <template #body="{ data }">
                    <span class="flex gap-1 items-center">
                        <template v-if="isEditing(data)">
                            <Checkbox
                                binary
                                v-model="enrollmentForSlot(data, slot.id).isFixed"
                            />

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
                                    v-if="wishInfoForSlot(data, slot.id).normal"
                                    class="wish-indicator text-green-500"
                                >
                                    <i class="pi pi-crown" />
                                    {{ wishInfoForSlot(data, slot.id).normal.rang }}
                                </span>

                                <span
                                    v-else-if="wishInfoForSlot(data, slot.id).moved"
                                    class="wish-indicator text-yellow-500"
                                >
                                    <i class="pi pi-arrow-right-arrow-left" />
                                    {{ wishInfoForSlot(data, slot.id).moved.rang }}
                                </span>

                                <template v-if="enrollmentForSlot(data, slot.id)?.isFixed">
                                    <div class="text-orange-600 flex gap-1 items-center">
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

        <Popover ref="wuenschePopoverRef" dismissable showCloseIcon style="min-width: 15rem">
            <div
                v-if="selectedItem"
                v-for="[slotId, wishes] of wuenscheBySlot(selectedItem)"
                :key="slotId"
                class="mb-2"
            >
                <b class="block mb-1">{{ slotLabel(slotId) }}</b>

                <ul class="ml-3">
                    <li v-for="w in wishes" :key="`${slotId}-${w.id}`">
                        {{ w.rang }}.
                        {{
                            instanzen.find((p) => p.id == w.id)?.profundumInfo.bezeichnung ??
                            '—'
                        }}
                    </li>
                </ul>
            </div>
        </Popover>
        <Popover ref="warningsRef" dismissable showCloseIcon style="min-width: 15rem">
            <ul class="list-disc pl-4" v-if="selectedItem">
                <li v-for="w in selectedItem.warnings" :key="w">
                    {{ w.text }}
                </li>
            </ul>
        </Popover>

        <DataTable
            :value="instanzen"
            v-model:filters="instanzenFilters"
            filterDisplay="row"
            :globalFilterFields="[
                'profundumInfo.bezeichnung',
                'numEinschreibungen',
                'maxEinschreibungen',
            ]"
        >
            <Column field="profundumInfo.bezeichnung" header="Bezeichnung" filter>
                <template #filter="{ filterModel, filterCallback }">
                    <InputText
                        v-model="filterModel.value"
                        placeholder="Suchen…"
                        class="p-column-filter w-full"
                        @input="filterCallback()"
                    />
                </template>

                <template #body="{ data }">
                    <Button
                        as="RouterLink"
                        :to="{
                            name: 'Profundum-Edit',
                            params: { profundumId: data.profundumId },
                        }"
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
            <Column header="slots" field="slots" filter>
                <template #filter="{ filterModel, filterCallback }">
                    <MultiSelect
                        v-model="filterModel.value"
                        :options="slots"
                        optionLabel="jahr"
                        optionValue="id"
                        placeholder="Slots wählen…"
                        display="chip"
                        class="p-column-filter w-full"
                        @change="filterCallback()"
                    >
                        <template #option="{ option }">
                            {{ formatSlot(option) }}
                        </template>

                        <template #chip="{ value }">
                            {{ slotLabel(value) }}
                        </template>
                    </MultiSelect>
                </template>

                <template #body="{ data }">
                    <span class="flex flex-wrap gap-1">
                        <Tag
                            v-for="slotId in data.slots"
                            :key="slotId"
                            class="text-sm px-1.5"
                            severity="secondary"
                        >
                            {{ slotLabel(slotId) }}
                        </Tag>
                    </span>
                </template>
            </Column>
            <Column header="Warnung">
                <template #body="{ data }">
                    <i
                        v-if="data.maxEinschreibungen < data.numEinschreibungen"
                        class="pi pi-exclamation-triangle text-xl p-2 inline-block bg-yellow-200 text-yellow-800 dark:text-yellow-400 dark:bg-yellow-950 rounded-lg"
                    />
                </template>
            </Column>
            <Column field="numEinschreibungen" header="Einschreibungen"></Column>
            <Column field="maxEinschreibungen" header="MaxEinschreibungen"></Column>
        </DataTable>
    </span>
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
    width: 0;
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
    white-space: nowrap;
}
</style>
