<script lang="ts" setup>
import { computed, ref } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import FreistellungsListe from '@/Freistellung/components/FreistellungsListe.vue';
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
    () =>
        store.sekretariatAntraege?.filter((a) => a.status === 'ElternbestaetigungAusstehend') ??
        [],
);
const zuPruefen = computed(
    () =>
        store.sekretariatAntraege?.filter(
            (a) => a.status === 'ElternbestaetigungEingereicht',
        ) ?? [],
);
const cevexOffen = computed(
    () =>
        store.sekretariatAntraege?.filter(
            (a) => a.status === 'SchulleiterBestaetigt' && !a.inCevexEingetragen,
        ) ?? [],
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
        title: 'Erledigt',
        description: 'Als in Cevex eingetragen markiert.',
    });
    if (ok) await store.refreshSekretariatAntraege();
    markingCevex.value = null;
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Sekretariat)</h1>

    <h2 class="text-lg font-semibold mt-4 mb-1">Elternbestätigung entscheiden</h2>
    <p class="mb-3 text-sm text-muted">
        Die folgenden Anträge wurden von allen betroffenen Lehrkräften und Mentor:innen
        entschieden. Bitte klären Sie, ob eine Elternbestätigung erforderlich ist.
    </p>
    <FreistellungsListe
        :antraege="zuEntscheiden"
        empty-text="Aktuell liegen keine zu entscheidenden Freistellungsanträge vor."
        show-student
    >
        <template #default="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="bearbeiten"
                    icon="i-lucide-circle-help"
                    color="success"
                    size="sm"
                    @click="openEntscheidungDialog(antrag, true)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>
    </FreistellungsListe>

    <h2 class="text-lg font-semibold mt-8 mb-1">Wartet auf Rückmeldung der Eltern</h2>
    <p class="mb-3 text-sm text-muted">
        Diese Anträge wurden mit einem Hinweis an den Schüler / die Schülerin zurückgesendet und
        warten darauf, dass die Elternbestätigung nachgereicht wird.
    </p>
    <FreistellungsListe
        :antraege="wartetAufEltern"
        empty-text="Aktuell wartet kein Antrag auf die Eltern."
        show-student
        :show-stunden="false"
        :show-entscheidungen="false"
        muted
    />

    <h2 class="text-lg font-semibold mt-8 mb-1">Elternbestätigung prüfen</h2>
    <p class="mb-3 text-sm text-muted">
        Der Schüler / die Schülerin hat die Elternbestätigung nachgereicht. Bitte prüfen.
    </p>
    <FreistellungsListe
        :antraege="zuPruefen"
        empty-text="Aktuell ist keine nachgereichte Elternbestätigung zu prüfen."
        show-student
    >
        <template #default="{ antrag }">
            <UButton
                label="Prüfen"
                icon="i-lucide-circle-help"
                color="success"
                size="sm"
                @click="openEntscheidungDialog(antrag, false)"
            />
        </template>
    </FreistellungsListe>

    <h2 class="text-lg font-semibold mt-8 mb-1">In Cevex einzutragen</h2>
    <p class="mb-3 text-sm text-muted">
        Diese Anträge wurden vom Schulleiter genehmigt und müssen noch in Cevex eingetragen
        werden.
    </p>
    <FreistellungsListe
        :antraege="cevexOffen"
        empty-text="Aktuell ist nichts in Cevex einzutragen."
        show-student
        :show-stunden="false"
        :show-entscheidungen="false"
    >
        <template #default="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="In Cevex eingetragen"
                    icon="i-lucide-check-circle"
                    color="success"
                    size="sm"
                    :loading="markingCevex === antrag.id"
                    @click="cevexErledigt(antrag.id)"
                />
                <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
            </div>
        </template>
    </FreistellungsListe>

    <h2 class="text-lg font-semibold mt-8 mb-2">Weitere Anträge</h2>
    <FreistellungsListe
        :antraege="verlauf"
        empty-text="Keine weiteren Freistellungsanträge."
        show-student
        :show-stunden="false"
        :show-entscheidungen="false"
        muted
        show-status
    >
        <template #default="{ antrag }">
            <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
        </template>
    </FreistellungsListe>
</template>
