<script setup>
import { computed, ref, watch } from 'vue';
import { Button, DatePicker, InputNumber, InputText, Select, Textarea, useToast } from 'primevue';
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

// datum is a [startDate, endDate] array when a range is selected
const datum = ref(null);
const grund = ref('');
const stunden = ref([]); // list of { datum, block, fach, lehrerId }
const loading = ref(false);

await store.updateLehrer();

const lehrerOptions = store.lehrer?.map((l) => ({
    label: `${l.nachname}, ${l.vorname}`,
    value: l.id,
})) ?? [];

// The list of days within the selected range
const tage = computed(() => {
    if (!Array.isArray(datum.value) || datum.value[0] == null) return [];
    const von = datum.value[0];
    const bis = datum.value[1] ?? von;
    const days = [];
    const cur = new Date(von);
    while (cur <= bis) {
        days.push(new Date(cur));
        cur.setDate(cur.getDate() + 1);
    }
    return days;
});

// Day options for the stunden table
const tagOptions = computed(() =>
    tage.value.map((d) => ({
        label: formatDateJs(d),
        value: toDateStr(d),
    }))
);

// When date range changes, remove stunden rows whose date is no longer in range
watch(tage, (newTage) => {
    const validDates = new Set(newTage.map((d) => toDateStr(d)));
    stunden.value = stunden.value.filter((s) => validDates.has(s.datum));
});

const datumValid = computed(() => Array.isArray(datum.value) && datum.value[0] != null);
const stundenValid = computed(() =>
    stunden.value.length > 0 &&
    stunden.value.every((s) => s.datum && s.block > 0 && s.fach.trim() && s.lehrerId)
);

function toDateStr(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function formatDateJs(d) {
    return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
}

function addStunde() {
    const defaultDatum = tagOptions.value[0]?.value ?? '';
    stunden.value.push({ datum: defaultDatum, block: 1, fach: '', lehrerId: null });
}

function removeStunde(index) {
    stunden.value.splice(index, 1);
}

async function submit() {
    if (!datumValid.value || !grund.value.trim() || !stundenValid.value) {
        toast.add({
            severity: 'warn',
            summary: 'Fehlende Angaben',
            detail: 'Bitte alle Felder ausfüllen und mindestens eine betroffene Stunde angeben.',
            life: 4000,
        });
        return;
    }

    loading.value = true;
    const api = mande('/api/freistellung/sus');

    const [von, bis] = datum.value;
    const datumVonStr = toDateStr(von);
    const datumBisStr = bis ? toDateStr(bis) : datumVonStr;

    try {
        await api.post({
            datumVon: datumVonStr,
            datumBis: datumBisStr,
            grund: grund.value.trim(),
            stunden: stunden.value.map((s) => ({
                datum: s.datum,
                block: s.block,
                fach: s.fach.trim(),
                lehrerId: s.lehrerId,
            })),
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
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
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
        Wähle den Zeitraum und gib für jede betroffene Unterrichtsstunde das Datum, den Block,
        das Fach und die Lehrkraft an.
    </p>

    <div class="flex flex-col gap-6 mt-4" style="max-width: 50rem">
        <!-- Date range -->
        <div class="flex flex-col gap-1">
            <label for="datum">Zeitraum der Freistellung</label>
            <DatePicker
                id="datum"
                v-model="datum"
                selection-mode="range"
                date-format="dd.mm.yy"
                show-icon
                fluid
                :manual-input="false"
            />
        </div>

        <!-- Reason -->
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

        <!-- Betroffene Stunden table -->
        <div class="flex flex-col gap-2">
            <label class="font-semibold">Betroffene Unterrichtsstunden</label>
            <p v-if="!datumValid" class="text-sm text-gray-500">
                Bitte zuerst den Zeitraum auswählen.
            </p>

            <template v-else>
                <div v-if="stunden.length > 0" class="overflow-x-auto">
                    <table class="w-full text-sm border-collapse">
                        <thead>
                            <tr class="text-left border-b">
                                <th class="py-1 pr-3">Datum</th>
                                <th class="py-1 pr-3">Block</th>
                                <th class="py-1 pr-3">Fach</th>
                                <th class="py-1 pr-3">Lehrkraft</th>
                                <th class="py-1"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(stunde, index) in stunden" :key="index" class="border-b last:border-0">
                                <td class="py-2 pr-3" style="min-width: 9rem">
                                    <Select
                                        v-model="stunde.datum"
                                        :options="tagOptions"
                                        option-label="label"
                                        option-value="value"
                                        fluid
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 6rem">
                                    <InputNumber
                                        v-model="stunde.block"
                                        :min="1"
                                        :max="12"
                                        show-buttons
                                        button-layout="horizontal"
                                        fluid
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 10rem">
                                    <InputText
                                        v-model="stunde.fach"
                                        placeholder="Fach"
                                        :max-length="200"
                                        fluid
                                    />
                                </td>
                                <td class="py-2 pr-3" style="min-width: 12rem">
                                    <Select
                                        v-model="stunde.lehrerId"
                                        :options="lehrerOptions"
                                        option-label="label"
                                        option-value="value"
                                        placeholder="Lehrkraft…"
                                        filter
                                        fluid
                                    />
                                </td>
                                <td class="py-2">
                                    <Button
                                        icon="pi pi-trash"
                                        severity="danger"
                                        text
                                        rounded
                                        size="small"
                                        @click="removeStunde(index)"
                                    />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <p v-else class="text-sm text-gray-500">
                    Noch keine Stunden hinzugefügt.
                </p>

                <div>
                    <Button
                        label="Stunde hinzufügen"
                        icon="pi pi-plus"
                        severity="secondary"
                        size="small"
                        @click="addStunde"
                    />
                </div>
            </template>
        </div>

        <Button
            label="Antrag einreichen"
            icon="pi pi-send"
            :loading="loading"
            :disabled="!datumValid || !grund.trim() || !stundenValid"
            @click="submit"
        />
    </div>
</template>

<style scoped></style>

