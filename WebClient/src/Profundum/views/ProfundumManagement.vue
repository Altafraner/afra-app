<script setup>
import { ref, computed } from 'vue';
import { mande } from 'mande';
import {
    Column,
    Checkbox,
    InputText,
    InputNumber,
    DataTable,
    Tabs,
    Tab,
    TabList,
    TabPanels,
    TabPanel,
    Button,
    useToast,
} from 'primevue';
import Dropdown from 'primevue/dropdown';

const toast = useToast();

const einwahlZeitraeume = ref([]);
const profunda = ref([]);
const profundaKategorien = ref([]);

const editingRows = ref([]);
const editingRowsProfunda = ref([]);

async function getEinwahlZeitraeume() {
    const getter = mande('/api/profundum/management/einwahlzeitraum');
    einwahlZeitraeume.value = await getter.get();
}

async function getProfunda() {
    const getter = mande('/api/profundum/management/profundum');
    try {
        const list = await getter.get();
        profunda.value = list.map((item) => ({
            ...item,
            kategorieId: item.kategorieId ?? item.kategorie,
        }));
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Profunda konnten nicht geladen werden.',
        });
    }
}

async function getProfundaKategorien() {
    const getter = mande('/api/profundum/management/kategorie');
    try {
        profundaKategorien.value = await getter.get();
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Profunda-Kategorien konnten nicht geladen werden.',
        });
    }
}

async function updateKategorie(kategorie) {
    const updater = mande(`/api/profundum/management/kategorie/${kategorie.id}`);
    try {
        await updater.put({
            bezeichnung: kategorie.bezeichnung,
            profilProfundum: !!kategorie.profilProfundum,
            maxProEinwahl: kategorie.maxProEinwahl ?? null,
        });
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Profunda-Kategorie konnte nicht geändert werden.',
        });
    } finally {
        await getProfundaKategorien();
    }
}

async function createKategorie(kategorie) {
    const creator = mande('/api/profundum/management/kategorie');
    try {
        await creator.post({
            bezeichnung: kategorie.bezeichnung?.trim() || '',
            profilProfundum: !!kategorie.profilProfundum,
            maxProEinwahl: kategorie.maxProEinwahl ?? null,
        });
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Profunda-Kategorie konnte nicht erstellt werden.',
        });
    } finally {
        await getProfundaKategorien();
    }
}

async function updateProfundum(item) {
    const updater = mande(`/api/profundum/management/profundum/${item.id}`);
    try {
        await updater.put({
            bezeichnung: item.bezeichnung,
            minKlasse: item.minKlasse ?? null,
            maxKlasse: item.maxKlasse ?? null,
            kategorieId: item.kategorieId ?? null,
        });
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Das Profundum konnte nicht geändert werden.',
        });
    } finally {
        await getProfunda();
    }
}

async function createProfundum(item) {
    const creator = mande('/api/profundum/management/profundum');
    try {
        await creator.post({
            bezeichnung: item.bezeichnung?.trim() || '',
            minKlasse: item.minKlasse ?? null,
            maxKlasse: item.maxKlasse ?? null,
            kategorieId: item.kategorieId ?? null,
        });
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Das Profundum konnte nicht erstelt werden.',
        });
    } finally {
        await getProfunda();
    }
}

async function deleteKategorie(id) {
    if (!id || id === '__new__kategorie') return;
    if (!confirm('Kategorie wirklich löschen?')) return;

    const deleter = mande(`/api/profundum/management/kategorie/${id}`);
    await deleter.delete();

    // remove from edit state if needed
    editingRows.value = editingRows.value.filter((r) => r.id !== id);

    await getProfundaKategorien();
    await getProfunda();
}

async function deleteProfundum(id) {
    if (!id || id === '__new__profundum') return;
    if (!confirm('Profundum-Definition wirklich löschen?')) return;

    const deleter = mande(`/api/profundum/management/profundum/${id}`);
    await deleter.delete();

    editingRowsProfunda.value = editingRowsProfunda.value.filter((r) => r.id !== id);

    await getProfunda();
}

function toggleEdit(rowData) {
    const rowIndex = editingRows.value.findIndex((r) => r.id === rowData.id);
    if (rowIndex === -1) {
        editingRows.value = [rowData];
    } else {
        // saving
        if (rowData._isNew) {
            createKategorie(rowData);
        } else {
            updateKategorie(rowData);
        }
        editingRows.value = editingRows.value.filter((r) => r.id !== rowData.id);
    }
}

function toggleEditProfunda(rowData) {
    const rowIndex = editingRowsProfunda.value.findIndex((r) => r.id === rowData.id);
    if (rowIndex === -1) {
        editingRowsProfunda.value = [rowData];
    } else {
        if (rowData._isNew) {
            createProfundum(rowData);
        } else {
            updateProfundum(rowData);
        }
        editingRowsProfunda.value = editingRowsProfunda.value.filter(
            (r) => r.id !== rowData.id,
        );
    }
}

function kategorieLabelById(id) {
    const hit = profundaKategorien.value.find((k) => k.id === id);
    return hit?.bezeichnung ?? '';
}

const kategorieRows = computed(() => {
    const newRow = {
        id: '__new__kategorie',
        bezeichnung: '',
        profilProfundum: false,
        maxProEinwahl: null,
        _isNew: true,
    };
    return [...profundaKategorien.value, newRow];
});

const profundaRows = computed(() => {
    const newRow = {
        id: '__new__profundum',
        bezeichnung: '',
        minKlasse: null,
        maxKlasse: null,
        kategorieId: null,
        _isNew: true,
    };
    return [...profunda.value, newRow];
});

// Preload
await getEinwahlZeitraeume();
await getProfunda();
await getProfundaKategorien();
</script>

<template>
    <h1>Profunda-Verwaltung</h1>

    <Tabs lazy value="0">
        <TabList>
            <Tab value="0">Zeiträume</Tab>
            <Tab value="1">Kategorien</Tab>
            <Tab value="2">Profunda</Tab>
        </TabList>
        <TabPanels>
            <TabPanel value="0">
                <DataTable :value="einwahlZeitraeume">
                    <Column field="einwahlStart" header="Einwahl Start" />
                    <Column field="einwahlStop" header="Einwahl Stop" />
                </DataTable>
            </TabPanel>
            <TabPanel value="1">
                <DataTable
                    v-model:editingRows="editingRows"
                    :value="kategorieRows"
                    editMode="row"
                    dataKey="id"
                    :tableStyle="{ tableLayout: 'fixed', width: '100%' }"
                >
                    <Column field="bezeichnung" header="Bezeichnung" style="width: 40%">
                        <template #body="{ data, field }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ data[field] }}
                            </span>
                        </template>
                        <template #editor="{ data, field }">
                            <div style="width: 100%; min-width: 0">
                                <InputText
                                    v-model="data[field]"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column field="profilProfundum" header="Profil" style="width: 20%">
                        <template #editor="{ data, field }">
                            <div
                                style="
                                    width: 100%;
                                    min-width: 0;
                                    display: flex;
                                    justify-content: center;
                                "
                            >
                                <Checkbox v-model="data[field]" :binary="true" />
                            </div>
                        </template>
                        <template #body="{ data, field }">
                            <span>
                                {{
                                    data._isNew && data[field] === false
                                        ? ''
                                        : data[field]
                                          ? 'Ja'
                                          : 'Nein'
                                }}
                            </span>
                        </template>
                    </Column>

                    <Column
                        field="maxProEinwahl"
                        header="Max. pro Schüler pro Einwahl"
                        style="width: 30%"
                    >
                        <template #body="{ data, field }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ data[field] }}
                            </span>
                        </template>
                        <template #editor="{ data, field }">
                            <div style="width: 100%; min-width: 0">
                                <InputNumber
                                    v-model="data[field]"
                                    :inputStyle="{ width: '100%', minWidth: '0' }"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column header="Aktion" style="width: 16%; max-width: 160px">
                        <template #body="{ data }">
                            <Button
                                :icon="data._isNew ? 'pi pi-plus' : 'pi pi-pencil'"
                                class="p-button-rounded p-button-text"
                                @click="toggleEdit(data)"
                                v-if="!editingRows.some((r) => r.id === data.id)"
                                v-tooltip="data._isNew ? 'Neu anlegen' : 'Bearbeiten'"
                            />
                            <Button
                                icon="pi pi-check"
                                class="p-button-rounded p-button-success p-button-text"
                                @click="toggleEdit(data)"
                                v-else
                                v-tooltip="'Speichern'"
                            />

                            <Button
                                icon="pi pi-trash"
                                class="p-button-rounded p-button-danger p-button-text"
                                style="margin-left: 0.25rem"
                                @click="deleteKategorie(data.id)"
                                v-if="!data._isNew"
                                v-tooltip="'Löschen'"
                            />
                        </template>
                    </Column>
                </DataTable>
            </TabPanel>
            <TabPanel value="2">
                <p>
                    Profunda Defintionen enthalten allgemeine, thematische Informationen zum
                    Profundum. Ihnen können mehere Profundums Instanzen im selben oder auch in
                    verschiedenen Semestern zugeordnet sein.
                </p>

                <DataTable
                    v-model:editingRows="editingRowsProfunda"
                    :value="profundaRows"
                    editMode="row"
                    dataKey="id"
                    :tableStyle="{ tableLayout: 'fixed', width: '100%' }"
                >
                    <Column field="bezeichnung" header="Bezeichnung" style="width: 40%">
                        <template #body="{ data, field }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ data[field] }}
                            </span>
                        </template>
                        <template #editor="{ data, field }">
                            <div style="width: 100%; min-width: 0">
                                <InputText
                                    v-model="data[field]"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column field="minKlasse" header="Min. Klasse" style="width: 10%">
                        <template #body="{ data, field }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ data[field] }}
                            </span>
                        </template>
                        <template #editor="{ data, field }">
                            <div style="width: 100%; min-width: 0">
                                <InputNumber
                                    v-model="data[field]"
                                    :inputStyle="{ width: '100%', minWidth: '0' }"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column field="maxKlasse" header="Max. Klasse" style="width: 10%">
                        <template #body="{ data, field }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ data[field] }}
                            </span>
                        </template>
                        <template #editor="{ data, field }">
                            <div style="width: 100%; min-width: 0">
                                <InputNumber
                                    v-model="data[field]"
                                    :inputStyle="{ width: '100%', minWidth: '0' }"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column field="kategorieId" header="Kategorie" style="width: 30%">
                        <template #body="{ data }">
                            <span
                                style="
                                    display: block;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    white-space: nowrap;
                                "
                            >
                                {{ kategorieLabelById(data.kategorieId) }}
                            </span>
                        </template>
                        <template #editor="{ data }">
                            <div style="width: 100%; min-width: 0">
                                <Dropdown
                                    v-model="data.kategorieId"
                                    :options="profundaKategorien"
                                    optionLabel="bezeichnung"
                                    optionValue="id"
                                    placeholder="Kategorie auswählen"
                                    class="w-full"
                                    style="width: 100%; min-width: 0; box-sizing: border-box"
                                    appendTo="self"
                                />
                            </div>
                        </template>
                    </Column>

                    <Column header="Aktion" style="width: 16%; max-width: 160px">
                        <template #body="{ data }">
                            <Button
                                :icon="data._isNew ? 'pi pi-plus' : 'pi pi-pencil'"
                                class="p-button-rounded p-button-text"
                                @click="toggleEditProfunda(data)"
                                v-if="!editingRowsProfunda.some((r) => r.id === data.id)"
                                v-tooltip="data._isNew ? 'Neu anlegen' : 'Bearbeiten'"
                            />
                            <Button
                                icon="pi pi-check"
                                class="p-button-rounded p-button-success p-button-text"
                                @click="toggleEditProfunda(data)"
                                v-else
                                v-tooltip="'Speichern'"
                            />

                            <Button
                                icon="pi pi-trash"
                                class="p-button-rounded p-button-danger p-button-text"
                                style="margin-left: 0.25rem"
                                @click="deleteProfundum(data.id)"
                                v-if="!data._isNew"
                                v-tooltip="'Löschen'"
                            />
                        </template>
                    </Column>
                </DataTable>
            </TabPanel>
        </TabPanels>
    </Tabs>
</template>
