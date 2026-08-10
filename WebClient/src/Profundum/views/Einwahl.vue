<script setup>
import { computed, h, ref, watch } from 'vue';
import { mande } from 'mande';
import { useSortable } from '@vueuse/integrations/useSortable';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { convertMarkdownToHtml } from '@/composables/markdown';
import EinwahlSingleProfundum from '@/Profundum/views/EinwahlSingleProfundum.vue';

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
const uncommitedChanges = ref(false);
const ranked = ref([]);
const draftBusy = ref(false);

useSortable('.ranked-list', ranked, { animation: 150, handle: '.drag-handle' });

watch(
    ranked,
    () => {
        uncommitedChanges.value = true;
    },
    {
        deep: true,
    },
);

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
        uncommitedChanges.value = false;
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

    uncommitedChanges.value = false;
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

await startup();
</script>

<template>
    <nav-breadcrumb :items="navItems" />

    <h1>Profundums-Einwahl</h1>

    <h2>Hinweise</h2>

    <UAlert color="info" icon="i-lucide-info" title="Hinweis">
        <template #description>
            <p class="mt-0">Bitte lese dir die folgenden Hinweise aufmerksam durch.</p>
            <p class="mb-0">
                Sie erklären den Prozess der Einwahl sowie notwendige Kriterien, die du erfüllen
                musst, damit deine Wünsche berücksichtigt werden können.
            </p>
        </template>
    </UAlert>

    <template v-if="katalog.fixiert.length > 0">
        <h3>Bereits festgelegte Belegungen</h3>

        <p>
            Hier siehst du, welche Belegungen für dich bereits hinterlegt sind. Du kannst diese
            nicht ändern. Solltes du mit den Belegungen nicht einverstanden sein, melde dich
            bitte bei der Verantwortlichen Person für die Profundumseinwahl.
        </p>

        <UTable
            :columns="[
                { header: 'Slot', accessorKey: 'slotLabel' },
                { header: 'Angebot', accessorKey: 'bezeichnung' },
            ]"
            :data="katalog.fixiert"
            :ui="{
                td: 'p-2',
                th: 'p-2',
            }"
        >
        </UTable>
    </template>

    <h3>Auswahlverfahren</h3>

    <p>
        Unten siehst du eine Liste mit den für dich verfügbaren Angeboten. Aus diesen kannst du
        über das <UIcon class="inline-block" name="i-lucide-plus" /> Plus Angebote auswählen,
        die du dir wünschst.
    </p>

    <p>
        Mit den Knöpfen <UIcon class="inline-block" name="i-lucide-arrow-up" /> Hoch und
        <UIcon class="inline-block" name="i-lucide-arrow-down" /> Runter kannst du eine
        Rangfolge deiner Wünsche erstellen. Wünsche weiter oben in der Liste werden dabei eher
        beachtet, als Wünsche weiter unten. Bei Profunda die du nicht in deine Wunschliste
        aufnimmst gehen wir davon aus, dass du diese nicht belegen willst.
    </p>

    <p>
        Für jedes Profundum kannst du über den
        <UIcon class="inline-block" name="i-lucide-info" /> Info-Knopf oder aus dem
        <em>Profundarium</em>
        weitere Infos erhalten.
    </p>

    <UAlert icon="i-lucide-info" title="Mindestanzahlen beachten">
        <template #description>
            <p class="mt-0">
                Damit alle eine Chance haben in Angebote zu kommen, die sie sich gewünscht
                haben, musst du mindestens eine bestimmte Anzahl an Angeboten in deine
                Wunschliste aufnehmen.
            </p>
            <p>Das sind</p>
            <ul>
                <li>
                    Mindestens
                    <strong>{{ katalog.minBelegWuensche }} Profunda insgesamt</strong> und
                </li>
                <li>
                    Mindestens
                    <strong>{{ katalog.minWuenschePerSlot }} Profunda für jeden Slot,</strong>
                    für den du dich einwählen musst.
                </li>
            </ul>
            <p class="mb-0">
                Weitere Einschränkungen werden dir möglicherweise angezeigt, nachdem du auf
                "abgeben" geklickt hast. Nimm in diesem Fall bitte entsprechende Änderungen vor
                und versuche es erneut.
            </p>
        </template>
    </UAlert>

    <p>
        Für manche Profunda hast du die Möglichkeit eine <strong>Partner:in zu wählen</strong>.
        Nutze dazu den <UIcon class="inline-block" name="i-lucide-user" /> Person-Knopf. Wenn du
        eine Partner:in hast, dann sollte eine:r von euch beiden dort einen Partner:innen-Code
        generieren. Diesen muss die andere Partner:in dann eingeben. Nur, wenn ein Code
        eingegeben wurde, können wir euren Wunsch berücksichtigen, zusammen eingewählt zu
        werden.
    </p>

    <h3>Auswertung</h3>

    <p>
        Nach dem Zeitfenster zur Einwahl berechnen wir aus den abgegebenen Wünschen eine
        Belegung, die eure Präferenzen bestmöglich berücksichtigt. Falls es dich interessiert,
        kannst du
        <a
            class="text-blue-500 hover:underline cursor-pointer"
            href="https://github.com/Altafraner/afra-app"
            target="_blank"
            >im Quellcode dieses Programms</a
        >
        sogar nachlesen, wie wir das machen.
    </p>

    <p>
        Solange du deine Wünsche fristgerecht abgibst, spielt es keine Rolle, ob du deine
        Wünsche früher oder später als deine Mitschüler:innen abgibst.
    </p>

    <p>
        Die berechnete Belegung bildet dann die Grundlage für die letztendliche Entscheidung
        über die Einwahlergebnise, die von den verantwortlichen Lehrer:innen getroffen wird.
    </p>

    <USeparator class="my-6" size="lg" />

    <h2>Deine Wünsche</h2>

    <div class="grid grid-cols-1 md:grid-cols-1 gap-6 mb-4">
        <div>
            <h3 class="flex items-center gap-2">
                Deine Rangfolge
                <UBadge
                    v-if="katalog.aktuelleWuensche.length > 0 && !uncommitedChanges"
                    :label="katalog.istAbgegeben ? 'Abgegeben' : 'Entwurf gespeichert'"
                    :color="katalog.istAbgegeben ? 'success' : 'warning'"
                />
            </h3>
            <ol class="ranked-list flex flex-col gap-2 list-none pl-0">
                <EinwahlSingleProfundum
                    v-for="(id, index) in ranked"
                    :key="id"
                    :angebot="optionenById.get(id)"
                    :has-partner="wunschByDefinition.get(id) != null"
                    :index="index"
                    :is-ranked="true"
                    :last-in-list="index === ranked.length - 1"
                    @openPartnerDialog="openPartnerDialog"
                    @move-down="moveDown"
                    @move-up="moveUp"
                    @open-detail-dialog="openDetailDialog"
                    @remove-from-ranked="removeFromRanked"
                />
            </ol>
            <div v-if="ranked.length === 0" class="w-full text-center text-muted text-sm">
                Keine Profunda ausgewählt.
            </div>
        </div>

        <div>
            <h3>belegbare Profunda außerhalb der Rangfolge</h3>
            <ul class="flex flex-col gap-2 list-none pl-0">
                <EinwahlSingleProfundum
                    v-for="option in verfuegbareOptionen"
                    :key="option.definitionId"
                    :angebot="option"
                    :has-partner="wunschByDefinition.get(option.definitionId) != null"
                    :is-ranked="false"
                    @open-detail-dialog="openDetailDialog"
                    @open-partner-dialog="openPartnerDialog"
                    @add-to-ranked="addToRanked"
                />
            </ul>
            <div
                v-if="verfuegbareOptionen.length === 0"
                class="w-full text-center text-muted text-sm"
            >
                Keine weiteren Profunda verfügbar.
            </div>
        </div>

        <UAlert
            v-if="ranked.length < katalog.minBelegWuensche || unterversorgteSlots.length > 0"
            color="error"
            icon="i-lucide-circle-x"
            title="Nicht ausreichend Profunda gewählt"
            variant="subtle"
        >
            <template #description>
                <p v-if="ranked.length < katalog.minBelegWuensche">
                    Insgesamt nur {{ ranked.length }} von
                    {{ katalog.minBelegWuensche }} benötigten Profunda gewählt.
                </p>
                <div class="grid grid-cols-[auto_1fr] gap-x-1">
                    <template v-for="[slotId, count] in unterversorgteSlots" :key="slotId">
                        <span>Slot {{ slotId }}:</span>
                        <span>
                            nur {{ count }} von {{ katalog.minWuenschePerSlot }} benötigten
                            Profunda gewählt.</span
                        >
                    </template>
                </div>
            </template>
        </UAlert>

        <div class="flex gap-2 mb-4">
            <UButton
                :disabled="ranked.length === 0 || draftBusy"
                class="flex-1 justify-center"
                color="neutral"
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
    </div>

    <UModal
        v-model:open="partnerDialogOpen"
        :title="`Partnerwahl: ${partnerDialogOption?.bezeichnung ?? ''}`"
        :ui="{
            footer: 'justify-end',
        }"
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
                    class="mt-4 w-full"
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
                    bestätigt und wird berücksichtigt.
                </p>
                <UFieldGroup class="mt-2 w-full">
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
                            color="primary"
                            @click="
                                copyToken(
                                    einladungByDefinition.get(partnerDialogOption.definitionId)
                                        .token,
                                )
                            "
                        />
                    </UTooltip>
                </UFieldGroup>
                <UButton
                    class="mt-4 w-full"
                    label="Einladung zurückziehen"
                    color="error"
                    variant="subtle"
                    :disabled="partnerBusy"
                    @click="
                        cancelEinladung(
                            einladungByDefinition.get(partnerDialogOption.definitionId).token,
                        )
                    "
                />
            </template>

            <div v-else class="flex flex-col gap-4">
                <p>
                    Ihr könnt euch gegenseitig als Team-Partner für dieses Profundum wählen.
                    Dazu muss einer von euch hier einen Code generieren, und der/die andere
                    diesen bei sich eingeben.
                </p>
                <UFormField label="Einladung annehmen">
                    <UFieldGroup class="w-full">
                        <UInput
                            v-model="redeemToken"
                            class="w-full"
                            placeholder="z. B. apfel-baum-schnee"
                        />
                        <UButton
                            :disabled="partnerBusy || !redeemToken.trim()"
                            label="Annehmen"
                            @click="redeemEinladung"
                        />
                    </UFieldGroup>
                </UFormField>
                <UFormField label="Einladung erstellen">
                    <UButton
                        :disabled="partnerBusy"
                        label="Code für Partnerin/Partner erzeugen"
                        @click="createEinladung"
                        class="w-full"
                    />
                </UFormField>
            </div>
        </template>
        <template #footer>
            <UButton
                color="neutral"
                icon="i-lucide-x"
                label="Schließen"
                variant="soft"
                @click="partnerDialogOpen = false"
            />
        </template>
    </UModal>

    <UModal
        v-model:open="detailDialogOpen"
        :title="detailOption?.bezeichnung ?? ''"
        :ui="{ content: 'max-w-2xl' }"
    >
        <template #description>
            <span class="flex flex-row flex-wrap items-center gap-4 text-muted">
                <UBadge v-if="detailOption?.profilProfundum" color="info" label="Profil" />
                <span
                    v-for="fachbereich in detailOption?.fachbereiche"
                    :key="fachbereich"
                    class="inline-flex items-center gap-1"
                >
                    <UIcon name="i-lucide-bookmark" />{{ fachbereich }}
                </span>
            </span>
        </template>
        <template #body>
            <template v-if="detailOption?.voraussetzungen?.length">
                <h4><UIcon class="inline-block" name="i-lucide-network" />Voraussetzungen</h4>
                <ul class="mb-2">
                    <li v-for="element in detailOption.voraussetzungen">{{ element }}</li>
                </ul>
            </template>
            <div v-if="detailDescriptionHtml" class="m-trim" v-html="detailDescriptionHtml" />
            <p v-else class="text-muted italic">Keine Beschreibung hinterlegt.</p>

            <template v-if="detailOption?.instanzen?.length">
                <USeparator class="my-4" size="sm" />
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
            <div class="flex justify-end w-full">
                <UButton
                    color="neutral"
                    icon="i-lucide-x"
                    label="Schließen"
                    variant="soft"
                    @click="detailDialogOpen = false"
                />
            </div>
        </template>
    </UModal>
</template>

<style scoped>
h3 {
    margin-top: calc(6 * var(--spacing));
}
</style>
