<script setup>
import { computed, ref } from 'vue';
import { Button, Dialog, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import { useUser } from '@/stores/user.ts';
import { formatStudent } from '@/helpers/formatters';
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import { formatFreistellungDateRange } from '@/Freistellung/helpers/formatters.js';

const toast = useToast();
const store = useFreistellungStore();
const userStore = useUser();

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Lehrer' } }];

const dialogVisible = ref(false);
const selectedAntrag = ref(null);
const kommentar = ref('');
const pendingStatus = ref(null);
const submitting = ref(false);

await store.updateLehrerAntraege();

// Split client-side: pending = this teacher hasn't decided yet, processed = already decided
const pendingAntraege = computed(() => {
    if (!userStore.user) return [];
    return (
        store.lehrerAntraege?.filter((a) => {
            const meineEntscheidung = a.entscheidungen.find(
                (e) => e.lehrer.id === userStore.user.id,
            );
            return meineEntscheidung?.status === 'Ausstehend';
        }) ?? []
    );
});
const processedAntraege = computed(() => {
    if (!userStore.user) return [];
    return (
        store.lehrerAntraege?.filter((a) => {
            const meineEntscheidung = a.entscheidungen.find(
                (e) => e.lehrer.id === userStore.user.id,
            );
            return meineEntscheidung?.status !== 'Ausstehend';
        }) ?? []
    );
});

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
            summary: pendingStatus.value === 'Genehmigt' ? 'Genehmigt' : 'Abgelehnt',
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
        <FreistellungsantragCard
            v-for="antrag in pendingAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            :showEntscheidungen="true"
        >
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
        </FreistellungsantragCard>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Du hast noch keine Freistellungsanträge bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <FreistellungsantragCard
            v-for="antrag in processedAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            :showStunden="true"
            :muted="true"
        />
    </div>

    <Dialog
        v-model:visible="dialogVisible"
        modal
        :header="pendingStatus === 'Genehmigt' ? 'Antrag genehmigen' : 'Antrag ablehnen'"
        style="width: 30rem"
    >
        <div class="flex flex-col gap-3">
            <p>
                Möchtest du den Freistellungsantrag
                <strong>„{{ selectedAntrag?.grund }}"</strong>
                von
                <strong> {{ formatStudent(selectedAntrag?.student) }} </strong>
                für
                <strong>{{
                    selectedAntrag
                        ? formatFreistellungDateRange(selectedAntrag.von, selectedAntrag.bis)
                        : ''
                }}</strong>
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
            <Button label="Abbrechen" severity="secondary" @click="dialogVisible = false" />
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
