<script setup>
import { computed, ref } from 'vue';
import { useUser } from '@/stores/user';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { formatDate, formatPerson } from '@/helpers/formatters';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import HybridAttendanceTable from '@/Attendance/components/HybridAttendanceTable.vue';
import { convertMarkdownToHtml } from '@/composables/markdown.ts';

const props = defineProps({
    terminId: String,
});

const loading = ref(true);
const user = useUser();
const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const otium = ref(null);

const aufsichtRunning = ref(false);

const maxEnrollmentsSetzenSelected = ref(false);
const maxEnrollmentsSelected = ref(null);
const betreuerZuweisenSelected = ref(false);
const ort = ref();
const bezeichnung = ref();
const bezeichnungSelected = ref();
const beschreibung = ref('');
const beschreibungSelected = ref();
const personSelected = ref(null);

const navItems = computed(() => [
    {
        label: 'Otium',
        to: {
            name: 'Otium-Katalog',
        },
    },
    {
        label: 'Verwaltung',
        to: {
            name: 'Verwaltung',
        },
    },
    {
        label: otium.value != null ? otium.value.otium : '',
        to:
            otium.value != null
                ? {
                      name: 'Verwaltung-Otium',
                      params: {
                          otiumId: otium.value.otiumId,
                      },
                  }
                : null,
    },
    {
        label:
            otium.value != null
                ? `${formatDate(new Date(otium.value.datum))} ${otium.value.block}`
                : '',
    },
]);

const mayEdit = computed(() => user.isOtiumsverantwortlich);

async function fetchData() {
    loading.value = true;
    const dataGetter = mande('/api/otium/management/termin/' + props.terminId);
    try {
        otium.value = await dataGetter.get();
    } catch (e) {
        await user.update();
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Laden der Daten aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

async function updateMaxEnrollments() {
    await simpleUpdate(
        'maxEinschreibungen',
        maxEnrollmentsSetzenSelected.value ? maxEnrollmentsSelected.value : null,
        'Es ist ein Fehler beim Aktualisieren der maximalen Teilnehmerzahl aufgetreten.',
    );
}

async function updateTutor() {
    await simpleUpdate(
        'tutor',
        betreuerZuweisenSelected.value ? personSelected.value : null,
        'Es ist ein Fehler beim Aktualisieren des Tutors aufgetreten.',
    );
}

async function updateOrt() {
    await simpleUpdate(
        'ort',
        ort.value,
        'Es ist ein Fehler beim Aktualisieren des Ortes aufgetreten.',
    );
}

async function updateBezeichnung() {
    await simpleUpdate(
        'bezeichnung',
        bezeichnungSelected.value ? bezeichnung.value : null,
        'Es ist ein Fehler beim Aktualisieren der Bezeichnung aufgetreten.',
    );
}

async function updateBeschreibung() {
    await simpleUpdate(
        'beschreibung',
        beschreibungSelected.value ? beschreibung.value : null,
        'Es ist ein Fehler beim Aktualisieren der Beschreibung aufgetreten.',
    );
}

async function simpleUpdate(name, value, errmsg) {
    const api = mande(`/api/otium/management/termin/${props.terminId}/${name}`);
    try {
        await api.patch({ value });
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: errmsg,
        });
        console.error(e);
    } finally {
        await fetchData();
    }
}

function startAufsicht() {
    if (aufsichtRunning.value) return;
    aufsichtRunning.value = true;
}

function stopAufsicht() {
    aufsichtRunning.value = false;
}

const startEditMaxEnrollments = () => {
    maxEnrollmentsSetzenSelected.value = otium.value.maxEinschreibungen !== null;
    maxEnrollmentsSelected.value = otium.value.maxEinschreibungen;
};

const startEditTutor = () => {
    betreuerZuweisenSelected.value = otium.value.tutor !== null;
    personSelected.value = betreuerZuweisenSelected.value ? otium.value.tutor.id : null;
};

const startEditOrt = () => {
    ort.value = otium.value.ort;
};

const startEditBezeichnung = () => {
    bezeichnungSelected.value = otium.value.bezeichnung !== null;
    bezeichnung.value = otium.value.bezeichnung;
};

const startEditBeschreibung = () => {
    beschreibungSelected.value = otium.value.beschreibung !== null;
    beschreibung.value = otium.value.beschreibung;
};

const initRemove = async (student) => {
    if (
        !(await requireConfirm(
            'Möchten Sie die Schüler:in wirklich ausschreiben?',
            'Schüler:in ausschreiben?',
        ))
    )
        return;

    const api = mande(`/api/otium/management/termin/${props.terminId}/student`);
    await api.post({ value: student.id });
    await fetchData();
};

await fetchData();
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>
        {{ otium.bezeichnung ?? otium.otium }}
    </h1>
    <grid>
        <GridEditRow header="Datum" hide-edit>
            <template #body>
                {{ formatDate(new Date(otium.datum)) }}
            </template>
        </GridEditRow>
        <GridEditRow header="Block" hide-edit>
            <template #body>
                {{ otium.block }}
            </template>
        </GridEditRow>
        <GridEditRow
            :hide-edit="!mayEdit"
            header="Ort"
            @edit="startEditOrt"
            @update="updateOrt"
        >
            <template #body>
                {{ otium.ort }}
            </template>
            <template #edit>
                <UFormField label="Ort" required>
                    <UInput v-model="ort" :maxlength="20" class="w-full" />
                </UFormField>
            </template>
        </GridEditRow>
        <GridEditRow
            :hide-edit="!mayEdit"
            header="Betreuer:in"
            @edit="startEditTutor"
            @update="updateTutor"
        >
            <template #body>
                <template v-if="otium.tutor === null"> Kein:e Betreuer:in </template>
                <template v-else>
                    {{ formatPerson(otium.tutor) }}
                </template>
            </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <USwitch v-model="betreuerZuweisenSelected" label="Betreuer:in zuweisen" />
                    <PersonSelectorNuxt
                        v-model="personSelected"
                        :disabled="!betreuerZuweisenSelected"
                        name="tutor"
                        required
                    />
                </div>
            </template>
        </GridEditRow>
        <GridEditRow
            header="max. Teilnehmer:innen"
            :hide-edit="!mayEdit"
            @edit="startEditMaxEnrollments"
            @update="updateMaxEnrollments"
        >
            <template #body>
                {{ otium.maxEinschreibungen ?? 'Unbegrenzt' }}
            </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <USwitch
                        v-model="maxEnrollmentsSetzenSelected"
                        label="Teilnehmer:innen-Zahl beschränken"
                    />
                    <UFormField
                        v-if="maxEnrollmentsSetzenSelected"
                        label="Max. Teilnehmer:innen"
                    >
                        <UInputNumber
                            v-model="maxEnrollmentsSelected"
                            :min="1"
                            class="w-full"
                            placeholder="Max. Teilnehmer:innen-Zahl eingeben"
                        />
                    </UFormField>
                </div>
            </template>
        </GridEditRow>
        <GridEditRow
            header="Bezeichnung (Termin)"
            :hide-edit="!mayEdit"
            @edit="startEditBezeichnung"
            @update="updateBezeichnung"
        >
            <template #body>
                {{ otium.bezeichnung ?? 'Unverändert' }}
            </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <USwitch v-model="bezeichnungSelected" label="Bezeichnung überschreiben" />
                    <UFormField v-if="bezeichnungSelected" label="Bezeichnung">
                        <UInput
                            v-model="bezeichnung"
                            class="w-full"
                            maxlength="70"
                            placeholder="Bezeichnung eingeben"
                        />
                    </UFormField>
                </div>
            </template>
        </GridEditRow>
        <GridEditRow
            header="Beschreibung (Termin)"
            :hide-edit="!mayEdit"
            @edit="startEditBeschreibung"
            @update="updateBeschreibung"
        >
            <template v-if="otium.beschreibung" #body>
                <div v-html="convertMarkdownToHtml(otium.beschreibung)" />
            </template>
            <template #body v-else> Unverändert </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <USwitch
                        v-model="beschreibungSelected"
                        label="Beschreibung überschreiben"
                    />
                    <UFormField
                        v-if="beschreibungSelected"
                        label="Beschreibung"
                        name="beschreibung"
                    >
                        <UTextarea
                            v-model="beschreibung"
                            :maxlength="500"
                            :rows="2"
                            autoresize
                            class="w-full"
                            placeholder="Beschreibung eingeben"
                        />
                    </UFormField>
                </div>
            </template>
        </GridEditRow>
    </grid>
    <div class="flex justify-between items-baseline gap-3 flex-wrap mt-3 mb-1">
        <h2>Einschreibungen</h2>
        <template v-if="otium.isSupervisionEnabled || user.isOtiumsverantwortlich">
            <UButton
                v-if="!aufsichtRunning"
                color="primary"
                label="Anwesenheitskontrolle"
                icon="i-lucide-eye"
                @click="startAufsicht"
            />
            <UButton
                v-else
                color="success"
                label="Anwesenheitskontrolle abschließen"
                icon="i-lucide-square"
                @click="stopAufsicht"
            />
        </template>
    </div>
    <HybridAttendanceTable
        :enable-supervision="aufsichtRunning"
        :event-id="props.terminId"
        :show-attendance="otium.isDoneOrRunning"
        :slot-id="otium.blockId"
        scope="otium"
        :enrollments="otium.einschreibungen"
        @update-attendance="(data) => (otium.einschreibungen = data)"
    >
        <template v-if="!aufsichtRunning && !otium.isDoneOrRunning" #studentActions="{ data }">
            <UTooltip text="Ausschreiben">
                <UButton
                    aria-label="Ausschreiben"
                    color="error"
                    icon="i-lucide-x"
                    variant="ghost"
                    @click="() => initRemove(data.student)"
                />
            </UTooltip>
        </template>
    </HybridAttendanceTable>
</template>

<style scoped></style>
