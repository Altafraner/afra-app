<script setup>
import { computed, ref, watch } from 'vue';
import {
    Button,
    DatePicker,
    InputNumber,
    InputText,
    Select,
    Textarea,
    useToast,
} from 'primevue';
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

const vonDateTime = ref(null);
const bisDateTime = ref(null);
const grund = ref('');
const beschreibung = ref('');
const stunden = ref([]);
const loading = ref(false);

await store.updateLehrer();

const lehrerOptions =
    store.lehrer?.map((l) => ({
        label: `${l.nachname}, ${l.vorname}`,
        value: l.id,
    })) ?? [];

const tage = computed(() => {
    if (!vonDateTime.value) return [];
    const von = vonDateTime.value;
    const bis = bisDateTime.value ?? von;
    const days = [];
    const cur = new Date(von);
    cur.setHours(0, 0, 0, 0);
    const bisDay = new Date(bis);
    bisDay.setHours(23, 59, 59, 999);
    while (cur <= bisDay) {
        days.push(new Date(cur));
        cur.setDate(cur.getDate() + 1);
    }
    return days;
});

const tagOptions = computed(() =>
    tage.value.map((d) => ({
        label: formatDateJs(d),
        value: toDateStr(d),
    })),
);

watch(tage, (newTage) => {
    const validDates = new Set(newTage.map((d) => toDateStr(d)));
    stunden.value = stunden.value.filter((s) => validDates.has(s.datum));
});

const datumValid = computed(() => vonDateTime.value != null && bisDateTime.value != null);
const stundenValid = computed(
    () =>
        stunden.value.length > 0 &&
        stunden.value.every((s) => s.datum && s.block > 0 && s.fach.trim() && s.lehrerId),
);

function toDateStr(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function toTimeStr(d) {
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
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
    if (
        !datumValid.value ||
        !grund.value.trim() ||
        !beschreibung.value.trim() ||
        !stundenValid.value
    ) {
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

    function toDateTimeStr(date) {
        const d = toDateStr(date);
        const t = toTimeStr(date);
        return `${d}T${t}`;
    }

    try {
        await api.post({
            grund: grund.value.trim(),
            von: toDateTimeStr(vonDateTime.value),
            bis: toDateTimeStr(bisDateTime.value),
            beschreibung: beschreibung.value.trim(),
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
        <!-- Title -->
        <div class="flex flex-col gap-1">
            <label for="grund">Kurztitel</label>
            <InputText id="grund" v-model="grund" :max-length="200" fluid />
        </div>

        <div class="flex flex-col gap-1">
            <label for="vonDateTime">Beginn (Datum und Uhrzeit)</label>
            <DatePicker
                id="vonDateTime"
                v-model="vonDateTime"
                show-time
                hour-format="24"
                show-icon
                fluid
                :manual-input="false"
                date-format="dd.mm.yy"
            />
        </div>

        <div class="flex flex-col gap-1">
            <label for="bisDateTime">Ende (Datum und Uhrzeit)</label>
            <DatePicker
                id="bisDateTime"
                v-model="bisDateTime"
                show-time
                hour-format="24"
                show-icon
                fluid
                :manual-input="false"
                date-format="dd.mm.yy"
            />
        </div>

        <div class="flex flex-col gap-1">
            <label for="beschreibung">Grund der Freistellung</label>
            <Textarea
                id="beschreibung"
                v-model="beschreibung"
                rows="4"
                placeholder="Bitte beschreibe den Grund deines Freistellungsantrags..."
                :max-length="1000"
                fluid
            />
        </div>

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
                            <tr
                                v-for="(stunde, index) in stunden"
                                :key="index"
                                class="border-b last:border-0"
                            >
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

                <p v-else class="text-sm text-gray-500">Noch keine Stunden hinzugefügt.</p>

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
            :disabled="!datumValid || !grund.trim() || !beschreibung.trim() || !stundenValid"
            @click="submit"
        />
    </div>
</template>

<style scoped></style>
