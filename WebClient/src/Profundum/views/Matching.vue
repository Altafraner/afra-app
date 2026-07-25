<script setup>
import { mande } from 'mande';
import { computed, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { formatSlot } from '@/helpers/formatters.ts';
import { fuzzyMatch } from '@/helpers/fuzzy.ts';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import MatchingPersonCell from '@/Profundum/components/MatchingPersonCell.vue';
import MatchingSlotCell from '@/Profundum/components/MatchingSlotCell.vue';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Verwaltung',
        to: {
            name: 'Profundum-Verwaltung',
        },
    },
    {
        label: 'Matching',
    },
];

const slots = ref([]);
const enrollments = ref([]);
const instanzen = ref([]);
const profunda = ref([]);
const matchingRunning = ref(false);
const toast = useToast();
const confirm = useConfirmPopover();

async function getSlots() {
    slots.value = await mande('/api/profundum/management/slot').get();
    visibleSlotIds.value = slots.value.map((s) => s.id);
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
            color: 'success',
            title: 'Erfolg',
            description: r.result,
        });
    } catch (e) {
        if (e?.response?.status === 429) {
            toast.add({
                color: 'warning',
                title: 'Matching läuft bereits',
                description:
                    'Das Matching wird gerade von einer anderen Sitzung ausgeführt. Bitte warten.',
            });
        } else {
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: 'Es ist ein Fehler beim Matching aufgetreten. ' + e,
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
    if (
        !(await confirm.requireConfirm(
            'Alle Einschreibungen werden fixiert. Automatisches Matching ist hiernach nichtmehr sinnvoll.',
            'Matching finalisieren',
        ))
    )
        return;
    await mande('/api/profundum/management/finalize').post();
    enrollments.value = await mande('/api/profundum/management/enrollments').get();
}

const enrollmentsByPersonAndSlot = computed(() => {
    const map = new Map();
    for (const row of enrollments.value) {
        map.set(row.person.id, new Map(row.enrollments?.map((e) => [e.profundumSlotId, e])));
    }
    return map;
});
const enrollmentForSlot = (row, slotId) =>
    enrollmentsByPersonAndSlot.value.get(row.person.id)?.get(slotId);

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
            color: 'success',
            title: 'Gespeichert',
            description: 'Änderung gespeichert.',
        });
        return true;
    } catch (err) {
        console.error(err);
        if (err?.response?.status === 429) {
            toast.add({
                color: 'error',
                title: 'Matching läuft.',
                description:
                    'Das Matching wird gerade von einer anderen Sitzung ausgeführt. Bitte warten und erneut eintragen..',
            });
        } else {
            let message = 'Speichern fehlgeschlagen.';
            if (err?.response?.data) {
                message += ' ' + err.response.data;
            }
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: message,
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

getSlots();
getEnrollments();
getInstanzen();
getProfunda();

const editingPersonId = ref(null);

const startEdit = (row) => {
    editingPersonId.value = row.person.id;
};

const stopEdit = () => {
    editingPersonId.value = null;
};

const isEditing = (row) => editingPersonId.value === row.person.id;

async function handleSave(row) {
    if (await updateEnrollment(row)) {
        stopEdit();
    }
}

const visibleSlotIds = ref([]);
const slotSelectItems = computed(() =>
    slots.value.map((s) => ({ id: s.id, label: formatSlot(s) })),
);
const visibleSlots = computed(() =>
    slots.value.filter((s) => visibleSlotIds.value.includes(s.id)),
);

const personFilter = ref('');
const filteredEnrollments = computed(() => {
    if (!personFilter.value.trim()) return enrollments.value;

    return enrollments.value
        .map((row) => {
            const haystack = [
                row.person.vorname,
                row.person.nachname,
                row.person.gruppe,
                row.person.email,
            ]
                .filter(Boolean)
                .join(' ');
            return { row, score: fuzzyMatch(personFilter.value, haystack) };
        })
        .filter(({ score }) => score !== null)
        .toSorted((a, b) => b.score - a.score)
        .map(({ row }) => row);
});

const enrollmentColumns = computed(() => [
    { id: 'person' },
    ...visibleSlots.value.map((slot) => ({ id: slot.id, header: formatSlot(slot) })),
]);

const columnPinning = ref({ left: ['person'] });

const slotLabel = (slotId) => {
    const s = slots.value.find((x) => x.id === slotId);
    return s ? formatSlot(s) : 'Unbekannter Slot';
};

const bezeichnungFilter = ref('');

const filteredInstanzen = computed(() =>
    instanzen.value.filter((row) => {
        const matchesBezeichnung =
            !bezeichnungFilter.value ||
            row.profundumInfo.bezeichnung
                .toLowerCase()
                .includes(bezeichnungFilter.value.toLowerCase());
        const matchesSlots =
            visibleSlotIds.value.length === 0 ||
            visibleSlotIds.value.some((s) => row.slots?.includes(s));
        return matchesBezeichnung && matchesSlots;
    }),
);

const instanzenColumns = [
    { id: 'bezeichnung', header: 'Bezeichnung' },
    { id: 'pdf', header: 'pdf' },
    { id: 'slots', header: 'Slots' },
    { id: 'warnung', header: 'Überbelegung' },
    { id: 'numEinschreibungen', accessorKey: 'numEinschreibungen', header: 'Einschreibungen' },
    {
        id: 'maxEinschreibungen',
        accessorKey: 'maxEinschreibungen',
        header: 'MaxEinschreibungen',
    },
];
</script>
<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Profunda-Matching</h1>

    <span class="flex flex-col gap-6">
        <span class="flex gap-1">
            <UButton :disabled="matchingRunning" class="match-btn" @click="autoMatching">
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
            </UButton>

            <UButton label="Matching finalisieren" color="warning" @click="finalize" />

            <UButton
                :href="`/api/profundum/management/matching.csv`"
                icon="i-lucide-table"
                download
                label="CSV-Export"
            />
        </span>

        <span class="flex flex-wrap gap-3 items-center">
            <USelect
                v-model="visibleSlotIds"
                :items="slotSelectItems"
                label-key="label"
                value-key="id"
                multiple
                class="w-80"
                placeholder="Slots anzeigen/filtern…"
            />

            <UInput
                v-model="personFilter"
                icon="i-lucide-search"
                placeholder="Person suchen…"
                class="w-64"
            />
        </span>

        <UTable
            :data="filteredEnrollments"
            :columns="enrollmentColumns"
            v-model:column-pinning="columnPinning"
            sticky="header"
            size="sm"
            :loading="matchingRunning"
            :ui="{
                root: 'overflow-auto max-h-[80vh]',
                thead: 'bg-default backdrop-blur-none',
                td: 'data-[pinned]:bg-default data-[pinned]:backdrop-blur-none',
                th: 'data-[pinned]:bg-default data-[pinned]:backdrop-blur-none',
            }"
        >
            <template #person-header>
                <span class="inline-flex justify-between w-full font-semibold">
                    <span>Person</span>
                    <span>Aktion</span>
                </span>
            </template>
            <template #person-cell="{ row }">
                <MatchingPersonCell
                    :row="row.original"
                    :slots="slots"
                    :profunda="profunda"
                    :editing="isEditing(row.original)"
                    @start-edit="startEdit(row.original)"
                    @save="handleSave(row.original)"
                />
            </template>

            <template
                v-for="slot in visibleSlots"
                :key="slot.id"
                #[`${slot.id}-cell`]="{ row }"
            >
                <MatchingSlotCell
                    :enrollment="enrollmentForSlot(row.original, slot.id)"
                    :wuensche="row.original.wuensche"
                    :options="instanzenForSlot(slot.id)"
                    :editing="isEditing(row.original)"
                />
            </template>
        </UTable>

        <div class="flex flex-wrap gap-3 items-center">
            <UInput
                v-model="bezeichnungFilter"
                placeholder="Bezeichnung suchen…"
                class="w-64"
            />
        </div>

        <UTable :data="filteredInstanzen" :columns="instanzenColumns">
            <template #bezeichnung-cell="{ row }">
                <UButton
                    :to="{
                        name: 'Profundum-Edit',
                        params: { profundumId: row.original.profundumId },
                    }"
                    variant="ghost"
                    :label="row.original.profundumInfo.bezeichnung"
                />
            </template>
            <template #pdf-cell="{ row }">
                <UTooltip text="PDF (experimentell)">
                    <UButton
                        :href="`/api/profundum/management/instanz/${row.original.id}.pdf`"
                        icon="i-lucide-file-text"
                        variant="ghost"
                        size="sm"
                        download
                        color="info"
                        aria-label="PDF (experimentell)"
                    />
                </UTooltip>
            </template>
            <template #slots-cell="{ row }">
                <span class="flex flex-wrap gap-1">
                    <UBadge
                        v-for="slotId in row.original.slots"
                        :key="slotId"
                        class="text-sm px-1.5"
                        color="neutral"
                        variant="subtle"
                    >
                        {{ slotLabel(slotId) }}
                    </UBadge>
                </span>
            </template>
            <template #warnung-cell="{ row }">
                <UIcon
                    v-if="
                        row.original.maxEinschreibungen &&
                        row.original.maxEinschreibungen < row.original.numEinschreibungen
                    "
                    name="i-lucide-triangle-alert"
                    class="text-xl p-2 inline-block bg-warning/15 text-warning rounded-lg"
                />
            </template>
        </UTable>
    </span>
</template>

<style scoped>
.match-btn {
    position: relative;
    overflow: hidden;
}

.match-btn__bg {
    position: absolute;
    inset: 0;
    width: 0;
    background: color-mix(in srgb, var(--ui-primary) 22%, transparent);
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
    border-left: 1px solid color-mix(in srgb, var(--ui-primary) 35%, var(--ui-border));
    min-width: 3.2rem;
    text-align: right;
}
</style>
