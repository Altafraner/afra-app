<script setup>
import { computed, h, ref } from 'vue';
import { mande } from 'mande';
import { useSortable } from '@vueuse/integrations/useSortable';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { convertMarkdownToHtml } from '@/composables/markdown';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Einwahl',
        to: {
            name: 'Profundum-Einwahl',
        },
    },
];

const toast = useToast();

const katalog = ref({
    optionen: [],
    fixiert: [],
    offeneSlotIds: [],
    minBelegWuensche: 7,
    minWuenschePerSlot: 3,
    aktuelleWuensche: [],
    istAbgegeben: false,
});
const ranked = ref([]);
const draftBusy = ref(false);

useSortable('.ranked-list', ranked, { animation: 150, handle: '.drag-handle' });

async function get() {
    const api = mande('/api/profundum/sus/wuensche');
    katalog.value = await api.get();
    const availableIds = new Set(katalog.value.optionen.map((o) => o.definitionId));
    ranked.value = katalog.value.aktuelleWuensche.filter((id) => availableIds.has(id));
}

async function saveDraft() {
    const api = mande('/api/profundum/sus/wuensche/entwurf');
    draftBusy.value = true;
    try {
        await api.post(ranked.value);
        katalog.value.istAbgegeben = false;
        toast.add({ color: 'success', title: 'Entwurf gespeichert' });
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body?.error ?? 'Entwurf konnte nicht gespeichert werden.',
        });
    } finally {
        draftBusy.value = false;
    }
}

async function send() {
    const api = mande('/api/profundum/sus/wuensche');

    try {
        await api.post(ranked.value);
    } catch (e) {
        const errors = Array.isArray(e.body?.error)
            ? e.body.error
            : String(e.body?.error ?? 'Unbekannter Fehler')
                  .split('\n')
                  .filter(Boolean);

        toast.add({
            color: 'error',
            title: 'Fehler',
            description: h('span', {}, [
                'Deine Belegwünsche sind fehlerhaft: ',
                h(
                    'ul',
                    { class: 'ml-2' },
                    errors.map((e) => h('li', {}, e)),
                ),
            ]),
        });
        return;
    }

    katalog.value.istAbgegeben = true;
    toast.add({
        color: 'success',
        title: 'Wünsche erfolgreich abgegeben',
        description: 'Deine Wünsche wurden erfolgreich gespeichert.',
    });
}

const optionenById = computed(() => {
    const map = new Map();
    for (const option of katalog.value.optionen) map.set(option.definitionId, option);
    return map;
});

const verfuegbareOptionen = computed(() =>
    katalog.value.optionen.filter((o) => !ranked.value.includes(o.definitionId)),
);

const slotAbdeckung = computed(() => {
    const counts = {};
    for (const slotId of katalog.value.offeneSlotIds) counts[slotId] = 0;
    for (const id of ranked.value) {
        const option = optionenById.value.get(id);
        if (!option) continue;
        for (const slotId of option.slotIds) {
            if (slotId in counts) counts[slotId]++;
        }
    }
    return counts;
});

const unterversorgteSlots = computed(() =>
    Object.entries(slotAbdeckung.value).filter(
        ([, count]) => count < katalog.value.minWuenschePerSlot,
    ),
);

const maySend = computed(
    () =>
        ranked.value.length >= katalog.value.minBelegWuensche &&
        unterversorgteSlots.value.length === 0,
);

function addToRanked(definitionId) {
    ranked.value.push(definitionId);
}

function removeFromRanked(index) {
    ranked.value.splice(index, 1);
}

function moveUp(index) {
    if (index === 0) return;
    const [item] = ranked.value.splice(index, 1);
    ranked.value.splice(index - 1, 0, item);
}

function moveDown(index) {
    if (index === ranked.value.length - 1) return;
    const [item] = ranked.value.splice(index, 1);
    ranked.value.splice(index + 1, 0, item);
}

const partnerApi = mande('/api/profundum/sus/partner');
const partnerData = ref({ einladungen: [], wuensche: [] });

async function loadPartnerData() {
    partnerData.value = await partnerApi.get();
}

const einladungByDefinition = computed(() => {
    const map = new Map();
    for (const e of partnerData.value.einladungen) map.set(e.profundumDefinitionId, e);
    return map;
});

const wunschByDefinition = computed(() => {
    const map = new Map();
    for (const w of partnerData.value.wuensche) map.set(w.profundumDefinitionId, w);
    return map;
});

const partnerDialogOpen = ref(false);
const partnerDialogOption = ref(null);
const redeemToken = ref('');
const partnerBusy = ref(false);

function openPartnerDialog(option) {
    partnerDialogOption.value = option;
    redeemToken.value = '';
    partnerDialogOpen.value = true;
}

async function createEinladung() {
    partnerBusy.value = true;
    try {
        await partnerApi.post(`/${partnerDialogOption.value.definitionId}`);
        await loadPartnerData();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte keine Einladung erstellen',
        });
    } finally {
        partnerBusy.value = false;
    }
}

async function cancelEinladung(token) {
    partnerBusy.value = true;
    try {
        await partnerApi.delete(`/einladung/${token}`);
        await loadPartnerData();
    } finally {
        partnerBusy.value = false;
    }
}

async function redeemEinladung() {
    partnerBusy.value = true;
    try {
        const wunsch = await partnerApi.post(
            `/redeem/${partnerDialogOption.value.definitionId}/${redeemToken.value.trim().toLowerCase()}`,
        );
        redeemToken.value = '';
        await loadPartnerData();
        toast.add({
            color: 'success',
            title: 'Partnerschaft bestätigt',
            description: `Mit ${wunsch.partner.vorname} ${wunsch.partner.nachname} für ${wunsch.bezeichnung}.`,
        });
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Einladung konnte nicht angenommen werden',
        });
    } finally {
        partnerBusy.value = false;
    }
}

async function dissolveWunsch(id) {
    partnerBusy.value = true;
    try {
        await partnerApi.delete(`/wunsch/${id}`);
        await loadPartnerData();
    } finally {
        partnerBusy.value = false;
    }
}

const detailDialogOpen = ref(false);
const detailOption = ref(null);

function openDetailDialog(option) {
    detailOption.value = option;
    detailDialogOpen.value = true;
}

const detailDescriptionHtml = computed(() =>
    detailOption.value?.beschreibung
        ? convertMarkdownToHtml(detailOption.value.beschreibung)
        : null,
);

const detailInstanzColumns = [
    { id: 'termin', header: 'Termin' },
    { id: 'ort', accessorKey: 'ort', header: 'Ort' },
    { id: 'verantwortlich', header: 'Verantwortlich' },
    { id: 'maxEinschreibungen', header: 'Max. Teilnehmer' },
];

async function copyToken(token) {
    await navigator.clipboard.writeText(token);
    toast.add({ color: 'success', title: 'Code kopiert' });
}

async function startup() {
    await Promise.all([get(), loadPartnerData()]);
}

startup();
</script>

<template>
    <nav-breadcrumb :items="navItems" />

    <h1>Profundums-Einwahl</h1>

    <p>
        Bitte lest euch die folgenden Hinweise aufmerksam durch.
        <strong
            >Der Zeitpunkt der Abgabe eurer Wünsche innerhalb des Einwahlzeitraums hat keinen
            Einfluss auf die Vergabe.</strong
        >
    </p>

    <h2>Bereits fixierte Belegungen</h2>

    <p v-if="katalog.fixiert.length === 0">
        Ihr habt aktuell keine bereits fixierten Belegungen für diesen Einwahlzeitraum.
    </p>
    <ul v-else class="list-disc pl-6">
        <li v-for="f in katalog.fixiert" :key="f.slotId">
            {{ f.slotLabel }}: {{ f.bezeichnung }}
        </li>
    </ul>

    <h2>Kriterien</h2>

    <p>
        Bitte beachtet das <em>profundarium</em> sowie die Detailansicht Info-Symbol neben jedem
        Angebot) für ausführliche Informationen zu jedem Angebot.
    </p>

    <p>
        Bringt eure Profunda unten in eine Rangfolge, absteigend nach Präferenz. Wählt
        mindestens
        <strong>{{ katalog.minBelegWuensche }}</strong> Profunda. Für jeden offenen Slot müssen
        mindestens <strong>{{ katalog.minWuenschePerSlot }}</strong> eurer gewählten Profunda
        ein Angebot enthalten – nicht gewählte Profunda gelten als "möchte ich nicht belegen".
    </p>

    <p>
        Weitere Einschränkungen werden euch möglicherweise angezeigt, nachdem ihr auf "abgeben"
        geklickt habt. In diesem Fall nehmt ihr bitte entsprechende Änderungen vor und versucht
        es erneut.
    </p>

    <h2>Matching</h2>

    <p>
        Nach dem Zeitfenster zur Einwahl berechnen wir aus den abgegebenen Wünschen eine
        Belegung, die eure Präferenzen nach
        <a
            class="text-blue-500 hover:underline cursor-pointer"
            href="https://github.com/Altafraner/afra-app"
            target="_blank"
            >veröffentlichter Berechnungsvorschrift</a
        >
        bestmöglich berücksichtigt.
    </p>

    <hr class="my-3" />

    <div class="grid grid-cols-1 md:grid-cols-1 gap-6 mb-4">
        <div>
            <h3 class="flex items-center gap-2">
                Meine Rangfolge
                <UBadge
                    v-if="katalog.aktuelleWuensche.length > 0"
                    :label="katalog.istAbgegeben ? 'Abgegeben' : 'Entwurf gespeichert'"
                    :color="katalog.istAbgegeben ? 'success' : 'warning'"
                />
            </h3>
            <ol class="ranked-list flex flex-col gap-2 list-none pl-0">
                <li
                    v-for="(id, index) in ranked"
                    :key="id"
                    class="flex items-center gap-2 border rounded p-2"
                >
                    <UIcon
                        name="i-lucide-grip-vertical"
                        class="drag-handle cursor-grab text-muted"
                    />
                    <span class="w-6 text-right font-bold">{{ index + 1 }}.</span>
                    <span class="flex-1">{{ optionenById.get(id)?.bezeichnung }}</span>
                    <UBadge
                        v-if="optionenById.get(id)?.profilProfundum"
                        label="Profil"
                        color="info"
                    />
                    <UTooltip
                        v-if="optionenById.get(id)?.erlaubtPartnerwahl"
                        text="Partnerwahl"
                    >
                        <UButton
                            icon="i-lucide-users"
                            :color="wunschByDefinition.get(id) ? 'success' : 'neutral'"
                            variant="ghost"
                            @click="openPartnerDialog(optionenById.get(id))"
                        />
                    </UTooltip>
                    <UTooltip text="Details">
                        <UButton
                            icon="i-lucide-info"
                            color="neutral"
                            variant="ghost"
                            @click="openDetailDialog(optionenById.get(id))"
                        />
                    </UTooltip>
                    <UButton
                        icon="i-lucide-arrow-up"
                        color="neutral"
                        variant="ghost"
                        :disabled="index === 0"
                        @click="moveUp(index)"
                    />
                    <UButton
                        icon="i-lucide-arrow-down"
                        color="neutral"
                        variant="ghost"
                        :disabled="index === ranked.length - 1"
                        @click="moveDown(index)"
                    />
                    <UButton
                        icon="i-lucide-x"
                        color="error"
                        variant="ghost"
                        @click="removeFromRanked(index)"
                    />
                </li>
            </ol>
        </div>

        <div>
            <h3>belegbare Profunda außerhalb der Rangfolge</h3>
            <ul class="flex flex-col gap-2 list-none pl-0">
                <li
                    v-for="option in verfuegbareOptionen"
                    :key="option.definitionId"
                    class="flex items-center gap-2 border rounded p-2"
                >
                    <span class="flex-1">{{ option.bezeichnung }}</span>
                    <UBadge v-if="option.profilProfundum" label="Profil" color="info" />
                    <UTooltip text="Details">
                        <UButton
                            icon="i-lucide-info"
                            color="neutral"
                            variant="ghost"
                            @click="openDetailDialog(option)"
                        />
                    </UTooltip>
                    <UTooltip v-if="option.erlaubtPartnerwahl" text="Partnerwahl">
                        <UButton
                            icon="i-lucide-users"
                            :color="
                                wunschByDefinition.get(option.definitionId)
                                    ? 'success'
                                    : 'neutral'
                            "
                            variant="ghost"
                            @click="openPartnerDialog(option)"
                        />
                    </UTooltip>
                    <UButton
                        icon="i-lucide-plus"
                        color="neutral"
                        variant="ghost"
                        @click="addToRanked(option.definitionId)"
                    />
                </li>
            </ul>
        </div>
    </div>

    <p v-if="ranked.length < katalog.minBelegWuensche" class="text-red-600">
        Insgesamt nur {{ ranked.length }} von {{ katalog.minBelegWuensche }} benötigten Profunda
        gewählt.
    </p>
    <p v-for="[slotId, count] in unterversorgteSlots" :key="slotId" class="text-red-600">
        Slot {{ slotId }}: nur {{ count }} von {{ katalog.minWuenschePerSlot }}
        benötigten Profunda gewählt.
    </p>

    <div class="flex gap-2 mb-4">
        <UButton
            :disabled="ranked.length === 0 || draftBusy"
            color="neutral"
            class="flex-1 justify-center"
            label="Speichern"
            @click="saveDraft"
        />
        <UButton
            :disabled="!maySend"
            class="flex-1 justify-center"
            label="Überprüfen und abgeben"
            @click="send"
        />
    </div>

    <UModal
        v-model:open="partnerDialogOpen"
        :title="`Partnerwahl: ${partnerDialogOption?.bezeichnung ?? ''}`"
    >
        <template #body>
            <template v-if="wunschByDefinition.get(partnerDialogOption?.definitionId)">
                <UAlert color="success" variant="subtle">
                    <template #description>
                        Bestätigte Partnerschaft mit
                        {{
                            wunschByDefinition.get(partnerDialogOption.definitionId).partner
                                .vorname
                        }}
                        {{
                            wunschByDefinition.get(partnerDialogOption.definitionId).partner
                                .nachname
                        }}.
                    </template>
                </UAlert>
                <UButton
                    class="mt-4"
                    label="Partnerschaft auflösen"
                    color="error"
                    :disabled="partnerBusy"
                    @click="
                        dissolveWunsch(
                            wunschByDefinition.get(partnerDialogOption.definitionId).id,
                        )
                    "
                />
            </template>

            <template v-else-if="einladungByDefinition.get(partnerDialogOption?.definitionId)">
                <p>
                    Teile diesen Code mit deiner Wunsch-Partnerin/deinem Wunsch-Partner. Erst
                    wenn sie oder er ihn ebenfalls einträgt, gilt die Partnerschaft als
                    bestätigt und wird beim Matching berücksichtigt.
                </p>
                <div class="flex gap-2 mt-2">
                    <UInput
                        readonly
                        class="w-full"
                        :model-value="
                            einladungByDefinition.get(partnerDialogOption.definitionId).token
                        "
                    />
                    <UTooltip text="Kopieren">
                        <UButton
                            icon="i-lucide-copy"
                            color="neutral"
                            @click="
                                copyToken(
                                    einladungByDefinition.get(partnerDialogOption.definitionId)
                                        .token,
                                )
                            "
                        />
                    </UTooltip>
                </div>
                <UButton
                    class="mt-4"
                    label="Einladung zurückziehen"
                    color="error"
                    variant="ghost"
                    :disabled="partnerBusy"
                    @click="
                        cancelEinladung(
                            einladungByDefinition.get(partnerDialogOption.definitionId).token,
                        )
                    "
                />
            </template>

            <template v-else>
                <p>
                    Ihr könnt euch gegenseitig als Team-Partner für dieses Profundum wählen. Das
                    wird beim Matching nur berücksichtigt, wenn <strong>beide</strong> sich
                    gegenseitig bestätigen.
                </p>

                <h4 class="mt-4 mb-1">Einladung erstellen</h4>
                <UButton
                    label="Code für Partnerin/Partner erzeugen"
                    :disabled="partnerBusy"
                    @click="createEinladung"
                />

                <h4 class="mt-4 mb-1">Einladung annehmen</h4>
                <div class="flex gap-2">
                    <UInput
                        v-model="redeemToken"
                        placeholder="z. B. apfel-baum-schnee"
                        class="w-full"
                    />
                    <UButton
                        label="Annehmen"
                        :disabled="partnerBusy || !redeemToken.trim()"
                        @click="redeemEinladung"
                    />
                </div>
            </template>
        </template>
        <template #footer>
            <UButton label="Schließen" color="neutral" @click="partnerDialogOpen = false" />
        </template>
    </UModal>

    <UModal
        v-model:open="detailDialogOpen"
        :title="detailOption?.bezeichnung ?? ''"
        :ui="{ content: 'max-w-2xl' }"
    >
        <template #body>
            <div class="flex flex-row flex-wrap items-center gap-4 mb-4 text-muted">
                <UBadge v-if="detailOption?.profilProfundum" label="Profil" color="info" />
                <span
                    v-for="fachbereich in detailOption?.fachbereiche"
                    :key="fachbereich"
                    class="inline-flex items-center gap-1"
                >
                    <UIcon name="i-lucide-bookmark" />{{ fachbereich }}
                </span>
                <span
                    v-if="detailOption?.voraussetzungen?.length"
                    class="inline-flex items-center gap-1"
                >
                    <UIcon name="i-lucide-network" />
                    Voraussetzung: {{ detailOption.voraussetzungen.join(', ') }}
                </span>
            </div>

            <h4 class="mt-0 mb-1">Beschreibung</h4>
            <div v-if="detailDescriptionHtml" class="m-trim" v-html="detailDescriptionHtml" />
            <p v-else class="text-muted italic">Keine Beschreibung hinterlegt.</p>

            <template v-if="detailOption?.instanzen?.length">
                <h4 class="mb-1 mt-4">Angebotene Instanzen</h4>
                <UTable :data="detailOption.instanzen" :columns="detailInstanzColumns">
                    <template #termin-cell="{ row }">{{
                        row.original.slotIds.join(', ')
                    }}</template>
                    <template #verantwortlich-cell="{ row }">
                        {{ row.original.verantwortliche.join(', ') || '–' }}
                    </template>
                    <template #maxEinschreibungen-cell="{ row }">
                        {{ row.original.maxEinschreibungen ?? '–' }}
                    </template>
                </UTable>
            </template>
        </template>

        <template #footer>
            <UButton label="Schließen" color="neutral" @click="detailDialogOpen = false" />
        </template>
    </UModal>
</template>

<style scoped></style>
