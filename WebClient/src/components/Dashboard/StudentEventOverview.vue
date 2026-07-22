<script lang="ts" setup>
import { formatCalendarDate, formatCalendarTime } from '@/helpers/formatters.ts';
import { computed, h, resolveComponent, shallowRef, VNode, watch } from 'vue';
import { AttendanceState } from '@/Attendance/models/attendance.ts';
import {
    CalendarDate,
    CalendarDateTime,
    DateValue,
    getDayOfWeek,
    isEqualDay,
    parseDate,
    parseDateTime,
    today,
} from '@internationalized/date';
import { mande } from 'mande';
import { StudentDashboard } from '@/models/dashboard.ts';
import { useUser } from '@/stores/user.ts';
import { useOtiumStore } from '@/Otium/stores/otium';
import { findPath } from '@/helpers/tree';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import { TableColumn } from '@nuxt/ui';
import AttendanceButton from '@/Attendance/components/AttendanceButton.vue';

const UButton = resolveComponent('UButton');

interface EventDescriptor {
    scope: string;
    label: string | undefined;
    slotLabel: string;
    attendance: AttendanceState | undefined;
    location: string | undefined;
    start: string;
    startDateTime: CalendarDateTime;
    niceStartDate: string | null;
    niceStartTime: string;
    payload: any;
}

const props = defineProps<
    | {
          scope: 'self';
          studentId?: undefined;
      }
    | {
          scope: 'student';
          studentId: string;
      }
>();

const api = mande('/api/dashboard/student');
const data = shallowRef<StudentDashboard | null>(null);
const user = useUser();
const otiumStore = useOtiumStore();

const startDate = shallowRef<CalendarDate>(today('Europe/Berlin'));
const numWeeks = shallowRef(3);
startDate.value = startDate.value.add({ days: -getDayOfWeek(startDate.value, 'de-DE', 'mon') });

async function loadData() {
    const options = {
        query: {
            start: startDate.value.toString(),
            numWeeks: numWeeks.value,
        },
    };
    data.value =
        props.scope == 'self'
            ? await api.get<StudentDashboard>(options)
            : await api.get<StudentDashboard>(props.studentId, options);
}

const loading = shallowRef(true);
const buttonsLoading = shallowRef(true);

Promise.all([otiumStore.updateKategorien(), loadData()]).then(() => {
    loading.value = false;
    buttonsLoading.value = false;
});

watch([startDate, numWeeks], async () => {
    buttonsLoading.value = true;
    await loadData();
    buttonsLoading.value = false;
});

const mappedTermine = computed(() => {
    return (data.value?.weeks ?? []).map((week) => {
        const mondayDate = parseDate(week.monday);
        const sunday = mondayDate.add({ days: 6 });
        const niceDate = `${formatCalendarDate(mondayDate, false)} – ${formatCalendarDate(sunday, false)}`;
        const hasWeeklyWarnings = week.warnings.length > 0;
        const dailyWarningEntries = Object.entries(week.dailyWarnings).map(([k, v]) => ({
            key: formatCalendarDate(parseDate(k)),
            value: v,
        }));
        const hasDailyWarnings = dailyWarningEntries.length > 0;
        dailyWarningEntries.sort((a, b) => a.key.localeCompare(b.key));
        let lastDate: DateValue | null = null;
        const niceEvents = week.events.map((event) => {
            const startDateTime = parseDateTime(event.start);
            const showDate = lastDate == null ? true : !isEqualDay(startDateTime, lastDate);
            lastDate = startDateTime;
            return {
                scope: event.scope,
                label: event.label,
                payload: event.payload,
                slotLabel: event.slotLabel,
                attendance: event.attendance,
                location: event.location,
                start: event.start,
                startDateTime: startDateTime,
                niceStartDate: showDate ? formatCalendarDate(startDateTime) : null,
                niceStartTime: formatCalendarTime(startDateTime),
            };
        });
        return {
            hasDailyWarnings,
            hasWeeklyWarnings,
            warnings: week.warnings,
            dailyWarningEntries,
            monday: mondayDate,
            niceDate,
            events: niceEvents,
        };
    });
});
const findKategorie = (kategorie: string) => {
    const path = findPath(otiumStore.kategorien, kategorie);
    for (const element of path) {
        if (element.icon != null) {
            return element;
        }
    }
    return path[0];
};
const labelRenderFunctions: Record<string, (row: EventDescriptor) => VNode | string> = {
    Otium: (row) =>
        row.label
            ? h(
                  UButton,
                  {
                      variant: 'subtle',
                      to: {
                          name: 'Otium-Katalog-Datum-Termin',
                          params: {
                              datum: row.start.split('T')[0],
                              terminId: row.payload.terminId,
                          },
                      },
                      class: 'flex',
                  },
                  () => [
                      h('span', { class: 'flex gap-2' }, [
                          row.payload.categoryId
                              ? h(OtiumKategorieTag, {
                                    value: findKategorie(row.payload.categoryId),
                                    hideName: true,
                                    minimal: true,
                                })
                              : null,
                          row.label,
                      ]),
                  ],
              )
            : h(UButton, {
                  label: 'Katalog',
                  to: {
                      name: 'Otium-Katalog-Datum',
                      params: { datum: row.start.split('T')[0] },
                  },
                  class: 'flex',
                  icon: 'i-lucide-list',
              }),
};

const columns: TableColumn<EventDescriptor>[] = [
    {
        id: 'date',
        header: 'Datum',
        accessorKey: 'niceStartDate',
        meta: {
            class: {
                td: 'whitespace-nowrap',
            },
        },
    },
    {
        id: 'time',
        header: 'Zeit',
        accessorKey: 'niceStartTime',
        meta: {
            class: {
                td: 'whitespace-nowrap tabular-nums',
            },
        },
    },
    {
        id: 'slot',
        header: 'Slot',
        accessorKey: 'slotLabel',
    },
    {
        id: 'angebot',
        header: 'Angebot',
        accessorKey: 'label',
        cell: ({ row }) => {
            const renderFunction = labelRenderFunctions[row.original.scope];
            return renderFunction ? renderFunction(row.original) : row.original.label;
        },
    },
    {
        id: 'location',
        header: 'Ort',
        accessorKey: 'location',
    },
    {
        id: 'attendance',
        cell: ({ row }) =>
            row.original.attendance
                ? h(AttendanceButton, {
                      status: row.original.attendance,
                      mayEdit: false,
                      minimal: true,
                  })
                : null,
    },
];
</script>

<template>
    <UCard description="Deine Termine der nächsten Zeit" title="Nächste Termine">
        <template v-if="loading">
            <div class="flex flex-col gap-4 mt-2">
                <div class="flex justify-between">
                    <USkeleton class="w-50 h-4" />
                    <span class="inline-flex gap-2">
                        <USkeleton class="w-16 h-4" />
                        <USkeleton class="w-4 h-4" />
                    </span>
                </div>
                <USeparator />
                <div class="flex justify-between">
                    <USkeleton class="w-50 h-4" />
                    <span class="inline-flex gap-2">
                        <USkeleton class="w-16 h-4" />
                        <USkeleton class="w-4 h-4" />
                    </span>
                </div>
                <USeparator />
                <div class="flex justify-between">
                    <USkeleton class="w-50 h-4" />
                    <span class="inline-flex gap-2">
                        <USkeleton class="w-16 h-4" />
                        <USkeleton class="w-4 h-4" />
                    </span>
                </div>
            </div>
        </template>
        <template v-else>
            <UAccordion :items="mappedTermine">
                <template #default="{ item }">
                    <div class="whitespace-nowrap tabular-nums">{{ item.niceDate }}</div>
                </template>
                <template #trailing="{ item, open }">
                    <div class="flex justify-end w-full mr-1 items-center gap-2">
                        <UBadge
                            v-if="item.hasWeeklyWarnings || item.hasDailyWarnings"
                            class="min-w-12 justify-center"
                            color="error"
                            label="Offen"
                        />
                        <UBadge
                            v-else
                            class="min-w-12 justify-center"
                            color="success"
                            label="Okay"
                        />
                        <UIcon v-if="open" class="size-5" name="i-lucide-chevron-up" />
                        <UIcon v-else class="size-5" name="i-lucide-chevron-down" />
                    </div>
                </template>
                <template #body="{ item }">
                    <UAlert
                        v-if="item.hasWeeklyWarnings || item.hasDailyWarnings"
                        color="warning"
                        title="Die Belegung entspricht nicht den Vorgaben."
                        variant="subtle"
                    >
                        <template #description>
                            <div class="flex flex-col gap-2 mt-1">
                                <div v-if="item.hasWeeklyWarnings">
                                    <div class="font-medium">Gesamte Woche</div>
                                    <ul>
                                        <li v-for="message in item.warnings">
                                            {{ message }}
                                        </li>
                                    </ul>
                                </div>
                                <template v-if="item.hasDailyWarnings">
                                    <div
                                        v-for="{
                                            key: date,
                                            value: messages,
                                        } in item.dailyWarningEntries"
                                        :key="date"
                                    >
                                        <div class="font-medium">{{ date }}</div>
                                        <ul>
                                            <li v-for="message in messages" :key="message">
                                                {{ message }}
                                            </li>
                                        </ul>
                                    </div>
                                </template>
                            </div>
                        </template>
                    </UAlert>
                    <UAlert
                        v-else
                        :title="user.isStudent ? 'Geschafft!' : 'Alles okay!'"
                        color="success"
                        description="Die Belegung entspricht den Vorgaben."
                        variant="subtle"
                    />
                    <UTable
                        :columns="columns"
                        :data="item.events"
                        :ui="{
                            td: 'whitespace-normal text-normal ',
                            th: '',
                            root: 'overflow-x-visible',
                        }"
                    />
                </template>
            </UAccordion>
        </template>
        <template #footer>
            <div class="flex text-muted gap-2">
                <div class="flex-1 inline-flex gap-1">
                    <UButton
                        :loading="buttonsLoading"
                        color="neutral"
                        icon="i-lucide-chevrons-left"
                        variant="subtle"
                        @click="
                            () => {
                                startDate = startDate.add({ days: -21 });
                                numWeeks += 3;
                            }
                        "
                    />
                    <UButton
                        :loading="buttonsLoading"
                        color="neutral"
                        icon="i-lucide-chevron-left"
                        variant="subtle"
                        @click="
                            () => {
                                startDate = startDate.add({ days: -7 });
                                numWeeks += 1;
                            }
                        "
                    />
                </div>
                <div class="shrink-2">
                    {{ formatCalendarDate(startDate, false) }} bis
                    {{ formatCalendarDate(startDate.add({ days: numWeeks * 7 }), false) }}
                </div>
                <div class="flex-1 inline-flex justify-end gap-1">
                    <UButton
                        :loading="buttonsLoading"
                        color="neutral"
                        icon="i-lucide-chevron-right"
                        variant="subtle"
                        @click="
                            () => {
                                numWeeks += 1;
                            }
                        "
                    />
                    <UButton
                        :loading="buttonsLoading"
                        color="neutral"
                        icon="i-lucide-chevrons-right"
                        variant="subtle"
                        @click="
                            () => {
                                numWeeks += 3;
                            }
                        "
                    />
                </div>
            </div>
        </template>
    </UCard>
</template>

<style scoped></style>
