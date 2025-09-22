<script setup>
import { ref, Suspense } from 'vue';
import { Button } from 'primevue';
import AfraOtiumSupervisionView from '@/Otium/components/Supervision/AfraOtiumSupervisionView.vue';
import { useRoute } from 'vue-router';
import { mande } from 'mande';

const route = useRoute();
const status = ref(route.query.blockId !== undefined);

const blocksAvailable = ref();
const blockSelected = ref();

function start(block) {
    blockSelected.value = block;
    status.value = true;
}

async function stop() {
    status.value = false;
    blockSelected.value = null;
    await setup();
}

async function setup() {
    if (route.query.blockId !== undefined) return;
    blocksAvailable.value = await mande('/api/otium/management/supervision/now').get();
}

await setup();
</script>

<template>
    <div class="flex justify-between items-center">
        <h1>Aufsicht</h1>
        <Button
            v-if="status"
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
                v-for="block in blocksAvailable"
                :key="block.id"
                :label="block.name"
                class="min-w-30"
                @click="() => start(block)"
            />
            <p v-if="blocksAvailable.length === 0">
                Es sind keine Slots zur Aufsicht verfügbar.
            </p>
        </div>
        <p>
            Mit dem Drücken auf den Block bestätigen Sie, dass sie eingeteilte Aufsicht für den
            Slot sind. Alle Änderungen, die Sie vornehmen, werden protokolliert.
        </p>
    </div>
    <div v-else>
        <Suspense>
            <afra-otium-supervision-view
                :block="blockSelected"
                :use-query-block="route.query.blockId !== undefined"
            />
            <template #fallback>
                <div class="flex justify-center">
                    <span class="p-3">Lade Aufsicht...</span>
                </div>
            </template>
        </Suspense>
    </div>
</template>

<style scoped></style>
