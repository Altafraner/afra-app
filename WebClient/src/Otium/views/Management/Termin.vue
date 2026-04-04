<script setup>
import { computed, ref } from 'vue';
import { useUser } from '@/stores/user';
import {
    Button,
    FloatLabel,
    InputNumber,
    InputText,
    Textarea,
    ToggleSwitch,
    useToast,
} from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { formatDate, formatPerson } from '@/helpers/formatters';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import PersonSelector from '@/components/PersonSelector.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import HybridAttendanceTable from '@/Attendance/components/HybridAttendanceTable.vue';

const props = defineProps({
    terminId: String,
});

const loading = ref(true);
const user = useUser();
const toast = useToast();
const { openConfirmDialog } = useConfirmPopover();
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
        route: {
            name: 'Katalog',
        },
    },
    {
        label: 'Verwaltung',
        route: {
            name: 'Verwaltung',
        },
    },
    {
        label: otium.value != null ? otium.value.otium : '',
        route:
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
                ? `${formatDate(new Date(otium.value.datum))} ${otium.value.block}. Block`
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
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden der Daten aufgetreten.',
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
            severity: 'error',
            summary: 'Fehler',
            detail: errmsg,
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

const initRemove = async (evt, student) => {
    openConfirmDialog(evt, remove.bind(this, student), 'Schüler:in ausschreiben?');

    async function remove(student) {
        const api = mande(`/api/otium/management/termin/${props.terminId}/student`);
        await api.post({ value: student.id });
        await fetchData();
    }
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
                <FloatLabel class="w-full" variant="on">
                    <InputText id="ort" v-model="ort" fluid maxlength="20" name="ort" />
                    <label for="ort">Ort</label>
                </FloatLabel>
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
                    <div class="flex justify-between">
                        <label for="betreuerSwitch">Betreuer:in zuweisen</label>
                        <ToggleSwitch v-model="betreuerZuweisenSelected" id="betreuerSwitch" />
                    </div>
                    <PersonSelector
                        v-model="personSelected"
                        :disabled="!betreuerZuweisenSelected"
                        name="tutor"
                        required
                    />
                </div>
            </template>
        </GridEditRow>
        <GridEditRow
            header="max. Teilnehner:innen"
            :hide-edit="!mayEdit"
            @edit="startEditMaxEnrollments"
            @update="updateMaxEnrollments"
        >
            <template #body>
                {{ otium.maxEinschreibungen ?? 'Unbegrenzt' }}
            </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <div class="flex justify-between">
                        <label for="maxEnrollmentSwitch"
                            >Teilnehmer:innen-Zahl beschränken</label
                        >
                        <ToggleSwitch
                            v-model="maxEnrollmentsSetzenSelected"
                            id="maxEnrollmentSwitch"
                        />
                    </div>
                    <FloatLabel v-if="maxEnrollmentsSetzenSelected" class="w-full" variant="on">
                        <InputNumber
                            id="maxEnrollmentInput"
                            v-model="maxEnrollmentsSelected"
                            :disabled="!maxEnrollmentsSetzenSelected"
                            fluid
                            name="maxEnrollments"
                        />
                        <label for="maxEnrollmentInput">max. Teilnehmer:innen</label>
                    </FloatLabel>
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
                    <div class="flex justify-between">
                        <label for="bezeichnungSwitch">Bezeichnung überschreiben</label>
                        <ToggleSwitch v-model="bezeichnungSelected" id="bezeichnungSwitch" />
                    </div>
                    <FloatLabel v-if="bezeichnungSelected" class="w-full" variant="on">
                        <InputText id="bezeichnung" v-model="bezeichnung" fluid />
                        <label for="bezeichnung">Bezeichnung</label>
                    </FloatLabel>
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
                <p
                    v-for="line in otium.beschreibung.split('\n')"
                    :key="line"
                    class="first:mt-0"
                >
                    {{ line }}
                </p>
            </template>
            <template #body v-else> Unverändert </template>
            <template #edit>
                <div class="w-full flex flex-col gap-3">
                    <div class="flex justify-between">
                        <label for="beschreibungSwitch">Beschreibung überschreiben</label>
                        <ToggleSwitch v-model="beschreibungSelected" id="beschreibungSwitch" />
                    </div>
                    <FloatLabel v-if="beschreibungSelected" class="w-full" variant="on">
                        <Textarea
                            id="beschreibung"
                            v-model="beschreibung"
                            auto-resize
                            fluid
                            maxlength="500"
                            rows="2"
                        />
                        <label for="beschreibung">Beschreibung</label>
                    </FloatLabel>
                </div>
            </template>
        </GridEditRow>
    </grid>
    <div class="flex justify-between items-baseline gap-3 flex-wrap mt-3 mb-1">
        <h2>Einschreibungen</h2>
        <template v-if="otium.isSupervisionEnabled || user.isOtiumsverantwortlich">
            <Button
                v-if="!aufsichtRunning"
                icon="pi pi-eye"
                label="Anwesenheitskontrolle"
                severity="secondary"
                @click="startAufsicht"
            />
            <Button
                v-else
                icon="pi pi-stop"
                label="Anwesenheitskontrolle abschließen"
                severity="success"
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
            <Button
                v-tooltip="'Ausschreiben'"
                aria-label="Ausschreiben"
                icon="pi pi-times"
                severity="danger"
                size="small"
                variant="text"
                @click="(evt) => initRemove(evt, data.student)"
            />
        </template>
    </HybridAttendanceTable>
</template>

<style scoped></style>
