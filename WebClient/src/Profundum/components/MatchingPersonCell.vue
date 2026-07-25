<script setup>
import UserPeek from '@/components/UserPeek.vue';
import { formatSlot, formatStudent } from '@/helpers/formatters.ts';

const props = defineProps({
    row: { type: Object, required: true },
    slots: { type: Array, required: true },
    profunda: { type: Array, required: true },
    editing: { type: Boolean, default: false },
});

defineEmits(['start-edit', 'save']);

const partnerFor = (partnerschaft) =>
    partnerschaft.personA.id === props.row.person.id
        ? partnerschaft.personB
        : partnerschaft.personA;

const slotLabel = (slotId) => {
    const s = props.slots.find((x) => x.id === slotId);
    return s ? formatSlot(s) : 'Unbekannter Slot';
};

const wuenscheBySlot = () => {
    const map = new Map();

    for (const w of props.row.wuensche ?? []) {
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

    const slotOrder = props.slots.map((s) => s.id);

    return [...map.entries()].toSorted(
        ([a], [b]) => slotOrder.indexOf(a) - slotOrder.indexOf(b),
    );
};
</script>

<template>
    <span class="grid grid-cols-[19em_1fr_1fr_1fr_1fr] gap-1">
        <UserPeek :person="row.person" class="w-full min-w-0" showGroup />

        <UPopover v-if="row.wuensche.length !== 0">
            <UButton icon="i-lucide-crown" color="info" variant="ghost" size="sm" />
            <template #content>
                <div
                    v-for="[slotId, wishes] of wuenscheBySlot()"
                    :key="slotId"
                    class="mb-2 p-3"
                >
                    <b class="block mb-1">{{ slotLabel(slotId) }}</b>
                    <ul class="ml-3">
                        <li v-for="w in wishes" :key="`${slotId}-${w.id}`">
                            {{ w.rang }}.
                            {{ profunda.find((p) => p.id === w.id)?.bezeichnung ?? '—' }}
                        </li>
                    </ul>
                </div>
            </template>
        </UPopover>
        <span v-else></span>

        <UTooltip v-if="(row.partnerschaften?.length ?? 0) !== 0" text="Partnerschaft(en)">
            <UPopover>
                <UButton icon="i-lucide-users" color="primary" variant="ghost" size="sm" />
                <template #content>
                    <ul class="list-disc pl-4 p-3">
                        <li v-for="p in row.partnerschaften" :key="p.id">
                            {{ p.bezeichnung }}: mit {{ formatStudent(partnerFor(p)) }}
                        </li>
                    </ul>
                </template>
            </UPopover>
        </UTooltip>
        <span v-else></span>

        <UPopover v-if="row.warnings.length !== 0">
            <UButton icon="i-lucide-triangle-alert" color="warning" variant="ghost" size="sm" />
            <template #content>
                <ul class="list-disc pl-4 p-3">
                    <li v-for="w in row.warnings" :key="w">
                        {{ w.text }}
                    </li>
                </ul>
            </template>
        </UPopover>
        <span v-else></span>

        <UButton
            v-if="!editing"
            icon="i-lucide-pencil"
            color="neutral"
            variant="ghost"
            size="sm"
            @click="$emit('start-edit')"
        />
        <UButton
            v-else
            icon="i-lucide-check"
            color="success"
            variant="ghost"
            size="sm"
            @click="$emit('save')"
        />
    </span>
</template>
