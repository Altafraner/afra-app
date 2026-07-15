<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { ref } from 'vue';
import SimpleTextDialog from '@/components/Form/SimpleTextDialog.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import KriteriumCreationForm from '@/Profundum/components/Forms/KriteriumCreationForm.vue';
import { convertMarkdownToHtml } from '@/composables/markdown.ts';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { Anker, FeedbackKategorie } from '@/Profundum/models/feedback.ts';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
        to: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
    {
        label: 'Kriterien',
        to: {
            name: 'Profundum-Feedback-Kriterien',
        },
    },
];

const feedback = useFeedback();
const overlay = useOverlay();
const { requireConfirm } = useConfirmPopover();

const ankerByKategorie = ref();
const kategorien = ref();

async function setup() {
    const tmp = await feedback.getAllAnker();
    if (tmp == null) return;
    ankerByKategorie.value = tmp.ankerByKategorie;
    kategorien.value = tmp.kategorien;
}

await setup();

async function startCreateAnker(kategorieId: string) {
    const modal = overlay.create(SimpleTextDialog);
    const data = await modal.open({
        title: 'Anker hinzufügen',
        label: 'Bezeichnung',
        placeholder: 'Bezeichnung eingeben',
        buttonText: 'Hinzufügen',
        minLength: 1,
        maxLength: 200,
    });
    if (!data) return;
    await feedback.createAnker(data, kategorieId);
    await setup();
}

async function startChangeAnker(anker: Anker) {
    const modal = overlay.create(SimpleTextDialog);
    const data = await modal.open({
        title: 'Anker ändern',
        label: 'Bezeichnung',
        placeholder: 'Bezeichnung eingeben',
        buttonText: 'Ändern',
        minLength: 1,
        maxLength: 200,
        default: anker.label,
    });
    if (!data) return;
    await feedback.updateAnker(anker.id, data, anker.kategorieId);
    await setup();
}

async function startDeleteAnker(anker: Anker) {
    if (!(await requireConfirm('Wollen Sie den Anker wirklich löschen?'))) return;
    await feedback.deleteAnker(anker.id);
    await setup();
}

async function startAddKategorie() {
    const modal = overlay.create(KriteriumCreationForm);
    const data = await modal.open({
        fachbereiche: undefined,
        isFachlich: undefined,
        label: undefined,
        variant: 'create',
    });
    await feedback.createKategorie(data.label, data.fachbereiche, data.isFachlich);
    await setup();
}

async function startEditKategorie(kategorie: FeedbackKategorie) {
    const modal = overlay.create(KriteriumCreationForm);
    const data = await modal.open({
        fachbereiche: kategorie.fachbereiche.map((k) => k.id),
        isFachlich: kategorie.isFachlich,
        label: kategorie.label,
        variant: 'update',
    });
    await feedback.updateKategorie(
        kategorie.id,
        data.label,
        data.fachbereiche,
        data.isFachlich,
    );
    await setup();
}

async function startDeleteKategorie(kategorieId: string) {
    if (!(await requireConfirm('Wollen Sie die Kategorie wirklich löschen?'))) return;
    await feedback.deleteKategorie(kategorieId);
    await setup();
}
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <div class="flex justify-between items-baseline">
        <h1 class="mb-4">Kriterium-Verwaltung</h1>
        <UButton
            class="mr-2"
            icon="i-lucide-plus"
            label="Neue Kategorie"
            variant="subtle"
            @click="startAddKategorie"
        />
    </div>
    <div class="flex gap-4 flex-col">
        <UCard v-for="kategorie in kategorien">
            <template #title>
                <template v-if="kategorie.isFachlich">Fachliche Kompetenz – </template
                >{{ kategorie.label }}
            </template>
            <template #default>
                <div class="grid grid-cols-[1fr_auto] items-baseline gap-1">
                    <template v-for="anker in ankerByKategorie[kategorie.id]">
                        <span v-html="convertMarkdownToHtml(anker.label, true)" />
                        <span class="flex gap-1 items-baseline justify-end">
                            <UTooltip text="Bearbeiten">
                                <UButton
                                    aria-label="Bearbeiten"
                                    color="secondary"
                                    icon="i-lucide-pencil"
                                    size="sm"
                                    variant="ghost"
                                    @click="startChangeAnker(anker)"
                                />
                            </UTooltip>
                            <UTooltip text="Löschen">
                                <UButton
                                    aria-label="Löschen"
                                    color="error"
                                    icon="i-lucide-x"
                                    size="sm"
                                    variant="ghost"
                                    @click="startDeleteAnker(anker)"
                                />
                            </UTooltip>
                        </span>
                    </template>
                </div>
            </template>
            <template #footer>
                <div class="flex justify-between items-baseline">
                    <span class="flex gap-4 items-baseline">
                        <UButton
                            color="secondary"
                            label="Bearbeiten"
                            icon="i-lucide-pencil"
                            variant="subtle"
                            @click="startEditKategorie(kategorie)"
                        />
                        <UButton
                            color="error"
                            label="Löschen"
                            icon="i-lucide-x"
                            variant="subtle"
                            @click="startDeleteKategorie(kategorie.id)"
                        />
                    </span>
                    <UButton
                        color="primary"
                        icon="i-lucide-plus"
                        label="Neuer Anker"
                        variant="subtle"
                        @click="startCreateAnker(kategorie.id)"
                    />
                </div>
            </template>
        </UCard>
    </div>
</template>
