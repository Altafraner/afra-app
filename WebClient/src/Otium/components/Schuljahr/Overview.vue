<script setup>
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { Button, Column, DataTable, useConfirm, useDialog, useToast } from 'primevue';
import { mande } from 'mande';
import CreateSchoolday from '@/Otium/components/Schuljahr/CreateSchoolday.vue';
import { useRouter } from 'vue-router';
import BlockSelectorDialog from '@/Otium/components/Management/BlockSelectorDialog.vue';

const settings = useOtiumStore();
const dialog = useDialog();
const confirm = useConfirm();
const toast = useToast();
const router = useRouter();

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
                console.log('Received update event');
                settings.updateSchuljahr(true);
            },
        },
    });
}

function updateDay(data) {
    dialog.open(CreateSchoolday, {
        data: {
            initialValues: data,
        },
        props: {
            modal: true,
            header: 'Tag bearbeiten',
        },
        emits: {
            onUpdate: () => {
                console.log('Received update event');
                settings.updateSchuljahr(true);
            },
        },
    });
}

function deleteDay(event, data) {
    confirm.require({
        target: event.currentTarget,
        message: 'Möchten Sie den Tag wirklich löschen?',
        header: 'Tag löschen',
        icon: 'pi pi-exclamation-triangle',
        acceptProps: {
            label: 'Ja',
            severity: 'danger',
        },
        rejectProps: {
            label: 'Nein',
            severity: 'secondary',
        },
        accept: async () => {
            const api = mande('/api/management/schuljahr/' + data.datum);
            try {
                await api.delete();
            } catch (error) {
                console.error(error);
                toast.add({
                    severity: 'error',
                    summary: 'Fehler',
                    detail: 'Der Tag konnte nicht gelöscht werden.',
                });
            } finally {
                await settings.updateSchuljahr(true);
            }
        },
    });
}

async function showAttendance(data) {
    const result = await mande('/api/schuljahr/' + data.datum).get();

    const blocks = result.map((item) => {
        return {
            id: item.id,
            label: item.name,
        };
    });

    dialog.open(BlockSelectorDialog, {
        props: {
            modal: true,
            header: 'Block auswählen',
        },
        data: {
            items: blocks,
        },
        onClose: (data) => {
            if (data.data && data.data.id)
                router.push({
                    name: 'Aufsicht',
                    query: {
                        blockId: data.data.id,
                    },
                });
        },
    });
}

setup();
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
    <DataTable :value="settings.schuljahr" data-key="datum">
        <Column header="Datum">
            <template #body="{ data }"
                >{{
                    new Date(data.datum).toLocaleDateString('de-DE', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                    })
                }}
                ({{
                    new Date(data.datum).toLocaleDateString('de-DE', {
                        weekday: 'short',
                    })
                }})
            </template>
        </Column>
        <Column field="wochentyp" header="Wochentyp" />
        <Column header="Blöcke">
            <template #body="{ data }">
                {{
                    data.blocks
                        .sort()
                        .map((b) => b.bezeichnung)
                        .join(', ')
                }}
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
                <Button
                    v-tooltip="'Aufsicht'"
                    icon="pi pi-eye"
                    severity="secondary"
                    size="small"
                    variant="text"
                    aria-label="Aufsicht"
                    @click="() => showAttendance(data)"
                />
                <Button
                    v-tooltip="'Bearbeiten'"
                    icon="pi pi-pencil"
                    severity="secondary"
                    size="small"
                    variant="text"
                    aria-label="Bearbeiten"
                    @click="() => updateDay(data)"
                />
                <Button
                    v-tooltip="'Löschen'"
                    icon="pi pi-times"
                    severity="danger"
                    size="small"
                    variant="text"
                    aria-label="Löschen"
                    @click="(evt) => deleteDay(evt, data)"
                />
            </template>
        </Column>
        <template #empty> Keine Schultage angelegt.</template>
    </DataTable>
</template>

<style scoped></style>
