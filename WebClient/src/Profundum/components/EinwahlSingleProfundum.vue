<script setup>
import { formatSlotId } from '@/helpers/formatters.ts';

defineProps({
    angebot: { type: Object, required: true },
    index: { type: Number, required: false },
    isRanked: { type: Boolean, required: true },
    lastInList: { type: Boolean, default: false },
    hasPartner: { type: Boolean, default: false },
});

defineEmits([
    'openPartnerDialog',
    'openDetailDialog',
    'moveUp',
    'moveDown',
    'removeFromRanked',
    'addToRanked',
]);
</script>

<template>
    <li class="grid grid-cols-subgrid col-span-9 items-center gap-2 rounded p-2 bg-muted">
        <UIcon
            v-if="isRanked"
            class="drag-handle cursor-grab text-muted col-1"
            name="i-lucide-grip-vertical"
        />
        <span v-if="isRanked" class="w-6 text-right font-bold col-2">{{ index + 1 }}.</span>
        <span class="flex-1 col-3">
            <UBadge v-if="angebot.profilProfundum" color="info" label="Profil" />
            {{ angebot.bezeichnung }}
        </span>
        <span class="flex flex-col gap-1 gap-x-1 col-4 flex-wrap justify-stretch">
            <UBadge
                v-for="slot in angebot.slotIds"
                :key="slot"
                :label="formatSlotId(slot)"
                variant="outline"
                color="secondary"
            />
        </span>
        <UTooltip v-if="angebot.erlaubtPartnerwahl" class="col-5" text="Partnerwahl">
            <UButton
                :class="{
                    'hover:bg-accented': !hasPartner,
                }"
                :color="hasPartner ? 'success' : 'neutral'"
                :variant="hasPartner ? 'solid' : 'ghost'"
                icon="i-lucide-users"
                @click="$emit('openPartnerDialog', angebot)"
            />
        </UTooltip>
        <UTooltip
            :class="{
                'col-start-6 col-end-8': !isRanked,
            }"
            class="col-6"
            text="Details"
        >
            <UButton
                class="hover:bg-accented"
                color="neutral"
                icon="i-lucide-info"
                variant="ghost"
                @click="$emit('openDetailDialog', angebot)"
            />
        </UTooltip>
        <UButton
            v-if="isRanked"
            :disabled="index === 0"
            color="primary"
            icon="i-lucide-arrow-up"
            variant="ghost"
            @click="$emit('moveUp', index)"
            class="col-7"
        />
        <UButton
            v-if="isRanked"
            :disabled="lastInList"
            color="primary"
            icon="i-lucide-arrow-down"
            variant="ghost"
            @click="$emit('moveDown', index)"
            class="col-8"
        />
        <UButton
            v-if="isRanked"
            color="error"
            icon="i-lucide-x"
            variant="ghost"
            class="col-9"
            @click="$emit('removeFromRanked', index)"
        />
        <UButton
            v-else
            class="col-9"
            color="primary"
            icon="i-lucide-plus"
            variant="ghost"
            @click="$emit('addToRanked', angebot.definitionId)"
        />
    </li>
</template>

<style scoped></style>
