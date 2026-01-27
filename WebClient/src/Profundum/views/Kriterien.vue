<script setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { Button, Card, useDialog } from 'primevue';
import { ref } from 'vue';
import SimpleTextDialog from '@/components/Form/SimpleTextDialog.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import KriteriumCreationForm from '@/Profundum/components/Forms/KriteriumCreationForm.vue';
import { convertMarkdownToHtml } from '@/composables/markdown.ts';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
        route: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
    {
        label: 'Kriterien',
        route: {
            name: 'Profundum-Feedback-Kriterien',
        },
    },
];

const feedback = useFeedback();
const dialog = useDialog();
const confirmPopover = useConfirmPopover();

const ankerByKategorie = ref();
const kategorien = ref();

async function setup() {
    const tmp = await feedback.getAllAnker();
    ankerByKategorie.value = tmp.ankerByKategorie;
    kategorien.value = tmp.kategorien;
}

await setup();

function startCreateAnker(kategorieId) {
    dialog.open(SimpleTextDialog, {
        props: {
            header: 'Anker hinzufügen',
            modal: true,
        },
        data: {
            maxLength: 200,
            minLength: 1,
            label: 'Bezeichnung',
            buttonLabel: 'Hinzufügen',
        },
        onClose: async ({ data }) => {
            await feedback.createAnker(data.result, kategorieId);
            await setup();
        },
    });
}

function startChangeAnker(anker) {
    dialog.open(SimpleTextDialog, {
        props: {
            header: 'Anker ändern',
            modal: true,
        },
        data: {
            maxLength: 200,
            minLength: 1,
            label: 'Bezeichnung',
            buttonLabel: 'Ändern',
            default: anker.label,
        },
        onClose: async ({ data }) => {
            await feedback.updateAnker(anker.id, data.result, anker.kategorieId);
            await setup();
        },
    });
}

function startDeleteAnker(evt, anker) {
    confirmPopover.openConfirmDialog(
        evt,
        deleteThis,
        'Löschen?',
        'Wollen Sie den Anker wirklich löschen?',
        'danger',
    );
    async function deleteThis() {
        await feedback.deleteAnker(anker.id);
        await setup();
    }
}

function startAddKategorie() {
    dialog.open(KriteriumCreationForm, {
        props: {
            header: 'Kriterium hinzufügen',
            modal: true,
        },
        data: {
            variant: 'create',
        },
        onClose: async ({ data }) => {
            await feedback.createKategorie(data.label, data.fachbereiche);
            await setup();
        },
    });
}

function startEditKategorie(kategorie) {
    dialog.open(KriteriumCreationForm, {
        props: {
            header: 'Kriterium bearbeiten',
            modal: true,
        },
        data: {
            variant: 'update',
            label: kategorie.label,
            fachbereiche: kategorie.fachbereiche.map((k) => k.id),
            isFachlich: kategorie.isFachlich,
        },
        onClose: async ({ data }) => {
            await feedback.updateKategorie(
                kategorie.id,
                data.label,
                data.fachbereiche,
                data.isFachlich,
            );
            await setup();
        },
    });
}

function startDeleteKategorie(evt, kategorieId) {
    confirmPopover.openConfirmDialog(
        evt,
        deleteThis,
        'Löschen?',
        'Wollen Sie die Kategorie wirklich löschen?',
        'danger',
    );
    async function deleteThis() {
        await feedback.deleteKategorie(kategorieId);
        await setup();
    }
}
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <div class="flex justify-between items-baseline">
        <h1 class="mb-4">Kriterium-Verwaltung</h1>
        <Button
            class="mr-2"
            icon="pi pi-plus"
            label="Kategorie hinzufügen"
            severity="primary"
            variant="text"
            @click="startAddKategorie"
        />
    </div>
    <div class="flex gap-4 flex-col">
        <Card v-for="kategorie in kategorien">
            <template #title>
                <h3 class="mt-0 mb-0">
                    <template v-if="kategorie.isFachlich">Fachliche Kompetenz – </template
                    >{{ kategorie.label }}
                </h3>
            </template>
            <template #content>
                <div class="grid grid-cols-[1fr_auto] items-baseline">
                    <template v-for="anker in ankerByKategorie[kategorie.id]">
                        <span v-html="convertMarkdownToHtml(anker.label, true)" />
                        <span class="flex gap-2 items-baseline justify-end">
                            <Button
                                v-tooltip="'Bearbeiten'"
                                aria-label="Bearbeiten"
                                icon="pi pi-pencil"
                                severity="secondary"
                                size="small"
                                variant="text"
                                @click="startChangeAnker(anker)"
                            />
                            <Button
                                v-tooltip="'Löschen'"
                                aria-label="Löschen"
                                icon="pi pi-times"
                                severity="danger"
                                size="small"
                                variant="text"
                                @click="startDeleteAnker($event, anker)"
                            />
                        </span>
                    </template>
                </div>
            </template>
            <template #footer>
                <div class="flex justify-between items-baseline mt-4">
                    <span class="flex gap-4 items-baseline">
                        <Button
                            icon="pi pi-pencil"
                            label="Bearbeiten"
                            severity="secondary"
                            size="small"
                            variant="text"
                            @click="startEditKategorie(kategorie)"
                        />
                        <Button
                            icon="pi pi-times"
                            label="Löschen"
                            severity="danger"
                            size="small"
                            variant="text"
                            @click="startDeleteKategorie($event, kategorie.id)"
                        />
                    </span>
                    <Button
                        class="mr-2"
                        icon="pi pi-plus"
                        label="Anker hinzufügen"
                        severity="primary"
                        size="small"
                        @click="startCreateAnker(kategorie.id)"
                    />
                </div>
            </template>
        </Card>
    </div>
</template>
