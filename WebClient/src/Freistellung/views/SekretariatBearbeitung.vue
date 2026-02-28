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

await store.updateSekretariatAntraege();

function formatDate(dateStr) {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
}

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
        await store.updateSekretariatAntraege();
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
    <p>
        Die folgenden Anträge wurden von allen betroffenen Lehrkräften genehmigt und warten auf
        die abschließende Bestätigung durch das Sekretariat.
    </p>

    <p v-if="!store.sekretariatAntraege?.length" class="mt-4">
        Aktuell liegen keine zu bearbeitenden Freistellungsanträge vor.
    </p>

    <div v-else class="flex flex-col gap-4 mt-4">
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
                <Tag severity="warn" :value="formatDate(antrag.datum)" />
            </div>

            <p class="text-sm mb-2">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>

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
</template>

<style scoped></style>
