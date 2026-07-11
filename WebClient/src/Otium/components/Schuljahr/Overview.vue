<script setup>
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { Button, Column, DataTable, useDialog } from 'primevue';
import { mande } from 'mande';
import CreateSchoolday from '@/Otium/components/Schuljahr/CreateSchoolday.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { computed, shallowRef } from 'vue';
import EditSchoolday from '@/Otium/components/Schuljahr/EditSchoolday.vue';

const settings = useOtiumStore();
const dialog = useDialog();
const { openConfirmDialog } = useConfirmPopover();
const toast = useToast();

async function setup() {
    await settings.updateSchuljahr(true);
    await settings.updateBlocks();
}

function addDay() {
    dialog.open(CreateSchoolday, {
        props: {
            modal: true,
            header: 'Tag hinzufügen',
        },
        emits: {
            onUpdate: () => {
                settings.updateSchuljahr(true);
            },
        },
    });
}

function deleteDay(event, data) {
    const callback = async () => {
        const api = mande('/api/management/schuljahr/' + data.datum);
        try {
            await api.delete();
        } catch (error) {
            console.error(error);
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: 'Der Tag konnte nicht gelöscht werden.',
            });
        } finally {
            await settings.updateSchuljahr(true);
        }
    };
    openConfirmDialog(event, callback, 'Tag Löschen', 'Möchten Sie den Tag wirklich löschen');
}

setup();

const displayData = computed(
    () =>
        settings.schuljahr?.map((day) => {
            const convertedDate = new Date(day.datum);
            return {
                datum: day.datum,
                displayDate: `${convertedDate.toLocaleDateString('de-DE', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                })} (${convertedDate.toLocaleDateString('de-DE', {
                    weekday: 'short',
                })})`,
                displayBlocks: day.blocks.map((b) => b.bezeichnung).join(', '),
                original: day,
                wochentyp: day.wochentyp,
            };
        }) ?? [],
);

const expandedRows = shallowRef([]);
</script>

<template>
    <h2>Schultage</h2>
    <p>
        Hier können Sie die Schultage in diesem Schuljahr verwalten. Sie können auch
        <Button
            :to="{ name: 'Verwaltung-Schuljahr-Neu' }"
            as="RouterLink"
            class="p-0 hover:underline"
            variant="link"
        >
            mehrere Termine anlegen.
        </Button>
    </p>
    <DataTable
        v-model:expanded-rows="expandedRows"
        :value="displayData"
        data-key="datum"
        dataKey="datum"
        size="small"
    >
        <Column header="Datum">
            <template #body="{ data, rowTogglerCallback }">
                <Button
                    :icon="
                        expandedRows && expandedRows[data.datum]
                            ? 'pi pi-chevron-down'
                            : 'pi pi-chevron-right'
                    "
                    :label="data.displayDate"
                    class="text-nowrap"
                    severity="info"
                    size="small"
                    variant="text"
                    @click="rowTogglerCallback"
                />
            </template>
        </Column>
        <Column field="wochentyp" header="Wochentyp" />
        <Column header="Blöcke">
            <template #body="{ data }">
                {{ data.displayBlocks }}
            </template>
        </Column>
        <Column class="afra-col-action text-right">
            <template #header>
                <Button
                    v-tooltip="'Tag hinzufügen'"
                    icon="pi pi-plus"
                    size="small"
                    aria-label="Tag hinzufügen"
                    @click="addDay"
                />
            </template>
            <template #body="{ data }">
                <div class="inline-flex">
                    <Button
                        v-tooltip="'Löschen'"
                        aria-label="Löschen"
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="(evt) => deleteDay(evt, data.original)"
                    />
                </div>
            </template>
        </Column>
        <template #expansion="{ data }"> <EditSchoolday :date="data.datum" /> </template>
        <template #empty> Keine Schultage angelegt.</template>
    </DataTable>
</template>

<style scoped></style>
