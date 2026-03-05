<script setup>
import { computed, ref } from 'vue';
import { Button, Tag, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag (Schulleiter)', route: { name: 'Freistellung-Schulleiter' } },
];

const approving = ref(null);

await store.updateSchulleiterAntraege();

// Split: pending = Bestaetigt (Sekretariat confirmed, Schulleiter hasn't acted yet)
const pendingAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status === 'Bestaetigt') ?? [],
);
const processedAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status !== 'Bestaetigt') ?? [],
);

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

const statusSeverity = {
    SchulleiterBestaetigt: 'success',
    Abgelehnt: 'danger',
};

const statusLabel = {
    SchulleiterBestaetigt: 'Genehmigt',
    Abgelehnt: 'Abgelehnt',
};

async function bestaetigen(antragId) {
    approving.value = antragId;
    const api = mande(`/api/freistellung/schulleiter/${antragId}/bestaetigen`);
    try {
        await api.put({});
        toast.add({
            severity: 'success',
            summary: 'Genehmigt',
            detail: 'Der Freistellungsantrag wurde endgültig genehmigt.',
            life: 3000,
        });
        store.schulleiterAntraege = null;
        await store.updateSchulleiterAntraege();
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        approving.value = null;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Schulleiter)</h1>

    <!-- Pending section -->
    <h2 class="text-lg font-semibold mt-4 mb-1">Warten auf abschließende Genehmigung</h2>
    <p class="mb-3 text-sm">
        Die folgenden Anträge wurden vom Sekretariat bestätigt und warten auf Ihre abschließende
        Genehmigung.
    </p>

    <p v-if="!pendingAntraege.length" class="mt-2">
        Aktuell liegen keine Freistellungsanträge zur Genehmigung vor.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in pendingAntraege"
            :key="antrag.id"
            class="border rounded-lg p-4 shadow-sm"
        >
            <div class="flex items-start justify-between gap-2 mb-2">
                <div>
                    <span class="font-semibold text-lg">{{ antrag.titel }}</span>
                    <span class="ml-2 text-base">
                        {{ antrag.student.nachname }}, {{ antrag.student.vorname }}
                    </span>
                    <span class="ml-1 text-sm text-gray-500">
                        {{ antrag.student.gruppe ?? '' }}
                    </span>
                </div>
                <div class="text-right text-sm whitespace-nowrap">
                    <Tag severity="warn" :value="formatDateRange(antrag.von, antrag.bis)" />
                    <div class="text-gray-500 mt-1">
                        {{ formatTime(antrag.von) }} – {{ formatTime(antrag.bis) }}
                    </div>
                </div>
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

            <Button
                label="Antrag genehmigen"
                icon="pi pi-check-circle"
                severity="success"
                size="small"
                :loading="approving === antrag.id"
                @click="bestaetigen(antrag.id)"
            />
        </div>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Es wurden noch keine Freistellungsanträge endgültig bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in processedAntraege"
            :key="antrag.id"
            class="border rounded-lg p-4 shadow-sm opacity-80"
        >
            <div class="flex items-start justify-between gap-2 mb-2">
                <div>
                    <span class="font-semibold text-lg">{{ antrag.titel }}</span>
                    <span class="ml-2 text-base">
                        {{ antrag.student.nachname }}, {{ antrag.student.vorname }}
                    </span>
                </div>
                <div class="flex items-center gap-2">
                    <Tag
                        :severity="statusSeverity[antrag.status]"
                        :value="statusLabel[antrag.status]"
                    />
                    <Tag
                        severity="secondary"
                        :value="formatDateRange(antrag.von, antrag.bis)"
                    />
                </div>
            </div>

            <p class="text-sm mb-2">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>
        </div>
    </div>
</template>

<style scoped></style>
