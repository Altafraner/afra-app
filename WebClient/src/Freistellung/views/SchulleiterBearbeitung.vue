<script setup>
import { computed, ref } from 'vue';
import { Button, Dialog, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import { statusSeverity, statusLabel } from '@/Freistellung/helpers/formatters.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag (Schulleiter)', route: { name: 'Freistellung-Schulleiter' } },
];

const approving = ref(null);
const dialogVisible = ref(false);
const selectedAntrag = ref(null);
const kommentar = ref('');
const submitting = ref(false);

await store.updateSchulleiterAntraege();

// Split: pending = Bestaetigt or SchulleiterAbgelehnt (Sekretariat confirmed, Schulleiter hasn't finally approved yet)
const pendingAntraege = computed(
    () =>
        store.schulleiterAntraege?.filter(
            (a) => a.status === 'Bestaetigt' || a.status === 'SchulleiterAbgelehnt',
        ) ?? [],
);
const processedAntraege = computed(
    () =>
        store.schulleiterAntraege?.filter(
            (a) => a.status !== 'Bestaetigt' && a.status !== 'SchulleiterAbgelehnt',
        ) ?? [],
);

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

function openAblehnenDialog(antrag) {
    selectedAntrag.value = antrag;
    kommentar.value = '';
    dialogVisible.value = true;
}

async function submitAblehnung() {
    if (!selectedAntrag.value || !kommentar.value.trim()) return;
    submitting.value = true;

    const api = mande(`/api/freistellung/schulleiter/${selectedAntrag.value.id}/ablehnen`);
    try {
        await api.put({ kommentar: kommentar.value.trim() });
        toast.add({
            severity: 'warn',
            summary: 'Abgelehnt',
            detail: 'Der Freistellungsantrag wurde vom Schulleiter abgelehnt.',
            life: 3000,
        });
        dialogVisible.value = false;
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
        submitting.value = false;
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
        <FreistellungsantragCard
            v-for="antrag in pendingAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            dateTagSeverity="warn"
        >
            <div class="flex gap-2">
                <Button
                    label="Antrag genehmigen"
                    icon="pi pi-check-circle"
                    severity="success"
                    size="small"
                    :loading="approving === antrag.id"
                    @click="bestaetigen(antrag.id)"
                />
                <Button
                    label="Ablehnen"
                    icon="pi pi-times"
                    severity="danger"
                    size="small"
                    @click="openAblehnenDialog(antrag)"
                />
            </div>
        </FreistellungsantragCard>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Es wurden noch keine Freistellungsanträge endgültig bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <FreistellungsantragCard
            v-for="antrag in processedAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            :showStunden="false"
            :showEntscheidungen="false"
            :muted="true"
            dateTagSeverity="secondary"
            :statusTag="{
                severity: statusSeverity[antrag.status],
                value: statusLabel[antrag.status],
            }"
        />
    </div>

    <!-- Reject dialog -->
    <Dialog
        v-model:visible="dialogVisible"
        modal
        header="Antrag ablehnen"
        style="width: 30rem"
    >
        <div class="flex flex-col gap-3">
            <p>
                Bitte geben Sie einen Kommentar an, was am Antrag
                <strong>„{{ selectedAntrag?.grund }}"</strong> korrigiert werden soll.
            </p>
            <div class="flex flex-col gap-1">
                <label for="kommentar">Kommentar</label>
                <Textarea
                    id="kommentar"
                    v-model="kommentar"
                    rows="3"
                    placeholder="Was muss korrigiert werden?"
                    :max-length="500"
                    fluid
                />
            </div>
        </div>
        <template #footer>
            <Button label="Abbrechen" severity="secondary" @click="dialogVisible = false" />
            <Button
                label="Ablehnen"
                severity="danger"
                :loading="submitting"
                :disabled="!kommentar.trim()"
                @click="submitAblehnung"
            />
        </template>
    </Dialog>
</template>

<style scoped></style>
