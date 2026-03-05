<script setup>
import { Button, Tag, useToast } from 'primevue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } }];

await store.updateMeineAntraege();

const statusSeverity = {
    Gestellt: 'info',
    AlleLehrerGenehmigt: 'warn',
    Abgelehnt: 'danger',
    Bestaetigt: 'warn',
    SchulleiterBestaetigt: 'success',
};

const statusLabel = {
    Gestellt: 'Eingereicht',
    AlleLehrerGenehmigt: 'Alle genehmigt',
    Abgelehnt: 'Abgelehnt',
    Bestaetigt: 'Sekretariat bestätigt',
    SchulleiterBestaetigt: 'Genehmigt',
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
    const d = new Date(dateStr);
    return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
}

function formatDateRange(von, bis) {
    const vonDate = new Date(von).toDateString();
    const bisDate = new Date(bis).toDateString();
    return vonDate === bisDate ? formatDate(von) : `${formatDate(von)} – ${formatDate(bis)}`;
}

function formatTime(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
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
            <div class="flex items-start justify-between gap-2 mb-1">
                <div>
                    <span class="font-semibold text-lg">{{ antrag.titel }}</span>
                    <Tag
                        class="ml-2"
                        :severity="statusSeverity[antrag.status]"
                        :value="statusLabel[antrag.status]"
                    />
                </div>
                <span class="text-sm text-gray-500 whitespace-nowrap">
                    {{ formatDateRange(antrag.von, antrag.bis) }}
                    {{ formatTime(antrag.von) }} – {{ formatTime(antrag.bis) }}
                </span>
            </div>

            <p class="text-sm mb-3">{{ antrag.grund }}</p>

            <h4 class="font-semibold mb-1 text-sm">Betroffene Stunden:</h4>
            <table v-if="antrag.betroffeneStunden?.length" class="w-full text-sm mb-3">
                <thead>
                    <tr class="text-left border-b text-gray-500">
                        <th class="py-1 pr-3 font-medium">Datum</th>
                        <th class="py-1 pr-3 font-medium">Block</th>
                        <th class="py-1 pr-3 font-medium">Fach</th>
                        <th class="py-1 font-medium">Lehrkraft</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="s in antrag.betroffeneStunden"
                        :key="s.id"
                        class="border-b last:border-0"
                    >
                        <td class="py-1 pr-3">{{ formatDate(s.datum) }}</td>
                        <td class="py-1 pr-3">{{ s.block }}</td>
                        <td class="py-1 pr-3">{{ s.fach }}</td>
                        <td class="py-1">{{ s.lehrer.nachname }}, {{ s.lehrer.vorname }}</td>
                    </tr>
                </tbody>
            </table>

            <h4 class="font-semibold mb-1 text-sm">Entscheidungen:</h4>
            <div class="flex flex-col gap-1 mb-2">
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
