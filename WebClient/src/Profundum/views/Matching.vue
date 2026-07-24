<script setup>
import { mande } from 'mande';
import { computed, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UserPeek from '@/components/UserPeek.vue';
import { formatSlot, formatStudent } from '@/helpers/formatters.ts';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

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
    return row.wuensche?.find((w) => w.id === option.profundumId) ?? null;
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

const partnerFor = (row, partnerschaft) =>
    partnerschaft.personA.id === row.person.id ? partnerschaft.personB : partnerschaft.personA;

const visibleSlotIds = ref([]);
const slotSelectItems = computed(() =>
    slots.value.map((s) => ({ id: s.id, label: formatSlot(s) })),
);
const visibleSlots = computed(() =>
    slots.value.filter((s) => visibleSlotIds.value.includes(s.id)),
);

const enrollmentColumns = computed(() => [
    { id: 'person' },
    ...visibleSlots.value.map((slot) => ({ id: slot.id, header: formatSlot(slot) })),
]);

const bezeichnungFilter = ref('');
const slotsFilter = ref([]);

const filteredInstanzen = computed(() =>
    instanzen.value.filter((row) => {
        const matchesBezeichnung =
            !bezeichnungFilter.value ||
            row.profundumInfo.bezeichnung
                .toLowerCase()
                .includes(bezeichnungFilter.value.toLowerCase());
        const matchesSlots =
            slotsFilter.value.length === 0 ||
            slotsFilter.value.some((s) => row.slots?.includes(s));
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

        <USelect
            v-model="visibleSlotIds"
            :items="slotSelectItems"
            label-key="label"
            value-key="id"
            multiple
            class="w-80"
            placeholder="Slots anzeigen…"
        />

        <UTable
            :data="enrollments"
            :columns="enrollmentColumns"
            size="sm"
            :loading="matchingRunning"
            :ui="{ root: 'overflow-x-auto' }"
        >
            <template #person-header>
                <span class="inline-flex justify-between w-full font-semibold">
                    <span>Person</span>
                    <span>Aktion</span>
                </span>
            </template>
            <template #person-cell="{ row }">
                <span
                    class="grid grid-cols-[16em_1fr_1fr_1fr_1fr] gap-1 sticky left-0 bg-default"
                >
                    <UserPeek :person="row.original.person" class="w-full" showGroup />

                    <UPopover v-if="row.original.wuensche.length !== 0">
                        <UButton icon="i-lucide-crown" color="info" variant="ghost" size="sm" />
                        <template #content>
                            <div
                                v-for="[slotId, wishes] of wuenscheBySlot(row.original)"
                                :key="slotId"
                                class="mb-2 p-3"
                            >
                                <b class="block mb-1">{{ slotLabel(slotId) }}</b>
                                <ul class="ml-3">
                                    <li v-for="w in wishes" :key="`${slotId}-${w.id}`">
                                        {{ w.rang }}.
                                        {{
                                            profunda.find((p) => p.id === w.id)?.bezeichnung ??
                                            '—'
                                        }}
                                    </li>
                                </ul>
                            </div>
                        </template>
                    </UPopover>
                    <span v-else></span>

                    <UTooltip
                        v-if="(row.original.partnerschaften?.length ?? 0) !== 0"
                        text="Partnerschaft(en)"
                    >
                        <UPopover>
                            <UButton
                                icon="i-lucide-users"
                                color="primary"
                                variant="ghost"
                                size="sm"
                            />
                            <template #content>
                                <ul class="list-disc pl-4 p-3">
                                    <li v-for="p in row.original.partnerschaften" :key="p.id">
                                        {{ p.bezeichnung }}: mit
                                        {{ formatStudent(partnerFor(row.original, p)) }}
                                    </li>
                                </ul>
                            </template>
                        </UPopover>
                    </UTooltip>
                    <span v-else></span>

                    <UPopover v-if="row.original.warnings.length !== 0">
                        <UButton
                            icon="i-lucide-triangle-alert"
                            color="warning"
                            variant="ghost"
                            size="sm"
                        />
                        <template #content>
                            <ul class="list-disc pl-4 p-3">
                                <li v-for="w in row.original.warnings" :key="w">
                                    {{ w.text }}
                                </li>
                            </ul>
                        </template>
                    </UPopover>
                    <span v-else></span>

                    <UButton
                        v-if="!isEditing(row.original)"
                        icon="i-lucide-pencil"
                        color="neutral"
                        variant="ghost"
                        size="sm"
                        @click="startEdit(row.original)"
                    />
                    <UButton
                        v-else
                        icon="i-lucide-check"
                        color="success"
                        variant="ghost"
                        size="sm"
                        @click="
                            async () => {
                                if (await updateEnrollment(row.original)) {
                                    stopEdit();
                                }
                            }
                        "
                    />
                </span>
            </template>

            <template
                v-for="slot in visibleSlots"
                :key="slot.id"
                #[`${slot.id}-cell`]="{ row }"
            >
                <span class="flex gap-1 items-center">
                    <template v-if="isEditing(row.original)">
                        <USwitch v-model="enrollmentForSlot(row.original, slot.id).isFixed" />

                        <USelectMenu
                            v-model="
                                enrollmentForSlot(row.original, slot.id).profundumInstanzId
                            "
                            :items="sortedInstanzenForSlot(slot.id, row.original)"
                            label-key="profundumInfo.bezeichnung"
                            value-key="id"
                            clear
                            class="w-60"
                            :disabled="!enrollmentForSlot(row.original, slot.id).isFixed"
                        >
                            <template #item="{ item }">
                                <span class="option-row gap-2">
                                    <span v-if="wishForOption(row.original, item)">
                                        ★ {{ wishForOption(row.original, item).rang }}
                                    </span>
                                    <span>{{ item.profundumInfo.bezeichnung }}</span>
                                    <span
                                        >({{ item.numEinschreibungen }} /
                                        {{ item.maxEinschreibungen }})</span
                                    >
                                </span>
                            </template>
                        </USelectMenu>
                    </template>
                    <template v-else>
                        <span class="readonly-value w-60 flex items-center gap-2">
                            <span
                                v-if="wishForSelectedEnrollment(row.original, slot.id)"
                                class="wish-indicator text-green-500"
                            >
                                <UIcon name="i-lucide-crown" />
                                {{ wishForSelectedEnrollment(row.original, slot.id).rang }}
                            </span>

                            <template v-if="enrollmentForSlot(row.original, slot.id)?.isFixed">
                                <div class="text-orange-600 flex gap-1 items-center">
                                    <UIcon name="i-lucide-lock" />
                                    <b>
                                        {{
                                            instanzen.find(
                                                (i) =>
                                                    i.id ===
                                                    enrollmentForSlot(row.original, slot.id)
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
                                            enrollmentForSlot(row.original, slot.id)
                                                ?.profundumInstanzId,
                                    )?.profundumInfo.bezeichnung ?? '—'
                                }}
                            </template>
                        </span>
                    </template>
                </span>
            </template>
        </UTable>

        <div class="flex flex-wrap gap-3 items-center">
            <UInput
                v-model="bezeichnungFilter"
                placeholder="Bezeichnung suchen…"
                class="w-64"
            />
            <USelect
                v-model="slotsFilter"
                :items="slotSelectItems"
                label-key="label"
                value-key="id"
                multiple
                placeholder="Slots filtern…"
                class="w-80"
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
                    class="text-xl p-2 inline-block bg-yellow-200 text-yellow-800 dark:text-yellow-400 dark:bg-yellow-950 rounded-lg"
                />
            </template>
        </UTable>
    </span>
</template>

<style scoped>
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

.wish-indicator {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    font-weight: 700;
    white-space: nowrap;
}
</style>
