<script lang="ts" setup>
import { h, resolveComponent, shallowRef, VNode } from 'vue';
import { mande } from 'mande';
import {
    DashboardMenteeOverview,
    DashboardMenteeStatus,
    ScopedDashboardTutorEventDescriptor,
    TutorDashboard,
} from '@/models/dashboard';
import { TableColumn } from '@nuxt/ui';
import { parseDateTime } from '@internationalized/date';
import {
    chooseColorNuxtUi,
    formatCalendarDateTime,
    formatStudent,
} from '@/helpers/formatters.ts';
import { useUser } from '@/stores/user.ts';

const UBadge = resolveComponent('UBadge');
const UButton = resolveComponent('UButton');

const api = mande('/api/dashboard/tutor');
const data = shallowRef<TutorDashboard | null>(null);
const user = useUser();

api.get<TutorDashboard>().then((result) => {
    data.value = result;
    loading.value = false;
});

const loading = shallowRef(true);

const titleRenderFunctions: Record<
    string,
    (row: ScopedDashboardTutorEventDescriptor) => VNode | string
> = {
    Otium: (row) =>
        h(UButton, {
            label: row.label,
            variant: 'subtle',
            to: {
                name: 'Verwaltung-Termin',
                params: { terminId: row.payload?.terminId },
            },
        }),
};

const eventColumns: TableColumn<ScopedDashboardTutorEventDescriptor>[] = [
    {
        header: 'Termin',
        accessorKey: 'label',
        cell: ({ row }) => {
            const renderFunction = titleRenderFunctions[row.original.scope];
            return renderFunction ? renderFunction(row.original) : row.original.label;
        },
    },
    {
        header: 'Datum',
        accessorKey: 'start',
        cell: ({ row }) => {
            const date = parseDateTime(row.original.start);
            return formatCalendarDateTime(date);
        },
    },
    {
        header: 'Slot',
        accessorKey: 'slotLabel',
    },
    {
        header: 'Auslastung',
        accessorKey: 'occupancy',
        cell: ({ row }) =>
            row.original.occupancy == undefined
                ? h(UBadge, {
                      label: '—',
                      color: 'neutral',
                      variant: 'soft',
                      class: 'w-full justify-center items-center',
                  })
                : h(UBadge, {
                      label: `${row.original.occupancy * 100} %`,
                      color: chooseColorNuxtUi(row.original.occupancy * 100),
                      variant: 'soft',
                      class: 'w-full justify-center items-center',
                  }),
    },
];

const statusColorMap: Record<DashboardMenteeStatus, string> = {
    Invalid: 'error',
    NotApplicable: 'neutral',
    Uncertain: 'warning',
    Valid: 'success',
};

const statusTextMap: Record<DashboardMenteeStatus, string> = {
    Invalid: 'Auffällig',
    NotApplicable: '—',
    Uncertain: 'Offen',
    Valid: 'Okay',
};

function renderWeekBadge(status: DashboardMenteeStatus) {
    return h(UBadge, {
        label: statusTextMap[status],
        variant: 'subtle',
        color: statusColorMap[status],
        class: 'w-full justify-center items-center',
    });
}

const menteeColumns: TableColumn<DashboardMenteeOverview>[] = [
    {
        header: 'Name',
        cell: ({ row }) =>
            h(UButton, {
                label: formatStudent(row.original.mentee),
                variant: 'subtle',
                to: {
                    name: 'Mentee',
                    params: {
                        studentId: row.original.mentee.id,
                    },
                },
            }),
    },
    {
        header: 'Letzte Woche',
        cell: ({ row }) => renderWeekBadge(row.original.last),
    },
    {
        header: 'Diese Woche',
        cell: ({ row }) => renderWeekBadge(row.original.current),
    },
    {
        header: 'Nächste Woche',
        cell: ({ row }) => renderWeekBadge(row.original.next),
    },
];
</script>

<template>
    <div class="flex flex-col gap-4">
        <h1>
            Guten Tag,
            {{ user.user ? `${user.user?.vorname} ${user.user?.nachname}` : 'User' }}!
        </h1>

        <UCard
            description="Sie sehen ihre Termine der kommenden drei Wochen"
            title="Nächste Termine"
        >
            <ASkeletonTable v-if="loading" :n="3" />
            <UTable v-else :columns="eventColumns" :data="data?.events ?? []">
                <template #empty>
                    <div class="flex gap-2 items-center justify-center">
                        <UIcon name="i-lucide-laugh" />
                        <span>Sie haben in der nächsten Zeit keine Termine!</span>
                    </div>
                </template>
            </UTable>
        </UCard>
        <UCard
            description="Eine Übersicht, ob ihre Mentees die Vorgaben einhalten"
            title="Mentees"
        >
            <ASkeletonTable v-if="loading" :n="3" />
            <UTable v-else :columns="menteeColumns" :data="data?.mentees ?? []" />
            <template #footer>
                <span class="text-muted text-sm">
                    <strong>Legende</strong><br />
                    Auffällig: Mindestens eine Vorgabe wurde endgültig nicht eingehalten. <br />
                    Offen: Mindestens eine Vorgabe wird nicht eingehalten. Eine Einhaltung ist
                    evtl. noch möglich.<br />
                    Okay: Ihr Mentee beachtet alle Vorgaben.
                </span>
            </template>
        </UCard>
    </div>
</template>

<style scoped></style>
