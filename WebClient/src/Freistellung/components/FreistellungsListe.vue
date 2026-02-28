<script lang="ts" setup>
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';

withDefaults(
    defineProps<{
        /** The Freistellungsanträge to render. */
        antraege: Freistellungsantrag[];
        /** Shown instead of the list when it is empty. */
        emptyText: string;
        /** Forwarded to FreistellungsantragCard for every item. */
        showStudent?: boolean;
        showStunden?: boolean;
        showEntscheidungen?: boolean;
        muted?: boolean;
        showStatus?: boolean;
        dateTagColor?: string | null;
    }>(),
    {
        showStudent: false,
        showStunden: true,
        showEntscheidungen: true,
        muted: false,
        showStatus: false,
        dateTagColor: null,
    },
);
</script>

<template>
    <p v-if="!antraege.length" class="mt-2 text-muted">{{ emptyText }}</p>
    <div v-else class="flex flex-col gap-4">
        <FreistellungsantragCard
            v-for="antrag in antraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="showStudent"
            :showStunden="showStunden"
            :showEntscheidungen="showEntscheidungen"
            :muted="muted"
            :showStatus="showStatus"
            :dateTagColor="dateTagColor"
        >
            <slot :antrag="antrag" />
        </FreistellungsantragCard>
    </div>
</template>
