<script lang="ts" setup>
import { computed, ref } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import FreistellungsListe from '@/Freistellung/components/FreistellungsListe.vue';
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
    () => store.schulleiterAntraege?.filter((a) => a.status === 'SchulleiterBestaetigt') ?? [],
);

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

    <h2 class="text-lg font-semibold mt-4 mb-1">Warten auf Entscheidung</h2>
    <p class="mb-3 text-sm text-muted">
        Die folgenden Anträge wurden vom Sekretariat weitergeleitet und warten auf Ihre
        Entscheidung.
    </p>
    <FreistellungsListe
        :antraege="pendingAntraege"
        empty-text="Aktuell liegen keine Freistellungsanträge zur Entscheidung vor."
        show-student
    >
        <template #default="{ antrag }">
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
    </FreistellungsListe>

    <!-- Rejected section: SL can still reverse a mistaken rejection -->
    <h2 class="text-lg font-semibold mt-8 mb-1">Abgelehnte Anträge</h2>
    <p class="mb-3 text-sm text-muted">
        Bei einer irrtümlichen Ablehnung kann der Antrag hier nachträglich noch genehmigt
        werden.
    </p>
    <FreistellungsListe
        :antraege="abgelehnteAntraege"
        empty-text="Aktuell sind keine Anträge abgelehnt."
        show-student
        muted
        show-status
    >
        <template #default="{ antrag }">
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
    </FreistellungsListe>

    <h2 class="text-lg font-semibold mt-8 mb-2">Genehmigte Anträge</h2>
    <FreistellungsListe
        :antraege="genehmigteAntraege"
        empty-text="Es wurden noch keine Freistellungsanträge genehmigt."
        show-student
        muted
        show-status
    >
        <template #default="{ antrag }">
            <FreistellungsPdfButton :antrag-id="antrag.id" :base-path="BASE_PATH" />
        </template>
    </FreistellungsListe>
</template>
