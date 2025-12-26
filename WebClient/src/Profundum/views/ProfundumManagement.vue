<script setup>
import { ref } from 'vue';
import { mande } from 'mande';
import {
    Button,
    Column,
    DataTable,
    Dialog,
    Skeleton,
    InputText,
    Textarea,
    Dropdown,
    useToast,
} from 'primevue';

const toast = useToast();

const loading = ref(true);
const createDialogOpen = ref(false);
const createModel = ref({
    bezeichnung: '',
    beschreibung: '',
    kategorieId: null,
    minKlasse: null,
    maxKlasse: null,
});
const profunda = ref([]);
const categories = ref([]);

async function createProfundum() {
    const api = mande('/api/profundum/management/profundum');
    try {
        const id = await api.post({
            ...createModel.value,
            bezeichnung: createModel.value.bezeichnung.trim(),
            beschreibung: createModel.value.beschreibung?.trim(),
        });
        toast.add({ severity: 'success', summary: 'Profundum angelegt' });

        createDialogOpen.value = false;
        await getProfunda();

        createModel.value = {
            bezeichnung: '',
            beschreibung: '',
            kategorieId: null,
            minKlasse: null,
            maxKlasse: null,
        };
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Profundum nicht erstellen',
        });
    }
}

async function deleteProfundum(id, bezeichnung) {
    if (!confirm(`Möchten Sie das Profundum ${bezeichnung} wirklich löschen?`)) {
        return;
    }

    const api = mande('/api/profundum/management/profundum');
    try {
        await api.delete(`/${id}`);
        toast.add({
            severity: 'success',
            summary: 'Gelöscht',
            detail: 'Profundum wurde entfernt',
        });

        await getProfunda();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Profundum nicht löschen',
        });
    }
}

async function getProfunda() {
    const getter = mande('/api/profundum/management/profundum');
    profunda.value = await getter.get();
}

async function getKategorien() {
    const getter = mande('/api/profundum/management/kategorie');
    categories.value = await getter.get();
}

async function setup() {
    await Promise.all([getProfunda(), getKategorien()]);
    loading.value = false;
}

setup();
</script>

<template>
    <template v-if="!loading">
        <h2>Alle Profunda</h2>
        <p>Klicken sie auf ein Profundum, um Details zu sehen oder es zu bearbeiten.</p>

        <DataTable :value="profunda" data-key="id">
            <Column header="Bezeichnung">
                <template #body="{ data }">
                    <Button
                        :label="data.bezeichnung"
                        variant="text"
                        as="RouterLink"
                        :to="{ name: 'Profundum-Edit', params: { profundumId: data.id } }"
                    />
                </template>
            </Column>

            <Column class="text-right afra-col-action">
                <template #header>
                    <Button
                        v-tooltip="'Neues Profundum'"
                        icon="pi pi-plus"
                        aria-label="Neues Profundum"
                        @click="createDialogOpen = true"
                    />
                </template>

                <template #body="{ data }">
                    <Button
                        v-tooltip.left="'Löschen'"
                        icon="pi pi-times"
                        severity="danger"
                        variant="text"
                        aria-label="Löschen"
                        @click="deleteProfundum(data.id, data.bezeichnung)"
                    />
                </template>
            </Column>

            <template #empty>
                <div class="flex justify-center">Es sind keine Profunda angelegt.</div>
            </template>
        </DataTable>
    </template>
    <template v-else>
        <Skeleton class="mb-6" height="3rem" />
        <Skeleton class="mb-4" />
        <DataTable :value="new Array(10)">
            <Column v-for="_ in new Array(3)">
                <template #body>
                    <Skeleton />
                </template>
                <template #header>
                    <Skeleton height="1.5em" />
                </template>
            </Column>
        </DataTable>
    </template>

    <Dialog
        v-model:visible="createDialogOpen"
        header="Neues Profundum anlegen"
        :modal="true"
        style="width: 40rem"
    >
        <div class="flex flex-col gap-4">
            <div class="field">
                <label>Bezeichnung*</label>
                <InputText
                    v-model="createModel.bezeichnung"
                    maxlength="80"
                    class="w-full"
                    required
                />
            </div>

            <div class="field">
                <label>Kategorie*</label>
                <Dropdown
                    v-model="createModel.kategorieId"
                    :options="categories"
                    optionLabel="bezeichnung"
                    optionValue="id"
                    placeholder="Kategorie auswählen"
                    class="w-full"
                />
            </div>

            <div class="field">
                <label>Beschreibung</label>
                <Textarea
                    v-model="createModel.beschreibung"
                    rows="4"
                    class="w-full"
                    maxlength="1000"
                    autoResize
                />
            </div>

            <div class="field">
                <label>Jahrgänge</label>
                <div class="flex gap-2 items-center">
                    <InputText
                        type="number"
                        v-model="createModel.minKlasse"
                        placeholder="min"
                        class="w-20"
                    />
                    –
                    <InputText
                        type="number"
                        v-model="createModel.maxKlasse"
                        placeholder="max"
                        class="w-20"
                    />
                </div>
            </div>
        </div>

        <template #footer>
            <Button label="Abbrechen" severity="secondary" @click="createDialogOpen = false" />
            <Button
                label="Anlegen"
                icon="pi pi-check"
                @click="createProfundum"
                :disabled="!createModel.bezeichnung || !createModel.kategorieId"
            />
        </template>
    </Dialog>
</template>

<style scoped></style>
