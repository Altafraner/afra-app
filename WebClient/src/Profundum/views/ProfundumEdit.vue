<script setup>
import { InputText, MultiSelect, Select, Textarea, useToast } from 'primevue';
import { mande } from 'mande';
import { computed, onMounted, ref } from 'vue';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import ProfundumInstanzen from '@/Profundum/components/ProfundumInstanzen.vue';
import KlassenrangeSelector from '@/components/KlassenRangeSelector.vue';
import { convertMarkdownToHtml } from '@/composables/markdown';
import { useManagement } from '@/Profundum/composables/verwaltung.ts';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const props = defineProps({ profundumId: String });
const toast = useToast();
const verwaltung = useManagement();

const loading = ref(true);

const klassenstufen = ref([]);
const klassenStufenSelects = computed(() => [
    { label: '–', value: null },
    ...klassenstufen.value.map((x) => ({ label: x.toString(), value: x })),
]);

const fachbereiche = ref([]);
async function loadFachbereiche() {
    fachbereiche.value = await verwaltung.getFachbereiche();
}

async function getKlassen() {
    const getter = mande('/api/klassen');
    klassenstufen.value = await getter.get();
}

const categories = ref([]);
const profundum = ref(null);
const profundaList = ref([]);

const apiProfunda = mande('/api/profundum/management/profundum');
const apiKategorie = mande('/api/profundum/management/kategorie');

const navItems = computed(() => [
    {
        label: 'Profundum',
    },
    {
        label: 'Verwaltung',
        route: {
            name: 'Profundum-Verwaltung',
        },
    },
    {
        label: profundum.value?.bezeichnung ?? 'Definition',
    },
]);

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
    }
}

async function loadProfundaList() {
    profundaList.value = await apiProfunda.get();
}

async function setup() {
    try {
        await Promise.all([
            loadProfundum(),
            loadCategories(),
            loadProfundaList(),
            getKlassen(),
            loadFachbereiche(),
        ]);
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
const updateKlassen = () =>
    savePatch({
        minKlasse: profundum.value.minKlasse ?? null,
        maxKlasse: profundum.value.maxKlasse ?? null,
    });
const updateDependencies = () => savePatch({ dependencyIds: profundum.value.dependencyIds });
const updateFachbereiche = () =>
    savePatch({
        fachbereichIds: profundum.value.fachbereichIds,
    });
</script>
<template>
    <template v-if="loading">Lade...</template>
    <template v-else>
        <NavBreadcrumb :items="navItems" />
        <h1>{{ profundum.bezeichnung }}</h1>
        <h2>Stammdaten</h2>

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
                    <Select
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
                    <div
                        class="m-trim"
                        v-html="convertMarkdownToHtml(profundum.beschreibung)"
                    />
                </template>
                <template #edit>
                    <Textarea
                        v-model="profundum.beschreibung"
                        auto-resize
                        fluid
                        rows="3"
                        maxlength="2000"
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

            <GridEditRow header="Voraussetzungen" @update="updateDependencies">
                <template
                    #body
                    v-if="profundum.dependencyIds && profundum.dependencyIds.length > 0"
                >
                    {{
                        profundum.dependencyIds
                            .map(
                                (id) =>
                                    profundaList.find((p) => p.id === id)?.bezeichnung ?? '??',
                            )
                            .join(', ')
                    }}
                </template>
                <template #body v-else> Keine Voraussetzungen </template>
                <template #edit>
                    <MultiSelect
                        v-model="profundum.dependencyIds"
                        :options="profundaList"
                        optionLabel="bezeichnung"
                        optionValue="id"
                        display="chip"
                        filter
                        filterPlaceholder="Suchen..."
                        class="w-full multiselect-wrap"
                        appendTo="self"
                        placeholder="Voraussetzungen auswählen"
                    />
                </template>
            </GridEditRow>
            <GridEditRow header="Fachbereiche" @update="updateFachbereiche">
                <template
                    #body
                    v-if="profundum.fachbereichIds && profundum.fachbereichIds.length > 0"
                >
                    {{ profundum.fachbereiche.map((fb) => fb.label).join(', ') }}
                </template>
                <template #body v-else> Keinen Fachbereichen zugeordnet </template>
                <template #edit>
                    <MultiSelect
                        v-model="profundum.fachbereichIds"
                        :options="fachbereiche"
                        optionLabel="label"
                        optionValue="id"
                        display="chip"
                        filter
                        filterPlaceholder="Suchen..."
                        class="w-full multiselect-wrap"
                        appendTo="self"
                    />
                </template>
            </GridEditRow>
        </Grid>

        <ProfundumInstanzen :profundumId="props.profundumId" />
    </template>
</template>

<style scoped>
.multiselect-wrap :deep(.p-multiselect-label-container) {
    height: auto;
}

.multiselect-wrap :deep(.p-multiselect-label) {
    display: flex;
    flex-wrap: wrap;
    white-space: normal;
    gap: 0.25rem;
    padding-top: 0.25rem;
    padding-bottom: 0.25rem;
}

.multiselect-wrap :deep(.p-multiselect-token) {
    margin-bottom: 0.25rem;
}
</style>
