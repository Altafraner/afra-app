<script lang="ts" setup>
import { ref } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import FreistellungsListe from '@/Freistellung/components/FreistellungsListe.vue';
import type { Freistellungsantrag } from '@/Freistellung/models/freistellung';

const store = useFreistellungStore();
const { run } = useFreistellungAction();
const resubmitting = ref<string | null>(null);

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } }];

await store.updateMeineAntraege();

function canNachreichen(antrag: Freistellungsantrag) {
    return antrag.status === 'WartetAufEltern';
}

async function elternbestaetigungNachreichen(antragId: string) {
    resubmitting.value = antragId;
    const ok = await run(
        `/api/freistellung/sus/${antragId}/elternbestaetigung-nachreichen`,
        null,
        {
            title: 'Nachgereicht',
            description: 'Das Sekretariat prüft die nachgereichte Elternbestätigung.',
        },
    );
    if (ok) await store.refreshMeineAntraege();
    resubmitting.value = null;
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <div class="flex items-center justify-between">
        <h1>Meine Freistellungsanträge</h1>
        <UButton icon="i-lucide-plus" label="Neuer Antrag" size="sm" to="/freistellung/neu" />
    </div>

    <FreistellungsListe
        class="mt-4"
        :antraege="store.meineAntraege ?? []"
        empty-text="Du hast noch keine Freistellungsanträge gestellt."
        show-status
    >
        <template #default="{ antrag }">
            <UButton
                v-if="canNachreichen(antrag)"
                label="Elternbestätigung nachreichen"
                icon="i-lucide-refresh-cw"
                color="warning"
                size="sm"
                :loading="resubmitting === antrag.id"
                @click="elternbestaetigungNachreichen(antrag.id)"
            />
        </template>
    </FreistellungsListe>
</template>
