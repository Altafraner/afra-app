<script lang="ts" setup>
import FreistellungsListe from '@/Freistellung/components/FreistellungsListe.vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';

export interface FreistellungsBoardSection {
    /** Unique key, also used as the slot name for this section's per-card actions. */
    key: string;
    title: string;
    description?: string;
    antraege: Freistellungsantrag[];
    emptyText: string;
    showStudent?: boolean;
    showStunden?: boolean;
    showEntscheidungen?: boolean;
    muted?: boolean;
    showStatus?: boolean;
    dateTagColor?: string | null;
}

defineProps<{ sections: FreistellungsBoardSection[] }>();
</script>

<template>
    <template v-for="(section, index) in sections" :key="section.key">
        <h2 class="text-lg font-semibold mb-1" :class="index === 0 ? 'mt-4' : 'mt-8'">
            {{ section.title }}
        </h2>
        <p v-if="section.description" class="mb-3 text-sm text-muted">
            {{ section.description }}
        </p>
        <FreistellungsListe
            :antraege="section.antraege"
            :empty-text="section.emptyText"
            :show-student="section.showStudent ?? false"
            :show-stunden="section.showStunden ?? true"
            :show-entscheidungen="section.showEntscheidungen ?? true"
            :muted="section.muted ?? false"
            :show-status="section.showStatus ?? false"
            :date-tag-color="section.dateTagColor ?? null"
        >
            <template #default="{ antrag }">
                <slot :name="section.key" :antrag="antrag" />
            </template>
        </FreistellungsListe>
    </template>
</template>
