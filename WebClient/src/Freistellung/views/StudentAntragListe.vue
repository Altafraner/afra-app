<script setup>
import { ref } from 'vue';
import { Button, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import { statusSeverity, statusLabel } from '@/Freistellung/helpers/formatters.js';

const store = useFreistellungStore();
const toast = useToast();
const resubmitting = ref(null);

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } }];

await store.updateMeineAntraege();

function canResubmit(antrag) {
    return antrag.status === 'SekretariatAbgelehnt' || antrag.status === 'SchulleiterAbgelehnt';
}

async function erneutEinreichen(antragId) {
    resubmitting.value = antragId;
    const api = mande(`/api/freistellung/sus/${antragId}/erneut-einreichen`);
    try {
        await api.put({});
        toast.add({
            severity: 'success',
            summary: 'Erneut eingereicht',
            detail: 'Der Freistellungsantrag wurde erneut zur Prüfung eingereicht.',
            life: 3000,
        });
        store.meineAntraege = null;
        await store.updateMeineAntraege();
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        resubmitting.value = null;
    }
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
        <FreistellungsantragCard
            v-for="antrag in store.meineAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :statusTag="{
                severity: statusSeverity[antrag.status],
                value: statusLabel[antrag.status],
            }"
        >
            <Button
                v-if="canResubmit(antrag)"
                label="Erneut einreichen"
                icon="pi pi-refresh"
                severity="warn"
                size="small"
                :loading="resubmitting === antrag.id"
                @click="erneutEinreichen(antrag.id)"
            />
        </FreistellungsantragCard>
    </div>
</template>

<style scoped></style>
