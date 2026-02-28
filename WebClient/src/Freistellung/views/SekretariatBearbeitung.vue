<script lang="ts" setup>
import { computed, ref } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import FreistellungsWorkflowBoard, {
    type FreistellungsBoardSection,
} from '@/Freistellung/components/FreistellungsWorkflowBoard.vue';
import FreistellungsPdfButton from '@/Freistellung/components/FreistellungsPdfButton.vue';
import ElternbestaetigungDialog from '@/Freistellung/components/ElternbestaetigungDialog.vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';

const BASE_PATH = '/api/freistellung/sekretariat';

const store = useFreistellungStore();
const { run } = useFreistellungAction();
const overlay = useOverlay();
const entscheidungDialog = overlay.create(ElternbestaetigungDialog);

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Sekretariat' } },
];

const markingCevex = ref<string | null>(null);

await store.updateSekretariatAntraege();

const zuEntscheiden = computed(
    () => store.sekretariatAntraege?.filter((a) => a.status === 'BeiSekretariat') ?? [],
);
const wartetAufEltern = computed(
    () => store.sekretariatAntraege?.filter((a) => a.status === 'WartetAufEltern') ?? [],
);
const zuPruefen = computed(
    () =>
        store.sekretariatAntraege?.filter(
            (a) => a.status === 'ElternbestaetigungEingereicht',
        ) ?? [],
);
const cevexOffen = computed(
    () => store.sekretariatAntraege?.filter((a) => a.status === 'Genehmigt') ?? [],
);
const verlauf = computed(() => {
    const erfasst = new Set<Freistellungsantrag>([
        ...zuEntscheiden.value,
        ...wartetAufEltern.value,
        ...zuPruefen.value,
        ...cevexOffen.value,
    ]);
    return store.sekretariatAntraege?.filter((a) => !erfasst.has(a)) ?? [];
});

const sections = computed<FreistellungsBoardSection[]>(() => [
    {
        key: 'zuEntscheiden',
        title: 'Elternbestätigung entscheiden',
        description:
            'Diese Anträge wurden von allen betroffenen Lehrkräften und Mentor:innen eingeschätzt. Bitte klären Sie, ob eine Elternbestätigung erforderlich ist.',
        antraege: zuEntscheiden.value,
        emptyText: 'Aktuell liegen keine zu entscheidenden Freistellungsanträge vor.',
        showStudent: true,
    },
    {
        key: 'wartetAufEltern',
        title: 'Wartet auf Rückmeldung der Eltern',
        description:
            'Diese Anträge wurden mit einem Hinweis an den Schüler / die Schülerin zurückgesendet und warten darauf, dass die Elternbestätigung nachgereicht wird.',
        antraege: wartetAufEltern.value,
        emptyText: 'Aktuell wartet kein Antrag auf die Eltern.',
        showStudent: true,
        showStunden: false,
        showEntscheidungen: false,
        muted: true,
    },
    {
        key: 'zuPruefen',
        title: 'Elternbestätigung prüfen',
        description:
            'Der Schüler / die Schülerin hat die Elternbestätigung nachgereicht. Bitte prüfen.',
        antraege: zuPruefen.value,
        emptyText: 'Aktuell ist keine nachgereichte Elternbestätigung zu prüfen.',
        showStudent: true,
    },
    {
        key: 'cevexOffen',
        title: 'In Cevex einzutragen',
        description:
            'Diese Anträge wurden vom Schulleiter genehmigt und müssen noch in Cevex eingetragen werden.',
        antraege: cevexOffen.value,
        emptyText: 'Aktuell ist nichts in Cevex einzutragen.',
        showStudent: true,
        showStunden: false,
        showEntscheidungen: false,
    },
    {
        key: 'verlauf',
        title: 'Weitere Anträge',
        antraege: verlauf.value,
        emptyText: 'Keine weiteren Freistellungsanträge.',
        showStudent: true,
        showStunden: false,
        showEntscheidungen: false,
        muted: true,
        showStatus: true,
    },
]);

async function openEntscheidungDialog(antrag: Freistellungsantrag, asksErforderlich: boolean) {
    const result = await entscheidungDialog.open({ grund: antrag.grund, asksErforderlich });
    if (!result) return;

    const weitergeleitet = !result.erforderlich || result.vorhanden;
    const ok = await run(`${BASE_PATH}/${antrag.id}/elternbestaetigung-entscheidung`, result, {
        title: 'Gespeichert',
        description: weitergeleitet
            ? 'Der Antrag wurde an die Schulleitung weitergeleitet.'
            : 'Der Antrag wurde mit Hinweis an den Schüler / die Schülerin zurückgesendet.',
    });
    if (ok) await store.refreshSekretariatAntraege();
}

async function cevexErledigt(antragId: string) {
    markingCevex.value = antragId;
    const ok = await run(`${BASE_PATH}/${antragId}/cevex-erledigt`, null, {
        title: 'Abgeschlossen',
        description: 'Der Antrag wurde als in Cevex eingetragen markiert.',
    });
    if (ok) await store.refreshSekretariatAntraege();
    markingCevex.value = null;
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Sekretariat)</h1>

    <FreistellungsWorkflowBoard :sections="sections">
        <template #zuEntscheiden="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Entscheiden"
                    icon="i-lucide-circle-help"
                    color="success"
                    size="sm"
                    @click="openEntscheidungDialog(antrag, true)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>

        <template #zuPruefen="{ antrag }">
            <UButton
                label="Prüfen"
                icon="i-lucide-circle-help"
                color="success"
                size="sm"
                @click="openEntscheidungDialog(antrag, false)"
            />
        </template>

        <template #cevexOffen="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Als eingetragen markieren"
                    icon="i-lucide-check-circle"
                    color="success"
                    size="sm"
                    :loading="markingCevex === antrag.id"
                    @click="cevexErledigt(antrag.id)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>

        <template #verlauf="{ antrag }">
            <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
        </template>
    </FreistellungsWorkflowBoard>
</template>
