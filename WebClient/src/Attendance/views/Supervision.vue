<script lang="ts" setup>
import { computed, ref, shallowRef, Suspense } from 'vue';
import { Button } from 'primevue';
import { useRoute } from 'vue-router';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import type { AttendanceSlot } from '@/Attendance/models/attendance';
import GeneralSupervision from '@/Attendance/components/GeneralSupervision.vue';

const navItems = [
    {
        label: 'Otium',
        route: {
            name: 'Katalog',
        },
    },
    {
        label: 'Aufsicht',
        route: {
            name: 'Aufsicht',
        },
    },
];

const route = useRoute();
const status = ref(route.query.slotId !== undefined);

const slotsAvailable = shallowRef<AttendanceSlot[]>([]);
const slotSelected = shallowRef<AttendanceSlot | null>(null);

const slotActive = computed<AttendanceSlot | null>(() =>
    route.query.slotId !== undefined
        ? {
              slotId: route.query.slotId as string,
              scope: route.query.scope as string,
              label: '',
          }
        : slotSelected.value,
);

function start(slot: AttendanceSlot) {
    slotSelected.value = slot;
    status.value = true;
}

async function stop() {
    status.value = false;
    slotSelected.value = null;
    await setup();
}

async function setup() {
    if (route.query.slotId !== undefined) return;
    slotsAvailable.value = await mande('/api/attendance/active').get();
}

await setup();
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <div class="flex justify-between items-center">
        <h1>Aufsicht</h1>
        <Button
            v-if="status && route.query.blockId === undefined"
            icon="pi pi-stop"
            label="Block Wechseln"
            severity="secondary"
            @click="stop"
        />
    </div>

    <div v-if="!status">
        <p>Um ihre Aufsicht zu starten, drücken Sie auf den entsprechenden Slot.</p>
        <div class="flex gap-3">
            <Button
                v-for="slot in slotsAvailable"
                :key="slot.slotId"
                :label="slot.label"
                class="min-w-30"
                @click="() => start(slot)"
            />
            <p v-if="slotsAvailable.length === 0">
                Es sind keine Slots zur Aufsicht verfügbar.
            </p>
        </div>
        <p>
            Mit dem Drücken auf den Slot bestätigen Sie, dass sie für diesen eine eingeteilte
            Aufsicht sind. Alle Änderungen, die Sie vornehmen, werden protokolliert.
        </p>
    </div>
    <div v-else>
        <Suspense>
            <GeneralSupervision :slot="slotActive" />
            <template #fallback>
                <div class="flex justify-center">
                    <span class="p-3">Lade Aufsicht...</span>
                </div>
            </template>
        </Suspense>
    </div>
</template>

<style scoped></style>
