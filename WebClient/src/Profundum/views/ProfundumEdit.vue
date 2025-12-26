<script setup>
import { InputText, Textarea, useToast } from 'primevue';
import Dropdown from 'primevue/dropdown';

import { mande } from 'mande';
import { ref, onMounted } from 'vue';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';

const props = defineProps({ profundumId: String });
const toast = useToast();

const loading = ref(true);
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
        await Promise.all([loadCategories(), loadProfundum()]);
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
                    <p v-for="line in (profundum.beschreibung ?? '').split('\n')" :key="line">
                        {{ line }}
                    </p>
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

            <GridEditRow header="Jahrgänge" @update="updateKlassen">
                <template #body>
                    <span v-if="!profundum.minKlasse && !profundum.maxKlasse">Alle</span>
                    <span v-else-if="profundum.minKlasse === profundum.maxKlasse">
                        nur {{ profundum.minKlasse }}
                    </span>
                    <span v-else>
                        <span v-if="profundum.minKlasse">ab {{ profundum.minKlasse }}</span>
                        <span v-if="profundum.maxKlasse"> bis {{ profundum.maxKlasse }}</span>
                    </span>
                </template>
                <template #edit>
                    <InputText type="number" v-model="profundum.minKlasse" placeholder="min" />
                    –
                    <InputText type="number" v-model="profundum.maxKlasse" placeholder="max" />
                </template>
            </GridEditRow>
        </Grid>
    </template>
</template>
