<script lang="ts" setup>
import { computed, ref } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import FreistellungsWorkflowBoard, {
    type FreistellungsBoardSection,
} from '@/Freistellung/components/FreistellungsWorkflowBoard.vue';
import FreistellungsPdfButton from '@/Freistellung/components/FreistellungsPdfButton.vue';
import SimpleTextDialog from '@/components/Form/SimpleTextDialog.vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';

const BASE_PATH = '/api/freistellung/schulleiter';

const store = useFreistellungStore();
const { run } = useFreistellungAction();
const overlay = useOverlay();
const ablehnenDialog = overlay.create(SimpleTextDialog);

const navItems = [
    { label: 'Freistellungsantrag (Schulleiter)', route: { name: 'Freistellung-Schulleiter' } },
];

const approving = ref<string | null>(null);

await store.updateSchulleiterAntraege();

const pendingAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status === 'BeimSchulleiter') ?? [],
);
const abgelehnteAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status === 'Abgelehnt') ?? [],
);
const genehmigteAntraege = computed(
    () =>
        store.schulleiterAntraege?.filter(
            (a) => a.status === 'Genehmigt' || a.status === 'Abgeschlossen',
        ) ?? [],
);

const sections = computed<FreistellungsBoardSection[]>(() => [
    {
        key: 'pending',
        title: 'Warten auf Entscheidung',
        description:
            'Die folgenden Anträge wurden vom Sekretariat weitergeleitet und warten auf Ihre Entscheidung.',
        antraege: pendingAntraege.value,
        emptyText: 'Aktuell liegen keine Freistellungsanträge zur Entscheidung vor.',
        showStudent: true,
    },
    {
        key: 'abgelehnt',
        title: 'Abgelehnte Anträge',
        description:
            'Bei einer irrtümlichen Ablehnung kann der Antrag hier nachträglich noch genehmigt werden.',
        antraege: abgelehnteAntraege.value,
        emptyText: 'Aktuell sind keine Anträge abgelehnt.',
        showStudent: true,
        muted: true,
        showStatus: true,
    },
    {
        key: 'genehmigt',
        title: 'Genehmigte Anträge',
        antraege: genehmigteAntraege.value,
        emptyText: 'Es wurden noch keine Freistellungsanträge genehmigt.',
        showStudent: true,
        muted: true,
        showStatus: true,
    },
]);

async function genehmigen(antragId: string) {
    approving.value = antragId;
    const ok = await run(`${BASE_PATH}/${antragId}/bestaetigen`, null, {
        title: 'Genehmigt',
        description: 'Der Freistellungsantrag wurde genehmigt.',
    });
    if (ok) await store.refreshSchulleiterAntraege();
    approving.value = null;
}

async function ablehnen(antrag: Freistellungsantrag) {
    const kommentar = await ablehnenDialog.open({
        title: 'Antrag ablehnen',
        description: `Bitte geben Sie einen Kommentar an, warum der Antrag "${antrag.grund}" abgelehnt wird.`,
        label: 'Kommentar',
        placeholder: 'Warum wird der Antrag abgelehnt?',
        minLength: 1,
        maxLength: 500,
        buttonText: 'Ablehnen',
        buttonColor: 'error',
    });
    if (!kommentar) return;

    const ok = await run(
        `${BASE_PATH}/${antrag.id}/ablehnen`,
        { kommentar },
        { title: 'Abgelehnt', description: 'Der Freistellungsantrag wurde abgelehnt.' },
    );
    if (ok) await store.refreshSchulleiterAntraege();
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Schulleiter)</h1>

    <FreistellungsWorkflowBoard :sections="sections">
        <template #pending="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Genehmigen"
                    icon="i-lucide-check-circle"
                    color="success"
                    size="sm"
                    :loading="approving === antrag.id"
                    @click="genehmigen(antrag.id)"
                />
                <UButton
                    label="Ablehnen"
                    icon="i-lucide-x"
                    color="error"
                    size="sm"
                    @click="ablehnen(antrag)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>

        <template #abgelehnt="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Doch genehmigen"
                    icon="i-lucide-undo"
                    color="success"
                    size="sm"
                    :loading="approving === antrag.id"
                    @click="genehmigen(antrag.id)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>

        <template #genehmigt="{ antrag }">
            <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
        </template>
    </FreistellungsWorkflowBoard>
</template>
