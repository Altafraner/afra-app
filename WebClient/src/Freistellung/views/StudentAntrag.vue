<script setup>
import { ref } from 'vue';
import { Button, DatePicker, InputText, MultiSelect, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';

const toast = useToast();
const router = useRouter();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } },
    { label: 'Neuer Antrag', route: { name: 'Freistellung-Neu' } },
];

const datum = ref(null);
const grund = ref('');
const selectedLehrer = ref([]);
const loading = ref(false);

await store.updateLehrer();

const lehrerOptions = store.lehrer?.map((l) => ({
    label: `${l.nachname}, ${l.vorname}`,
    value: l.id,
})) ?? [];

async function submit() {
    if (!datum.value || !grund.value.trim() || selectedLehrer.value.length === 0) {
        toast.add({
            severity: 'warn',
            summary: 'Fehlende Angaben',
            detail: 'Bitte alle Felder ausfüllen und mindestens eine Lehrkraft auswählen.',
            life: 4000,
        });
        return;
    }

    loading.value = true;
    const api = mande('/api/freistellung/sus');

    const d = datum.value;
    const datumStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    try {
        await api.post({
            datum: datumStr,
            grund: grund.value.trim(),
            lehrerIds: selectedLehrer.value,
        });
        store.meineAntraege = null;
        toast.add({
            severity: 'success',
            summary: 'Antrag gestellt',
            detail: 'Dein Freistellungsantrag wurde erfolgreich eingereicht.',
            life: 3000,
        });
        await router.push({ name: 'Freistellung-Meine' });
    } catch (e) {
        const errorMessage =
            e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsantrag stellen</h1>
    <p>
        Hier kannst du einen Freistellungsantrag (Antrag auf Unterrichtsbefreiung) einreichen.
        Bitte gib das Datum, den Grund sowie alle Lehrkräfte an, deren Unterricht du an diesem
        Tag besuchst.
    </p>

    <div class="flex flex-col gap-4 mt-4" style="max-width: 40rem">
        <div class="flex flex-col gap-1">
            <label for="datum">Datum der Freistellung</label>
            <DatePicker
                id="datum"
                v-model="datum"
                date-format="dd.mm.yy"
                show-icon
                fluid
            />
        </div>

        <div class="flex flex-col gap-1">
            <label for="grund">Grund der Freistellung</label>
            <Textarea
                id="grund"
                v-model="grund"
                rows="4"
                placeholder="Bitte beschreibe den Grund deines Freistellungsantrags..."
                :max-length="1000"
                fluid
            />
        </div>

        <div class="flex flex-col gap-1">
            <label for="lehrer">Betroffene Lehrkräfte</label>
            <MultiSelect
                id="lehrer"
                v-model="selectedLehrer"
                :options="lehrerOptions"
                option-label="label"
                option-value="value"
                placeholder="Lehrkräfte auswählen..."
                filter
                fluid
            />
        </div>

        <Button
            label="Antrag einreichen"
            icon="pi pi-send"
            :loading="loading"
            :disabled="!datum || !grund.trim() || selectedLehrer.length === 0"
            @click="submit"
        />
    </div>
</template>

<style scoped></style>
