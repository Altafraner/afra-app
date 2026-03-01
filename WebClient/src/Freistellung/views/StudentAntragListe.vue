<script setup>
import { Button, Tag, useToast } from 'primevue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } },
];

await store.updateMeineAntraege();

const statusSeverity = {
    Gestellt: 'info',
    AlleLehrerGenehmigt: 'warn',
    Abgelehnt: 'danger',
    Bestaetigt: 'success',
};

const statusLabel = {
    Gestellt: 'Eingereicht',
    AlleLehrerGenehmigt: 'Alle Lehrkräfte genehmigt',
    Abgelehnt: 'Abgelehnt',
    Bestaetigt: 'Bestätigt',
};

const entscheidungSeverity = {
    Ausstehend: 'secondary',
    Genehmigt: 'success',
    Abgelehnt: 'danger',
};

const entscheidungLabel = {
    Ausstehend: 'Ausstehend',
    Genehmigt: 'Genehmigt',
    Abgelehnt: 'Abgelehnt',
};

function formatDate(dateStr) {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
}

function formatDateRange(von, bis) {
    return von === bis ? formatDate(von) : `${formatDate(von)} – ${formatDate(bis)}`;
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <div class="flex items-center justify-between">
        <h1>Meine Freistellungsanträge</h1>
        <Button
            label="Neuer Antrag"
            icon="pi pi-plus"
            :to="{ name: 'Freistellung-Neu' }"
            as="router-link"
            size="small"
        />
    </div>

    <p v-if="!store.meineAntraege?.length" class="mt-4">
        Du hast noch keine Freistellungsanträge gestellt.
    </p>

    <div v-else class="flex flex-col gap-4 mt-4">
        <div
            v-for="antrag in store.meineAntraege"
            :key="antrag.id"
            class="border rounded-lg p-4 shadow-sm"
        >
            <div class="flex items-start justify-between gap-2 mb-2">
                <div>
                    <span class="font-semibold text-lg">{{ formatDateRange(antrag.datumVon, antrag.datumBis) }}</span>
                    <Tag
                        class="ml-2"
                        :severity="statusSeverity[antrag.status]"
                        :value="statusLabel[antrag.status]"
                    />
                </div>
            </div>

            <p class="text-sm mb-3">{{ antrag.grund }}</p>

            <h4 class="font-semibold mb-1 text-sm">Entscheidungen der Lehrkräfte:</h4>
            <div class="flex flex-col gap-1">
                <div
                    v-for="e in antrag.entscheidungen"
                    :key="e.id"
                    class="flex items-center justify-between text-sm"
                >
                    <span>{{ e.lehrer.nachname }}, {{ e.lehrer.vorname }}</span>
                    <div class="flex items-center gap-2">
                        <Tag
                            :severity="entscheidungSeverity[e.status]"
                            :value="entscheidungLabel[e.status]"
                        />
                        <span v-if="e.kommentar" class="text-xs text-gray-500 italic">
                            „{{ e.kommentar }}"
                        </span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped></style>
