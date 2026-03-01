<script setup>
import { ref } from 'vue';
import { Button, Tag, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Sekretariat' } },
];

const confirming = ref(null);

await Promise.all([
    store.updateSekretariatAntraege(),
    store.updateProcessedSekretariatAntraege(),
]);

function formatDate(dateStr) {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
}

function formatDateRange(von, bis) {
    return von === bis ? formatDate(von) : `${formatDate(von)} – ${formatDate(bis)}`;
}

const statusSeverity = {
    Bestaetigt: 'success',
    Abgelehnt: 'danger',
};

const statusLabel = {
    Bestaetigt: 'Bestätigt',
    Abgelehnt: 'Abgelehnt',
};

async function bestaetigen(antragId) {
    confirming.value = antragId;
    const api = mande(`/api/freistellung/sekretariat/${antragId}/bestaetigen`);
    try {
        await api.put({});
        toast.add({
            severity: 'success',
            summary: 'Bestätigt',
            detail: 'Der Freistellungsantrag wurde vom Sekretariat bestätigt.',
            life: 3000,
        });
        store.sekretariatAntraege = null;
        store.processedSekretariatAntraege = null;
        await Promise.all([
            store.updateSekretariatAntraege(),
            store.updateProcessedSekretariatAntraege(),
        ]);
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        confirming.value = null;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Sekretariat)</h1>

    <!-- Pending confirmation section -->
    <h2 class="text-lg font-semibold mt-4 mb-1">Warten auf Bestätigung</h2>
    <p class="mb-3 text-sm">
        Die folgenden Anträge wurden von allen betroffenen Lehrkräften genehmigt und warten auf
        die abschließende Bestätigung durch das Sekretariat.
    </p>

    <p v-if="!store.sekretariatAntraege?.length" class="mt-2">
        Aktuell liegen keine zu bearbeitenden Freistellungsanträge vor.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in store.sekretariatAntraege"
            :key="antrag.id"
            class="border rounded-lg p-4 shadow-sm"
        >
            <div class="flex items-start justify-between gap-2 mb-2">
                <div>
                    <span class="font-semibold text-lg">
                        {{ antrag.student.nachname }}, {{ antrag.student.vorname }}
                    </span>
                    <span class="ml-2 text-sm text-gray-500">
                        {{ antrag.student.gruppe ?? '' }}
                    </span>
                </div>
                <Tag severity="warn" :value="formatDateRange(antrag.datumVon, antrag.datumBis)" />
            </div>

            <p class="text-sm mb-2">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>

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

            <h4 class="font-semibold mb-1 text-sm">Lehrkraft-Entscheidungen:</h4>
            <div class="flex flex-col gap-1 mb-3">
                <div
                    v-for="e in antrag.entscheidungen"
                    :key="e.id"
                    class="flex items-center gap-2 text-sm"
                >
                    <Tag severity="success" value="Genehmigt" />
                    <span>{{ e.lehrer.nachname }}, {{ e.lehrer.vorname }}</span>
                    <span v-if="e.kommentar" class="text-xs text-gray-500 italic">
                        „{{ e.kommentar }}"
                    </span>
                </div>
            </div>

            <Button
                label="Antrag bestätigen"
                icon="pi pi-check-circle"
                severity="success"
                size="small"
                :loading="confirming === antrag.id"
                @click="bestaetigen(antrag.id)"
            />
        </div>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!store.processedSekretariatAntraege?.length" class="mt-2">
        Es wurden noch keine Freistellungsanträge abschließend bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in store.processedSekretariatAntraege"
            :key="antrag.id"
            class="border rounded-lg p-4 shadow-sm opacity-80"
        >
            <div class="flex items-start justify-between gap-2 mb-2">
                <div>
                    <span class="font-semibold text-lg">
                        {{ antrag.student.nachname }}, {{ antrag.student.vorname }}
                    </span>
                    <span class="ml-2 text-sm text-gray-500">
                        {{ antrag.student.gruppe ?? '' }}
                    </span>
                </div>
                <div class="flex items-center gap-2">
                    <Tag
                        :severity="statusSeverity[antrag.status]"
                        :value="statusLabel[antrag.status]"
                    />
                    <Tag severity="secondary" :value="formatDateRange(antrag.datumVon, antrag.datumBis)" />
                </div>
            </div>

            <p class="text-sm mb-2">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>

            <table v-if="antrag.betroffeneStunden?.length" class="w-full text-sm mb-2">
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

            <div class="flex flex-col gap-1">
                <div
                    v-for="e in antrag.entscheidungen"
                    :key="e.id"
                    class="flex items-center gap-2 text-sm"
                >
                    <Tag severity="success" value="Genehmigt" />
                    <span>{{ e.lehrer.nachname }}, {{ e.lehrer.vorname }}</span>
                    <span v-if="e.kommentar" class="text-xs text-gray-500 italic">
                        „{{ e.kommentar }}"
                    </span>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped></style>
