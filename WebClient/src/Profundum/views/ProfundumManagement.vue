<script setup>
import { ref } from 'vue';
import { mande } from 'mande';
import {
    Button,
    Column,
    Message,
    DataTable,
    Dialog,
    InputText,
    Textarea,
    useToast,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    Select,
    MultiSelect,
} from 'primevue';
import { useConfirmPopover } from '@/composables/confirmPopover';

import EinwahlZeitraeume from '@/Profundum/components/EinwahlZeitraeume.vue';
import Slots from '@/Profundum/components/Slots.vue';
import { useManagement } from '@/Profundum/composables/verwaltung.ts';

const toast = useToast();
const confirm = useConfirmPopover();
const verwaltung = useManagement();

const currentTab = ref('0');
const createDialogOpen = ref(false);

const createModel = ref({
    bezeichnung: '',
    beschreibung: '',
    kategorieId: null,
    minKlasse: null,
    maxKlasse: null,
    fachbereichIds: [],
});

const profunda = ref([]);
const categories = ref([]);
const fachbereiche = ref([]);

async function createProfundum() {
    const api = mande('/api/profundum/management/profundum');
    try {
        await api.post({
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

function deleteProfundum(event, data) {
    confirm.openConfirmDialog(
        event,
        doDelete,
        'Profundum Löschen',
        'Das Löschen kann nicht rückgängig gemacht werden. Das Löschen von Profunda mit bereits hinterlegten Belegungen kann zu Problemen bei der nächsten Einwahl führen!',
        'danger',
    );

    async function doDelete() {
        const api = mande('/api/profundum/management/profundum');
        try {
            await api.delete(`/${data.id}`);
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
}

async function getProfunda() {
    const getter = mande('/api/profundum/management/profundum');
    profunda.value = await getter.get();
}

async function getKategorien() {
    const getter = mande('/api/profundum/management/kategorie');
    categories.value = await getter.get();
}

async function getFachbereiche() {
    fachbereiche.value = await verwaltung.getFachbereiche();
}

async function setup() {
    await Promise.all([getProfunda(), getKategorien(), getFachbereiche()]);
}

await setup();
</script>

<template>
    <h2>Profunda Verwaltung</h2>

    <Tabs class="mt-5" v-model:value="currentTab">
        <TabList>
            <Tab value="0">Profunda</Tab>
            <Tab value="1">Einwahlzeiträume</Tab>
            <Tab value="2">Slots</Tab>
        </TabList>
        <TabPanels>
            <TabPanel value="0">
                <Message severity="warn" class="mb-4">
                    Diese Funktionen sind nicht ausgiebig getestet und insbesondere die Löschung
                    von bereits genutzen Profunda kann erheblichen Datenverlust beispielsweise
                    für Matching und Feedback-Funktionen auslösen. Alte Profunda sollten
                    beispielsweise im neuen Semester einfach keine neue Instanz erhalten, statt
                    gelöscht zu werden.
                </Message>

                <DataTable :value="profunda" data-key="id">
                    <Column header="Bezeichnung">
                        <template #body="{ data }">
                            <Button
                                :label="data.bezeichnung"
                                variant="text"
                                as="RouterLink"
                                :to="{
                                    name: 'Profundum-Edit',
                                    params: { profundumId: data.id },
                                }"
                            />
                        </template>
                    </Column>

                    <Column class="text-right afra-col-action">
                        <template #header>
                            <Button
                                v-tooltip.left="'Neues Profundum'"
                                icon="pi pi-plus"
                                aria-label="Neues Profundum"
                                @click="createDialogOpen = true"
                            />
                        </template>

                        <template #body="{ data }">
                            <Button
                                v-tooltip.left="'Löschen'"
                                icon="pi pi-trash"
                                severity="danger"
                                variant="text"
                                aria-label="Löschen"
                                @click="deleteProfundum($event, data)"
                            />
                        </template>
                    </Column>

                    <template #empty>
                        <div class="flex justify-center">Es sind keine Profunda angelegt.</div>
                    </template>
                </DataTable>
            </TabPanel>

            <TabPanel value="1">
                <EinwahlZeitraeume />
            </TabPanel>

            <TabPanel value="2">
                <Slots />
            </TabPanel>
        </TabPanels>
    </Tabs>

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
                <Select
                    v-model="createModel.kategorieId"
                    :options="categories"
                    optionLabel="bezeichnung"
                    optionValue="id"
                    placeholder="Kategorie auswählen"
                    class="w-full"
                />
            </div>

            <div class="field">
                <label>Fachbereiche</label>
                <MultiSelect
                    v-model="createModel.fachbereichIds"
                    :options="fachbereiche"
                    optionLabel="label"
                    optionValue="id"
                    placeholder="Fachbereiche wählen"
                    display="chip"
                    filter
                    filterPlaceholder="Suchen..."
                    class="w-full multiselect-wrap"
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
                        v-model.number="createModel.minKlasse"
                        placeholder="min"
                        class="w-20"
                    />
                    –
                    <InputText
                        type="number"
                        v-model.number="createModel.maxKlasse"
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
