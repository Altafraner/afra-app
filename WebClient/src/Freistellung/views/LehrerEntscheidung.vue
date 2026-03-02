<script setup>
import { computed, ref } from 'vue';
import { Button, Dialog, Tag, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import { useUser } from '@/stores/user.ts';

const toast = useToast();
const store = useFreistellungStore();
const userStore = useUser();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Lehrer' } },
];

const dialogVisible = ref(false);
const selectedAntrag = ref(null);
const kommentar = ref('');
const pendingStatus = ref(null);
const submitting = ref(false);

await store.updateLehrerAntraege();

// Split client-side: pending = this teacher hasn't decided yet, processed = already decided
const pendingAntraege = computed(() => {
    if (!userStore.user) return [];
    return store.lehrerAntraege?.filter((a) => {
        const meineEntscheidung = a.entscheidungen.find((e) => e.lehrer.id === userStore.user.id);
        return meineEntscheidung?.status === 'Ausstehend';
    }) ?? [];
});
const processedAntraege = computed(() => {
    if (!userStore.user) return [];
    return store.lehrerAntraege?.filter((a) => {
        const meineEntscheidung = a.entscheidungen.find((e) => e.lehrer.id === userStore.user.id);
        return meineEntscheidung?.status !== 'Ausstehend';
    }) ?? [];
});

function formatDate(dateStr) {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
}

function formatDateRange(von, bis) {
    return von === bis ? formatDate(von) : `${formatDate(von)} – ${formatDate(bis)}`;
}

function openDialog(antrag, status) {
    selectedAntrag.value = antrag;
    pendingStatus.value = status;
    kommentar.value = '';
    dialogVisible.value = true;
}

async function submitDecision() {
    if (!selectedAntrag.value) return;
    submitting.value = true;

    const api = mande(`/api/freistellung/lehrer/${selectedAntrag.value.id}/entscheidung`);
    try {
        await api.put({
            status: pendingStatus.value,
            kommentar: kommentar.value.trim() || null,
        });
        toast.add({
            severity: pendingStatus.value === 'Genehmigt' ? 'success' : 'warn',
            summary:
                pendingStatus.value === 'Genehmigt' ? 'Genehmigt' : 'Abgelehnt',
            detail: `Der Freistellungsantrag wurde ${pendingStatus.value === 'Genehmigt' ? 'genehmigt' : 'abgelehnt'}.`,
            life: 3000,
        });
        dialogVisible.value = false;
        store.lehrerAntraege = null;
        await store.updateLehrerAntraege();
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        submitting.value = false;
    }
}

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
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Lehrkraft)</h1>

    <!-- Pending section -->
    <h2 class="text-lg font-semibold mt-4 mb-2">Ausstehende Anträge</h2>

    <p v-if="!pendingAntraege.length" class="mt-2">
        Aktuell liegen keine ausstehenden Freistellungsanträge für dich vor.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in pendingAntraege"
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
                <Tag severity="info" :value="formatDateRange(antrag.datumVon, antrag.datumBis)" />
            </div>

            <p class="text-sm mb-3">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>

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

            <div class="flex gap-2">
                <Button
                    label="Genehmigen"
                    icon="pi pi-check"
                    severity="success"
                    size="small"
                    @click="openDialog(antrag, 'Genehmigt')"
                />
                <Button
                    label="Ablehnen"
                    icon="pi pi-times"
                    severity="danger"
                    size="small"
                    @click="openDialog(antrag, 'Abgelehnt')"
                />
            </div>
        </div>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Du hast noch keine Freistellungsanträge bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <div
            v-for="antrag in processedAntraege"
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
                <Tag severity="secondary" :value="formatDateRange(antrag.datumVon, antrag.datumBis)" />
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
                    <Tag
                        :severity="entscheidungSeverity[e.status]"
                        :value="entscheidungLabel[e.status]"
                    />
                    <span>{{ e.lehrer.nachname }}, {{ e.lehrer.vorname }}</span>
                    <span v-if="e.kommentar" class="text-xs text-gray-500 italic">
                        „{{ e.kommentar }}"
                    </span>
                </div>
            </div>
        </div>
    </div>

    <Dialog
        v-model:visible="dialogVisible"
        modal
        :header="pendingStatus === 'Genehmigt' ? 'Antrag genehmigen' : 'Antrag ablehnen'"
        style="width: 30rem"
    >
        <div class="flex flex-col gap-3">
            <p>
                Möchtest du den Freistellungsantrag von
                <strong>
                    {{ selectedAntrag?.student?.nachname }},
                    {{ selectedAntrag?.student?.vorname }}
                </strong>
                für
                <strong>{{ selectedAntrag ? formatDateRange(selectedAntrag.datumVon, selectedAntrag.datumBis) : '' }}</strong>
                {{ pendingStatus === 'Genehmigt' ? 'genehmigen' : 'ablehnen' }}?
            </p>
            <div class="flex flex-col gap-1">
                <label for="kommentar">Kommentar (optional)</label>
                <Textarea
                    id="kommentar"
                    v-model="kommentar"
                    rows="3"
                    placeholder="Optionaler Kommentar..."
                    :max-length="500"
                    fluid
                />
            </div>
        </div>
        <template #footer>
            <Button
                label="Abbrechen"
                severity="secondary"
                @click="dialogVisible = false"
            />
            <Button
                :label="pendingStatus === 'Genehmigt' ? 'Genehmigen' : 'Ablehnen'"
                :severity="pendingStatus === 'Genehmigt' ? 'success' : 'danger'"
                :loading="submitting"
                @click="submitDecision"
            />
        </template>
    </Dialog>
</template>

<style scoped></style>
