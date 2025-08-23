<script setup>
import { useUser } from '@/stores/user.js';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { Button, Column, DataTable, Dialog, Skeleton, useConfirm, useToast } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { findPath } from '@/helpers/tree.js';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import { RouterLink } from 'vue-router';
import AfraKategorieTag from '@/Otium/components/Shared/AfraKategorieTag.vue';
import CreateOtiumForm from '@/Otium/components/Management/CreateOtiumForm.vue';

const user = useUser();
const settings = useOtiumStore();
const toast = useToast();
const confirm = useConfirm();

const loading = ref(true);
const createDialogOpen = ref(false);

const otia = ref([]);

async function getOtia() {
    const getter = mande('/api/otium/management/otium');
    otia.value = await getter.get();
}

async function deleteOtium(id) {
    const api = mande('/api/otium/management/otium/' + id);
    try {
        await api.delete();
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Löschen des Otiums aufgetreten',
        });
    } finally {
        await getOtia();
    }
}

async function createOtium(data) {
    const api = mande('/api/otium/management/otium');
    try {
        await api.post(data);
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Erstellen des Otiums aufgetreten',
        });
    } finally {
        createDialogOpen.value = false;
        await getOtia();
    }
}

function openCreateDialog() {
    createDialogOpen.value = true;
}

const openConfirmDialog = (event, callback, message) => {
    confirm.require({
        target: event.currentTarget,
        message: message,
        icon: 'pi pi-exclamation-triangle',
        acceptProps: {
            label: 'Ja',
            severity: 'danger',
        },
        rejectProps: {
            label: 'Nein',
            severity: 'secondary',
        },
        accept: callback,
    });
};

const confirmDelete = (event, id) => {
    const onConfirm = () => deleteOtium(id);
    openConfirmDialog(event, onConfirm, 'Otium löschen?');
};

async function setup() {
    try {
        await settings.updateKategorien();
        await getOtia();
        loading.value = false;
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        await user.update();
    }
}

setup();
</script>

<template>
    <template v-if="!loading">
        <h2>Alle Otia</h2>
        <p>Klicken sie auf ein Otium, um Details zu sehen oder es zu Bearbeiten.</p>
        <DataTable :value="otia">
            <Column header="Bezeichnung">
                <template #body="{ data }">
                    <Button
                        :as="RouterLink"
                        :label="data.bezeichnung"
                        :to="{ name: 'Verwaltung-Otium', params: { otiumId: data.id } }"
                        variant="text"
                    />
                </template>
            </Column>
            <Column header="Kategorie">
                <template #body="{ data }">
                    <SimpleBreadcrumb
                        :model="findPath(settings.kategorien, data.kategorie)"
                        wrap
                    >
                        <template #item="{ item }">
                            <AfraKategorieTag :value="item" minimal />
                        </template>
                    </SimpleBreadcrumb>
                </template>
            </Column>
            <Column class="text-right" header="Termine">
                <template #body="{ data }">
                    {{ data.termine }}
                </template>
            </Column>
            <Column class="text-right afra-col-action">
                <template #header>
                    <Button
                        v-tooltip="'Neues Otium'"
                        icon="pi pi-plus"
                        @click="openCreateDialog"
                    />
                </template>
                <template #body="{ data }">
                    <Button
                        v-if="!data.termine || data.termine.length === 0"
                        v-tooltip.left="'Löschen'"
                        icon="pi pi-times"
                        severity="danger"
                        variant="text"
                        @click="(event) => confirmDelete(event, data.id)"
                    />
                    <Button
                        v-else
                        v-tooltip.left="'Nur Otia ohne Termine können gelöscht werden'"
                        disabled
                        icon="pi pi-times"
                        severity="secondary"
                        variant="text"
                    />
                </template>
            </Column>
            <template #empty>
                <div class="flex justify-center">Es sind keine Otia angelegt.</div>
            </template>
        </DataTable>
        <Dialog
            v-model:visible="createDialogOpen"
            :style="{ width: '35rem' }"
            header="Otium hinzufügen"
            modal
        >
            <CreateOtiumForm @submit="createOtium" />
        </Dialog>
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
</template>

<style scoped></style>
