<script setup>
import { InputText, Textarea, Button, useToast, Select } from 'primevue';
import Dropdown from 'primevue/dropdown';
import { formatStudent } from '@/helpers/formatters';

import { mande } from 'mande';
import { ref, onMounted, computed } from 'vue';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import ProfundumInstanzen from '@/Profundum/components/ProfundumInstanzen.vue';
import AfraPersonSelector from '@/Otium/components/Form/AfraPersonSelector.vue';
import KlassenrangeSelector from '@/components/KlassenRangeSelector.vue';
import { convertMarkdownToHtml } from '@/composables/markdown';

const props = defineProps({ profundumId: String });
const toast = useToast();

const loading = ref(true);

const klassenstufen = ref([]);
const klassenStufenSelects = computed(() => [
    { label: '–', value: null },
    ...klassenstufen.value.map((x) => ({ label: x.toString(), value: x })),
]);

async function getKlassen() {
    const getter = mande('/api/klassen');
    klassenstufen.value = await getter.get();
}

const categories = ref([]);
const profundum = ref(null);

const apiProfunda = mande('/api/profundum/management/profundum');
const apiKategorie = mande('/api/profundum/management/kategorie');

async function loadCategories() {
    categories.value = await apiKategorie.get();
}

async function loadProfundum() {
    profundum.value = await apiProfunda.get(props.profundumId);

    if (!profundum.value) {
        toast.add({
            severity: 'error',
            summary: 'Nicht gefunden',
            detail: 'Profundum existiert nicht',
        });
        return;
    }
}

async function setup() {
    try {
        await Promise.all([loadProfundum(), loadCategories(), getKlassen()]);
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Konnte Daten nicht laden.',
        });
    } finally {
        loading.value = false;
    }
}

onMounted(setup);

async function savePatch(patch) {
    try {
        await apiProfunda.put(`/${props.profundumId}`, {
            ...profundum.value,
            ...patch,
        });
        Object.assign(profundum.value, patch);
        toast.add({ severity: 'success', summary: 'Gespeichert' });
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte nicht speichern',
        });
    } finally {
        await loadProfundum();
    }
}

const updateTitel = () => savePatch({ bezeichnung: profundum.value.bezeichnung });
const updateKategorie = () => savePatch({ kategorieId: profundum.value.kategorieId });
const updateBeschreibung = () => savePatch({ beschreibung: profundum.value.beschreibung });
const updateVerantwortliche = () =>
    savePatch({ verantwortlicheIds: profundum.value.verantwortlicheIds });
const updateKlassen = () =>
    savePatch({
        minKlasse: profundum.value.minKlasse ?? null,
        maxKlasse: profundum.value.maxKlasse ?? null,
    });
</script>
<template>
    <template v-if="loading">Lade...</template>

    <template v-else>
        <h1>Profundum bearbeiten</h1>

        <Grid>
            <GridEditRow header="Titel" @update="updateTitel">
                <template #body>
                    <span>{{ profundum.bezeichnung }}</span>
                </template>
                <template #edit>
                    <InputText v-model="profundum.bezeichnung" fluid maxlength="80" />
                </template>
            </GridEditRow>

            <GridEditRow header="Kategorie" header-class="self-start" @update="updateKategorie">
                <template #body>
                    {{
                        categories.find((x) => x.id === profundum.kategorieId)?.bezeichnung ??
                        '–'
                    }}
                </template>
                <template #edit>
                    <Dropdown
                        v-model="profundum.kategorieId"
                        :options="categories"
                        optionLabel="bezeichnung"
                        optionValue="id"
                        placeholder="Kategorie auswählen"
                        class="w-full"
                        appendTo="self"
                    />
                </template>
            </GridEditRow>

            <GridEditRow
                header="Beschreibung"
                header-class="self-start"
                @update="updateBeschreibung"
            >
                <template #body>
            <div class="m-trim" v-html="convertMarkdownToHtml(profundum.beschreibung)" />
                </template>
                <template #edit>
                    <Textarea
                        v-model="profundum.beschreibung"
                        auto-resize
                        fluid
                        rows="3"
                        maxlength="1000"
                    />
                </template>
            </GridEditRow>

            <GridEditRow
                header="Verantwortliche"
                header-class="self-start"
                @update="updateVerantwortliche"
            >
                <template #body>
                    <template v-if="profundum.verantwortlicheInfo === []">
                        Kein:e Betreuer:in
                    </template>
                    <template v-else>
                        <template v-for="tutor in profundum.verantwortlicheInfo">
                            {{ formatStudent(tutor) }},
                        </template>
                    </template>
                </template>
                <template #edit>
                    <AfraPersonSelector
                        v-model="profundum.verantwortlicheIds"
                        :multi="true"
                        name="tutor"
                        required
                        class="multiselect-wrap"
                    />
                </template>
            </GridEditRow>

            <GridEditRow header="Jahrgänge" @update="updateKlassen">
                <template #body>
                    <span v-if="!profundum.minKlasse && !profundum.maxKlasse">Alle</span>
                    <span v-else-if="profundum.minKlasse === profundum.maxKlasse">
                        nur {{ profundum.minKlasse }}
                    </span>
                    <span v-else>
                        <span v-if="profundum.minKlasse"
                            >ab Klasse {{ profundum.minKlasse }}</span
                        >
                        <span v-if="profundum.maxKlasse">
                            bis Klasse {{ profundum.maxKlasse }}</span
                        >
                    </span>
                </template>

                <template #edit>
                    <KlassenrangeSelector
                        :min="profundum.minKlasse"
                        :max="profundum.maxKlasse"
                        :options="klassenStufenSelects"
                        @update:min="profundum.minKlasse = $event"
                        @update:max="profundum.maxKlasse = $event"
                    />
                </template>
            </GridEditRow>
        </Grid>

        <ProfundumInstanzen :profundumId="props.profundumId" />
    </template>
</template>
