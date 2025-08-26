<script setup>
import { computed, ref } from 'vue';
import Stepper from 'primevue/stepper';
import StepItem from 'primevue/stepitem';
import Step from 'primevue/step';
import StepPanel from 'primevue/steppanel';
import FloatLabel from 'primevue/floatlabel';
import DatePicker from 'primevue/datepicker';
import { Button, Column, DataTable, Message, MultiSelect, Select, useToast } from 'primevue';
import Form from '@primevue/forms/form';
import { formatDate, formatMachineDate } from '@/helpers/formatters.js';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import { useOtiumStore } from '@/Otium/stores/otium.js';

const toast = useToast();
const router = useRouter();
const otium = useOtiumStore();
await otium.updateBlocks();

const stepperStatus = ref('0');

const schoolyearStart = ref(null);
const schoolyearEnd = ref(null);

const ferien = ref([]);
const newFerien = ref(null);

const weeks = ref([]);

const angeboteProWochentag = ref([
    {
        tag: 'Montag',
        blocksH: [],
        blocksN: [],
    },
    {
        tag: 'Dienstag',
        blocksH: [],
        blocksN: [],
    },
    {
        tag: 'Mittwoch',
        blocksH: [],
        blocksN: [],
    },
    {
        tag: 'Donnerstag',
        blocksH: [],
        blocksN: [],
    },
    {
        tag: 'Freitag',
        blocksH: [],
        blocksN: [],
    },
    {
        tag: 'Samstag',
        blocksH: [],
        blocksN: [],
    },
]);

const ferienSorted = computed(() => {
    return ferien.value.sort((a, b) => {
        if (a.start < b.start) return -1;
        if (a.start > b.start) return 1;
        return 0;
    });
});
const datesDisabled = computed(() => {
    if (!schoolyearStart.value || !schoolyearEnd.value) {
        return [];
    }

    const dates = [];
    let current = schoolyearStart.value;
    while (current < schoolyearEnd.value) {
        // Check if current is in ferien
        if (ferien.value.some((e) => e.start <= current && e.end >= current))
            dates.push(current);

        current = new Date(current);
        current.setDate(current.getDate() + 1);
    }

    return dates;
});
const blocksAvailable = otium.blocks;

function resolveStep1({ values }) {
    const errors = {};

    if (!values.schoolyearStart)
        errors.schoolyearStart = [
            { message: 'Bitte geben Sie das Anreisedatum nach den Sommerferien an.' },
        ];

    if (!values.schoolyearEnd)
        errors.schoolyearEnd = [
            { message: 'Bitte geben Sie das Abreisedatum in die Sommerferien an.' },
        ];

    if (
        values.schoolyearStart &&
        values.schoolyearEnd &&
        values.schoolyearEnd <= values.schoolyearStart
    )
        errors.schoolyearEnd = [
            { message: 'Das Abreisedatum muss nach dem Anreisedatum liegen.' },
        ];

    return { values, errors };
}

function form1Submit({ valid }) {
    if (!valid) return;

    stepperStatus.value = '2';
}

function addFerien() {
    if (!newFerien.value) return;

    const start = newFerien.value[0];
    const end = newFerien.value[1] ?? new Date(start);

    if (!start || start > end) {
        return;
    }

    // Weird workaround for timezone issues
    end.setHours(12);
    ferien.value.push({ start, end });
    newFerien.value = null;
}

function form2Submit() {
    const tempWeeks = [];

    let current = new Date(schoolyearStart.value);
    current.setDate(current.getDate() - current.getDay() + 1); // Set to Monday

    while (current < schoolyearEnd.value) {
        const weekStart = new Date(current);
        const weekEnd = new Date(current);
        weekEnd.setDate(weekEnd.getDate() + 6);

        const hasHoliday = ferien.value.some((e) => {
            const startsInWeek = e.start >= weekStart && e.start <= weekEnd;
            const endsInWeek = e.end >= weekStart && e.end <= weekEnd;
            return (startsInWeek || endsInWeek) && !(startsInWeek && endsInWeek); // XOR to ignore one-day holidays
        });

        const isInHoliday = ferien.value.some((e) => {
            return e.start <= weekStart && e.end >= weekEnd;
        });

        tempWeeks.push({
            start: weekStart,
            end: weekEnd,
            type: isInHoliday ? 'F' : hasHoliday ? 'H' : 'N',
        });

        current.setDate(current.getDate() + 7); // add 7 days to get the next week
    }

    weeks.value = tempWeeks;
    stepperStatus.value = '3';
}

async function submit() {
    const wochentypen = { H: 'H-Woche', N: 'N-Woche' };
    const data = [];

    // This is weird a workaround for the fact that js handles dates in the local timezone but upon ISO string conversion transforms them to UTC.
    const start = new Date(schoolyearStart.value).setHours(12);
    const end = new Date(schoolyearEnd.value).setHours(12);

    const current = new Date(start);
    while (current < end) {
        if (current.getDay() === 0) {
            current.setDate(current.getDate() + 1);
            continue;
        }

        // Check if the current date is in a holiday
        const isInHoliday = ferien.value.some((e) => {
            return e.start <= current && e.end >= current;
        });

        if (isInHoliday) {
            current.setDate(current.getDate() + 1);
            continue;
        }

        // Find the week for the current date
        const week = weeks.value.find((w) => {
            return current >= w.start && current <= w.end;
        });

        if (!week) {
            current.setDate(current.getDate() + 1);
            console.error('Could not find Week', current);
            continue;
        }

        // Find the day of the week
        const dayOfWeek = current.getDay();

        // Find the blocks for the day of the week
        const angeboteAmWochentag = angeboteProWochentag.value.find(
            (d) =>
                d.tag ===
                [
                    'Sonntag',
                    'Montag',
                    'Dienstag',
                    'Mittwoch',
                    'Donnerstag',
                    'Freitag',
                    'Samstag',
                ][dayOfWeek],
        );
        const blocks =
            week.type === 'H' ? angeboteAmWochentag.blocksH : angeboteAmWochentag.blocksN;

        data.push({
            datum: formatMachineDate(current),
            wochentyp: wochentypen[week.type],
            blocks: blocks,
        });

        current.setDate(current.getDate() + 1);
    }

    const api = mande('/api/management/schuljahr');
    try {
        await api.post(data);
        toast.add({
            severity: 'success',
            summary: 'Erfolg',
            detail: 'Die Termine wurden erfolgreich gespeichert.',
            life: 15000,
        });
        router.push({ name: 'Verwaltung' });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Termine konnten nicht gespeichert werden.',
        });
    }
}
</script>

<template>
    <h1>Schuljahr planen</h1>
    <Stepper v-model:value="stepperStatus" linear>
        <StepItem value="0">
            <Step>Start</Step>
            <StepPanel v-slot="{ activateCallback }">
                <p>Dieses Programm hilf ihnen, ihr Schuljahr vorauszuplanen.</p>
                <Button fluid label="Start" @click="() => activateCallback('1')" />
            </StepPanel>
        </StepItem>
        <StepItem value="1">
            <Step>Aufbau des Schuljahrs</Step>
            <StepPanel>
                <!--
        Enter schoolyear timeframe
        -->
                <p>Bitte geben sie Start und Ende des Schuljahres an.</p>
                <Form
                    v-slot="$form"
                    :resolver="resolveStep1"
                    class="flex flex-col gap-4"
                    @submit="form1Submit"
                >
                    <div class="w-full">
                        <FloatLabel variant="on">
                            <DatePicker
                                id="schoolyearStart"
                                v-model="schoolyearStart"
                                date-format="dd.mm.yy"
                                fluid
                                name="schoolyearStart"
                                select-other-months
                                show-icon
                            />
                            <label for="schoolyearStart">Anreise nach den Sommerferien</label>
                        </FloatLabel>
                        <Message
                            v-if="$form.schoolyearStart?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.schoolyearStart.error.message }}
                        </Message>
                    </div>
                    <div class="w-full">
                        <FloatLabel variant="on">
                            <DatePicker
                                id="schoolyearEnd"
                                v-model="schoolyearEnd"
                                date-format="dd.mm.yy"
                                fluid
                                name="schoolyearEnd"
                                select-other-months
                                show-icon
                            />
                            <label for="schoolyearEnd">Abreise in die Sommerferien</label>
                        </FloatLabel>
                        <Message
                            v-if="$form.schoolyearEnd?.invalid"
                            severity="error"
                            size="small"
                            variant="simple"
                        >
                            {{ $form.schoolyearEnd.error.message }}
                        </Message>
                    </div>
                    <Button label="Weiter" type="submit" />
                </Form>
            </StepPanel>
        </StepItem>
        <StepItem value="2">
            <Step>Ferien</Step>
            <StepPanel>
                <div class="flex flex-col gap-4">
                    <p>Bitte geben sie alle Schulferien und Feiertage an.</p>
                    <div class="flex w-full gap-4 justify-stretch">
                        <FloatLabel class="w-full" variant="on">
                            <DatePicker
                                v-model="newFerien"
                                :disabled-dates="datesDisabled"
                                :max-date="schoolyearEnd"
                                :min-date="schoolyearStart"
                                class="w-full"
                                date-format="dd.mm.yy"
                                select-other-months
                                selection-mode="range"
                                show-button-bar
                                show-icon
                            />
                            <label>Schulferien (Abreise - Anreise oder Feiertag)</label>
                        </FloatLabel>
                        <Button label="Hinzufügen" @click="addFerien" />
                    </div>
                    <DataTable :value="ferienSorted">
                        <Column header="Start">
                            <template #body="{ data }">
                                {{ formatDate(data.start) }}
                            </template>
                        </Column>
                        <Column header="Ende">
                            <template #body="{ data }">
                                {{ formatDate(data.end) }}
                            </template>
                        </Column>
                        <Column class="afra-col-action text-right">
                            <template #body="{ data }">
                                <Button
                                    icon="pi pi-trash"
                                    severity="danger"
                                    variant="text"
                                    @click="
                                        () => {
                                            const index = ferien.indexOf(data);
                                            if (index > -1) {
                                                ferien.splice(index, 1);
                                            } else alert('Shit');
                                        }
                                    "
                                />
                            </template>
                        </Column>
                        <template #empty>
                            <div class="flex justify-center">
                                Sie können oben Ferientermine angeben.
                            </div>
                        </template>
                    </DataTable>
                    <div class="grid grid-cols-[1fr_4fr] gap-4">
                        <Button
                            label="Zurück"
                            severity="secondary"
                            @click="() => (stepperStatus.value = '1')"
                        />
                        <Button fluid label="Weiter" @click="() => form2Submit()" />
                    </div>
                </div>
            </StepPanel>
        </StepItem>
        <StepItem value="3">
            <Step>H-/ N-Wochen</Step>
            <StepPanel>
                <p>
                    Wir haben automatisch versucht die H-/ N-Wochen vorherzusagen. Bitte
                    kontrollieren Sie die Vorhersagen und passen Sie ggf. den Typ an.
                </p>
                <DataTable :value="weeks">
                    <Column header="Start">
                        <template #body="{ data }">
                            {{ formatDate(data.start) }}
                        </template>
                    </Column>
                    <Column header="Ende">
                        <template #body="{ data }">
                            {{ formatDate(data.end) }}
                        </template>
                    </Column>
                    <Column header="Typ">
                        <template #body="{ data }">
                            <Select
                                v-if="data.type === 'F'"
                                v-model="data.type"
                                :options="[{ label: 'Ferien', value: 'F' }]"
                                disabled
                                fluid
                                option-label="label"
                                option-value="value"
                            />
                            <Select
                                v-else
                                v-model="data.type"
                                :options="[
                                    { label: 'H-Woche', value: 'H' },
                                    { label: 'N-Woche', value: 'N' },
                                ]"
                                fluid
                                option-label="label"
                                option-value="value"
                            />
                        </template>
                    </Column>
                </DataTable>
                <Button
                    class="mt-4"
                    fluid
                    label="Weiter"
                    @click="() => (stepperStatus = '4')"
                />
            </StepPanel>
        </StepItem>
        <StepItem value="4">
            <Step>Blöcke</Step>
            <StepPanel>
                <p>Bitte ordnen Sie den Wochentagen die stattfindenden Blöcke zu.</p>
                <DataTable :value="angeboteProWochentag">
                    <Column field="tag" header="Wochentag" />
                    <Column header="H-Woche">
                        <template #body="{ data }">
                            <MultiSelect
                                v-model="data.blocksH"
                                :options="blocksAvailable"
                                :showToggleAll="false"
                                fluid
                                option-label="bezeichnung"
                                option-value="schemaId"
                                placeholder="Keine Blöcke gewählt"
                            />
                        </template>
                    </Column>
                    <Column header="N-Woche">
                        <template #body="{ data }">
                            <MultiSelect
                                v-model="data.blocksN"
                                :options="blocksAvailable"
                                :showToggleAll="false"
                                fluid
                                option-label="bezeichnung"
                                option-value="schemaId"
                                placeholder="Keine Blöcke gewählt"
                            />
                        </template>
                    </Column>
                </DataTable>
                <Button class="mt-4" fluid label="Abschließen" @click="submit" />
            </StepPanel>
        </StepItem>
    </Stepper>
</template>

<style scoped></style>
