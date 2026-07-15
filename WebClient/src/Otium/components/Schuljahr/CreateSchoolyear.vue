<script lang="ts" setup>
import { getDayOfWeek, isEqualDay } from '@internationalized/date';
import { computed, h, reactive, ref, resolveComponent, watch } from 'vue';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import PersonSelectorNuxt from '@/components/PersonSelectorNuxt.vue';
import { ChipProps, FormError } from '@nuxt/ui';
import ADateRangePicker from '@/components/Form/ADateRangePicker.vue';
import { UserInfoMinimal } from '@/models/user/user.ts';

const UButton = resolveComponent('UButton');
const UBadge = resolveComponent('UBadge');
const USelect = resolveComponent('USelect');
const UChip = resolveComponent('UChip');

const toast = useToast();
const router = useRouter();
const otium = useOtiumStore();
await otium.updateBlocks();

const stepperStatus = ref(0);

const weeks = ref<WeekWithType[]>([]);

const aufsichten = ref<SlotAndDayWithSupervisors[]>([]);

const angeboteProWochentag = ref([
    {
        tag: 'Montag',
        slotsH: [],
        slotsN: [],
    },
    {
        tag: 'Dienstag',
        slotsH: [],
        slotsN: [],
    },
    {
        tag: 'Mittwoch',
        slotsH: [],
        slotsN: [],
    },
    {
        tag: 'Donnerstag',
        slotsH: [],
        slotsN: [],
    },
    {
        tag: 'Freitag',
        slotsH: [],
        slotsN: [],
    },
    {
        tag: 'Samstag',
        slotsH: [],
        slotsN: [],
    },
]);

const blocksAvailable = otium.blocks;

watch(stepperStatus, (newValue) => {
    if (newValue !== 5) return;
    const newAufsichten = [] as SlotAndDayWithSupervisors[];
    for (const day of angeboteProWochentag.value) {
        for (const slotH of day.slotsH) {
            newAufsichten.push({
                day: day.tag,
                typ: 'H-Woche',
                block: slotH,
                supervisors: [],
            });
        }
        for (const slotN of day.slotsN) {
            newAufsichten.push({
                day: day.tag,
                typ: 'N-Woche',
                block: slotN,
                supervisors: [],
            });
        }
    }
    aufsichten.value = newAufsichten;
});

function validateTimeframe(state: Partial<TimeframeFormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.start)
        errors.push({
            name: 'start',
            message: 'Bitte geben Sie das Startdatum an!',
        });

    if (!state.end)
        errors.push({
            name: 'end',
            message: 'Bitte geben Sie das Enddatum an.',
        });

    if (state.start && state.end && state.start.compare(state.end) >= 0)
        errors.push({
            name: 'end',
            message: 'Das Enddatum muss nach dem Startdatum liegen.',
        });

    return errors;
}

function submitTimeframe() {
    stepperStatus.value = 2;
}

async function submit() {
    const wochentypen = { H: 'H-Woche', N: 'N-Woche' };
    const data = [];

    let current = timeFrameState.start! as CalendarDate;
    while (current.compare(timeFrameState.end!) <= 0) {
        const dayOfWeek = getDayOfWeek(current, 'de-DE', 'mon');
        // skip sunndays
        if (dayOfWeek === 6) {
            current = current.add({ days: 1 });
            continue;
        }

        // Check if the current date is in a holiday
        const isInHoliday = vacationDays.value.some((e) => {
            return isEqualDay(current, e);
        });

        if (isInHoliday) {
            current = current.add({ days: 1 });
            continue;
        }

        // Find the week for the current date
        const monday = current.add({ days: -dayOfWeek });
        const week = weeks.value.find((w) => {
            return isEqualDay(monday, w.start as DateValue);
        });

        if (!week) {
            current = current.add({ days: 1 });
            console.error('Could not find Week', current);
            continue;
        }

        const niceWochentage = [
            'Montag',
            'Dienstag',
            'Mittwoch',
            'Donnerstag',
            'Freitag',
            'Samstag',
            'Sonntag',
        ];

        // Find the blocks for the day of the week
        const angeboteAmWochentag = angeboteProWochentag.value.find(
            (d) => d.tag === niceWochentage[dayOfWeek],
        )!;
        const blocks =
            week.type === 'H'
                ? angeboteAmWochentag.slotsH.map((w) => {
                      const aufsichtenSlot = aufsichten.value.find(
                          (a) =>
                              a.day === niceWochentage[dayOfWeek] &&
                              a.typ === 'H-Woche' &&
                              a.block === w,
                      )!;
                      return {
                          schemaId: w,
                          supervisors: aufsichtenSlot.supervisors,
                      };
                  })
                : angeboteAmWochentag.slotsN.map((w) => {
                      const aufsichtenSlot = aufsichten.value.find(
                          (a) =>
                              a.day === niceWochentage[dayOfWeek] &&
                              a.typ === 'N-Woche' &&
                              a.block === w,
                      )!;
                      return {
                          schemaId: w,
                          supervisors: aufsichtenSlot.supervisors,
                      };
                  });

        if (blocks.length > 0)
            data.push({
                datum: current.toString(),
                wochentyp: wochentypen[week.type as WeekType],
                blocks: blocks,
            });

        current = current.add({ days: 1 });
    }

    const api = mande('/api/management/schuljahr');
    try {
        await api.post(data);
        toast.add({
            color: 'success',
            title: 'Erfolg',
            description: 'Die Termine wurden erfolgreich gespeichert.',
        });
        await router.push({ name: 'Verwaltung' });
    } catch (error) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Die Termine konnten nicht gespeichert werden.',
        });
    }
}

const timeFrameState = reactive<TimeframeFormSchema>({
    start: undefined,
    end: undefined,
});

const vacationState = ref<VacationFormSchema>({
    start: undefined,
    end: undefined,
});

const vacations = ref<Vacation[]>([]);
const vacationDays = computed(() => {
    const days = [] as DateValue[];

    for (const vacation of vacations.value) {
        if (!vacation.end) {
            days.push(vacation.start as CalendarDate);
            continue;
        }

        let currentDate = vacation.start;
        while (currentDate.compare(vacation.end as CalendarDate) <= 0) {
            days.push(currentDate as DateValue);
            currentDate = currentDate.add({ days: 1 });
        }
    }
    return days;
});
const sortedVacations = computed(() => {
    return vacations.value.sort((a, b) => a.start.compare(b.start as DateValue));
});

function addVacation() {
    if (!vacationState.value?.start) return;

    if (
        vacationState.value.end &&
        isEqualDay(vacationState.value.start as DateValue, vacationState.value.end as DateValue)
    )
        vacationState.value.end = undefined;
    vacations.value.push(Object.assign({}, vacationState.value) as Vacation);
    vacationState.value = {
        start: undefined,
        end: undefined,
    };
}

function isDateDisabled(date: DateValue) {
    return vacationDays.value.some((v) => isEqualDay(v, date));
}

const vacationColumns: TableColumn<Vacation>[] = [
    {
        id: 'start',
        header: 'Beginn',
        accessorFn: (data) => `${data.start.day}.${data.start.month}.${data.start.year}`,
    },
    {
        id: 'end',
        header: 'Ende',
        accessorFn: (data) => {
            if (!data.end) return '';
            return `${data.end.day}.${data.end.month}.${data.end.year}`;
        },
    },
    {
        id: 'actions',
        cell: ({ row }) =>
            h(UButton, {
                icon: 'i-lucide-x',
                color: 'error',
                variant: 'ghost',
                size: 'sm',
                onClick: () => {
                    const index = vacations.value.indexOf(row.original);
                    if (index > -1) {
                        vacations.value.splice(index, 1);
                    }
                },
            }),
        meta: {
            class: {
                td: 'text-right',
            },
        },
    },
];

function submitVacations() {
    const tempWeeks = [] as WeekWithType[];

    let current = timeFrameState.start!;
    current = current.add({
        days: -getDayOfWeek(current as DateValue, 'de-DE', 'mon'),
    }); // Set to Monday

    while (current.compare(timeFrameState.end!) <= 0) {
        const weekStart = current;
        const weekEnd = current.add({ days: 6 });

        const hasHoliday = vacations.value.some((e) => {
            if (!e.end) return false; // Ignore one-day holidays in weektype calculation
            const startsInWeek =
                e.start.compare(weekStart as DateValue) >= 0 &&
                e.start.compare(weekEnd as DateValue) <= 0;
            if (startsInWeek) return true;
            return (
                e.end.compare(weekStart as DateValue) >= 0 &&
                e.end.compare(weekEnd as DateValue) <= 0
            ); // ends in week
        });

        const isInHoliday = vacations.value.some((e) => {
            if (!e.end) return false;
            return (
                e.start.compare(weekStart as DateValue) <= 0 &&
                e.end.compare(weekEnd as DateValue) >= 0
            );
        });

        tempWeeks.push({
            start: weekStart as CalendarDate,
            end: weekEnd as CalendarDate,
            type: isInHoliday ? 'F' : hasHoliday ? 'H' : 'N',
        });

        current = current.add({ days: 7 }); // add 7 days to get the next week
    }

    weeks.value = tempWeeks;
    stepperStatus.value = 3;
}

const weekColumns: TableColumn<WeekWithType>[] = [
    {
        id: 'start',
        header: 'Montag',
        accessorFn: (data) => `${data.start.day}.${data.start.month}.${data.start.year}`,
    },
    {
        id: 'end',
        header: 'Sonntag',
        accessorFn: (data) => `${data.end.day}.${data.end.month}.${data.end.year}`,
    },
    {
        id: 'type',
        header: 'Typ',
        cell: ({ row }) =>
            row.original.type == 'F'
                ? h(UBadge, { label: 'Ferien', color: 'neutral' })
                : h(
                      USelect,
                      {
                          items: weekTypeOptions,
                          modelValue: row.original.type,
                          'onUpdate:modelValue': (value: WeekType) => {
                              row.original.type = value;
                          },
                      },
                      {
                          leading: ({
                              modelValue,
                              ui,
                          }: {
                              modelValue: WeekType | undefined;
                              ui: any;
                          }) => {
                              if (!modelValue) return null;
                              return h(UChip, {
                                  size: ui.itemLeadingChipSize() as ChipProps['size'],
                                  class: ui.itemLeadingChip(),
                                  standalone: true,
                                  inset: true,
                                  ...getChip(modelValue),
                              });
                          },
                      },
                  ),
    },
];

const slotColumns: TableColumn<DayWithBlocks>[] = [
    {
        id: 'day',
        header: 'Tag',
        accessorKey: 'tag',
    },
    {
        id: 'h',
        header: 'H-Woche',
        cell: ({ row }) => {
            return h(USelect, {
                class: 'w-full min-w-40',
                items: blocksAvailable,
                labelKey: 'bezeichnung',
                valueKey: 'schemaId',
                multiple: true,
                placeholder: 'Keine Slots ausgewählt',
                modelValue: row.original.slotsH,
                'onUpdate:modelValue': (newValue: string[]) => {
                    row.original.slotsH = newValue;
                },
            });
        },
    },
    {
        id: 'n',
        header: 'N-Woche',
        cell: ({ row }) => {
            return h(USelect, {
                class: 'w-full min-w-40',
                items: blocksAvailable,
                labelKey: 'bezeichnung',
                valueKey: 'schemaId',
                multiple: true,
                placeholder: 'Keine Slots ausgewählt',
                modelValue: row.original.slotsN,
                'onUpdate:modelValue': (newValue: string[]) => {
                    row.original.slotsN = newValue;
                },
            });
        },
    },
];

const supervisorColumns: TableColumn<SlotAndDayWithSupervisors>[] = [
    {
        header: 'Tag',
        accessorKey: 'day',
    },
    {
        header: 'Typ',
        accessorKey: 'typ',
    },
    {
        header: 'Block',
        accessorFn: (data) =>
            (blocksAvailable! as any[]).find((b) => b.schemaId === data.block)?.bezeichnung,
    },
    {
        header: 'Aufsichten',
        cell: ({ row }) => {
            return h(PersonSelectorNuxt, {
                class: 'w-full min-w-40',
                multiple: true,
                filter: (p: UserInfoMinimal) => p.rolle == 'Tutor',
                modelValue: row.original.supervisors,
                'onUpdate:modelValue': (data) => {
                    row.original.supervisors = data as string[];
                },
                placeholder: 'Aufsicht wählen',
            });
        },
    },
];
</script>

<script lang="ts">
import { CalendarDate, DateValue } from '@internationalized/date';
import { TableColumn } from '@nuxt/ui';

const stepperItems = [
    {
        title: 'Start',
        slot: 'start',
    },
    {
        title: 'Zeitrahmen',
        slot: 'timeframe',
    },
    {
        title: 'Ferien',
        slot: 'holidays',
    },
    {
        title: 'H/N-Wochen',
        slot: 'weektypes',
    },
    {
        title: 'Slots',
        slot: 'slots',
    },
    {
        title: 'Aufsichten',
        slot: 'supervisors',
    },
];

const weekTypeOptions = [
    {
        value: 'H',
        label: 'H-Woche',
        chip: {
            color: 'success',
        },
    },
    {
        value: 'N',
        label: 'N-Woche',
        chip: {
            color: 'error',
        },
    },
];

function getChip(value: string) {
    return weekTypeOptions.find((v) => v.value === value)?.chip;
}

interface TimeframeFormSchema {
    start: CalendarDate | undefined;
    end: CalendarDate | undefined;
}

type VacationFormSchema = TimeframeFormSchema;

interface Vacation {
    start: DateValue;
    end: DateValue | undefined;
}

type WeekType = 'H' | 'N';
type WeekTypeOrVacation = WeekType | 'F';

interface WeekWithType {
    start: DateValue;
    end: DateValue;
    type: WeekTypeOrVacation;
}

interface DayWithBlocks {
    tag: string;
    slotsN: string[];
    slotsH: string[];
}

interface SlotAndDayWithSupervisors {
    day: string;
    typ: string;
    block: string;
    supervisors: string[];
}
</script>

<template>
    <h1>Schultage anlegen</h1>
    <UStepper v-model="stepperStatus" :items="stepperItems" disabled>
        <template #start>
            <p>Dieses Programm hilf Ihnen dabei, ihr Schuljahr vorauszuplanen.</p>
            <UButton
                class="w-full"
                label="Start"
                @click="
                    () => {
                        stepperStatus = 1;
                    }
                "
            />
        </template>
        <template #timeframe>
            <p>
                Bitte geben sie Start und Ende des Zeitraumes an, für den Sie Schultage anlegen
                möchten.
            </p>
            <UForm
                :state="timeFrameState"
                :validate="validateTimeframe"
                class="flex flex-col gap-4"
                @submit="submitTimeframe"
            >
                <UFormField label="Beginn des Zeitraums" name="start">
                    <ADatePicker
                        v-model="timeFrameState.start as CalendarDate"
                        class="w-full"
                    />
                </UFormField>
                <UFormField label="Ende des Zeitraums" name="end">
                    <ADatePicker v-model="timeFrameState.end as CalendarDate" class="w-full" />
                </UFormField>
                <UButton label="Weiter" type="submit" />
            </UForm>
        </template>
        <template #holidays>
            <div class="flex flex-col gap-4">
                <p>Bitte geben sie alle Schulferien und Feiertage an.</p>
                <div class="flex w-full gap-4 justify-stretch">
                    <ADateRangePicker
                        v-model="vacationState as any"
                        :isDateDisabled="isDateDisabled"
                        :maxValue="timeFrameState.end"
                        :minValue="timeFrameState.start"
                        :numberOfMonths="3"
                        class="w-full"
                    />
                    <UButton label="Hinzufügen" @click="addVacation" />
                </div>
                <UTable :columns="vacationColumns" :data="vacations as any[]" />
                <div class="flex gap-4">
                    <UButton
                        class="flex-1"
                        color="neutral"
                        label="Zurück"
                        variant="soft"
                        @click="
                            () => {
                                stepperStatus = 1;
                            }
                        "
                    />
                    <UButton class="flex-4" label="Weiter" @click="() => submitVacations()" />
                </div>
            </div>
        </template>
        <template #weektypes>
            <p>
                Wir haben automatisch versucht die H-/ N-Wochen vorherzusagen. Bitte
                kontrollieren Sie die Vorhersagen und passen Sie ggf. den Typ an.
            </p>
            <UTable :columns="weekColumns" :data="weeks as any[]" />
            <UButton class="mt-4 w-full" label="Weiter" @click="() => (stepperStatus = 4)" />
        </template>
        <template #slots>
            <p>Bitte ordnen Sie den Wochentagen die stattfindenden Blöcke zu.</p>
            <UTable :columns="slotColumns" :data="angeboteProWochentag" />
            <UButton class="mt-4 w-full" label="Weiter" @click="() => (stepperStatus = 5)" />
        </template>
        <template #supervisors>
            <p>Bitte geben Sie Aufsichten für die Slots an.</p>
            <UTable :columns="supervisorColumns" :data="aufsichten" />
            <UButton class="mt-4 w-full" label="Abschließen" @click="submit" />
        </template>
    </UStepper>
</template>

<style scoped></style>
