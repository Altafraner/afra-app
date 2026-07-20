<script setup>
import { useUser } from '@/stores/user';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { Button, Column, DataTable, Skeleton, useToast } from 'primevue';
import { ref, shallowRef, watch } from 'vue';
import { mande } from 'mande';
import { findPath } from '@/helpers/tree.js';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import { RouterLink } from 'vue-router';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import CreateOtiumForm from '@/Otium/components/Management/CreateOtiumForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';

const user = useUser();
const settings = useOtiumStore();
const toast = useToast();
const { openConfirmDialog } = useConfirmPopover();
const loading = ref(true);
const showHidden = shallowRef(false);
const overlay = useOverlay();

const otia = shallowRef([]);

async function getOtia() {
    const getter = mande('/api/otium/management/otium');
    otia.value = await getter.get({ query: { includeHidden: showHidden.value } });
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
        await getOtia();
    }
}

const createDialog = overlay.create(CreateOtiumForm);

async function openCreateDialog() {
    const data = await createDialog.open();
    if (!data) return;
    await createOtium(data);
}

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

async function hide(data, value) {
    try {
        const api = mande(`/api/otium/management/otium/${data.id}/hidden`);
        await api.put(null, { query: { value: value } });
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Verstecken aufgetreten',
        });
    } finally {
        await getOtia();
    }
}

setup();

watch(showHidden, getOtia);
</script>

<template>
    <template v-if="!loading">
        <h2>Alle Otia</h2>
        <p>Klicken sie auf ein Otium, um Details zu sehen oder es zu Bearbeiten.</p>
        <DataTable :value="otia" data-key="id" size="small">
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
                            <OtiumKategorieTag :value="item" minimal />
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
                        aria-label="Neues Otium"
                        @click="openCreateDialog"
                        size="small"
                    />
                </template>
                <template #body="{ data }">
                    <div class="inline-flex gap-1">
                        <Button
                            v-if="!data.termine || data.termine.length === 0"
                            v-tooltip.left="'Löschen'"
                            aria-label="Löschen"
                            icon="pi pi-times"
                            severity="danger"
                            size="small"
                            variant="text"
                            @click="(event) => confirmDelete(event, data.id)"
                        />
                        <Button
                            v-else
                            v-tooltip.left="'Nur Otia ohne Termine können gelöscht werden'"
                            aria-disabled
                            aria-label="Löschen"
                            disabled
                            icon="pi pi-times"
                            severity="danger"
                            size="small"
                            variant="text"
                        />
                        <Button
                            v-if="!data.hidden"
                            v-tooltip.left="'Verstecken'"
                            aria-label="Verstecken"
                            icon="pi pi-eye"
                            severity="secondary"
                            size="small"
                            variant="text"
                            @click="() => hide(data, true)"
                        />
                        <Button
                            v-else
                            v-tooltip.left="'Verstecken'"
                            aria-label="Verstecken"
                            icon="pi pi-eye-slash"
                            severity="warn"
                            size="small"
                            variant="text"
                            @click="() => hide(data, false)"
                        />
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="flex justify-center">Es sind keine Otia angelegt.</div>
            </template>
        </DataTable>
        <div class="flex justify-end mt-4">
            <Button
                v-if="!showHidden"
                icon="pi pi-eye"
                label="Ausgeblendete anzeigen"
                severity="secondary"
                @click="showHidden = true"
            />
            <Button
                v-else
                icon="pi pi-eye-slash"
                label="Ausgeblendete verbergen"
                severity="secondary"
                @click="showHidden = false"
            />
        </div>
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
