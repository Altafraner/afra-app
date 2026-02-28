<script setup>
import { ref } from 'vue';
import { Button, Dialog, Tag, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Lehrer' } },
];

const dialogVisible = ref(false);
const selectedAntrag = ref(null);
const kommentar = ref('');
const pendingStatus = ref(null);
const submitting = ref(false);

await store.updatePendingAntraege();

function formatDate(dateStr) {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
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
        store.pendingAntraege = null;
        await store.updatePendingAntraege();
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
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Lehrkraft)</h1>

    <p v-if="!store.pendingAntraege?.length" class="mt-4">
        Aktuell liegen keine ausstehenden Freistellungsanträge für dich vor.
    </p>

    <div v-else class="flex flex-col gap-4 mt-4">
        <div
            v-for="antrag in store.pendingAntraege"
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
                <Tag severity="info" :value="formatDate(antrag.datum)" />
            </div>

            <p class="text-sm mb-3">
                <span class="font-semibold">Grund:</span> {{ antrag.grund }}
            </p>

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
                für den
                <strong>{{ selectedAntrag ? formatDate(selectedAntrag.datum) : '' }}</strong>
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
