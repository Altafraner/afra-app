<script setup>
import { ref, onMounted } from 'vue';
import { mande } from 'mande';
import { Button, Calendar, Dialog, useToast } from 'primevue';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';

const toast = useToast();
const api = mande('/api/profundum/management/einwahlzeitraum');

const zeitraeume = ref([]);
const loading = ref(true);

const dialogOpen = ref(false);
const createModel = ref({
    einwahlStart: null,
    einwahlStop: null,
});

function toDateOrNull(value) {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
}

async function load() {
    loading.value = true;
    const res = await api.get();
    zeitraeume.value = res.map((z) => ({
        ...z,
        einwahlStartDate: toDateOrNull(z.einwahlStart),
        einwahlStopDate: toDateOrNull(z.einwahlStop),
    }));
    loading.value = false;
}

async function createEinwahlzeitraum() {
    try {
        await api.post({
            einwahlStart: createModel.value.einwahlStart
                ? createModel.value.einwahlStart.toISOString()
                : null,
            einwahlStop: createModel.value.einwahlStop
                ? createModel.value.einwahlStop.toISOString()
                : null,
        });

        toast.add({ severity: 'success', summary: 'Einwahlzeitraum angelegt' });

        dialogOpen.value = false;
        createModel.value = { einwahlStart: null, einwahlStop: null };
        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Einwahlzeitraum nicht speichern',
        });
    }
}

async function updateEinwahlzeitraum(z) {
    try {
        await api.put(`/${z.id}`, {
            id: z.id,
            einwahlStart: z.einwahlStartDate ? z.einwahlStartDate.toISOString() : null,
            einwahlStop: z.einwahlStopDate ? z.einwahlStopDate.toISOString() : null,
        });

        toast.add({ severity: 'success', summary: 'Einwahlzeitraum gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Einwahlzeitraum nicht speichern',
        });
    }
}

async function deleteEinwahlzeitraum(z) {
    if (!confirm('Möchten Sie diesen Einwahlzeitraum wirklich löschen?')) return;

    try {
        await api.delete(`/${z.id}`);
        toast.add({
            severity: 'success',
            summary: 'Gelöscht',
            detail: 'Einwahlzeitraum wurde entfernt',
        });
        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Einwahlzeitraum nicht löschen',
        });
    }
}

onMounted(load);
</script>

<template>
    <h2 class="mt-6">Einwahlzeiträume</h2>

    <template v-if="loading">
        <div>Lade …</div>
    </template>

    <template v-else>
        <Grid v-if="zeitraeume.length">
            <GridEditRow
                v-for="z in zeitraeume"
                :key="z.id"
                header="Einwahlzeitraum"
                :canDelete="true"
                @update="updateEinwahlzeitraum(z)"
                @delete="deleteEinwahlzeitraum(z)"
            >
                <template #body>
                    <span>
                        {{ z.einwahlStartDate?.toLocaleString('de-DE') ?? '–' }}
                        –
                        {{ z.einwahlStopDate?.toLocaleString('de-DE') ?? '–' }}
                    </span>
                </template>

                <template #edit>
                    <div class="flex flex-col gap-2 w-full">
                        <div>
                            <label class="block mb-1">Start</label>
                            <Calendar
                                v-model="z.einwahlStartDate"
                                showTime
                                hourFormat="24"
                                class="w-full"
                            />
                        </div>
                        <div>
                            <label class="block mb-1">Ende</label>
                            <Calendar
                                v-model="z.einwahlStopDate"
                                showTime
                                hourFormat="24"
                                class="w-full"
                            />
                        </div>
                    </div>
                </template>
            </GridEditRow>
        </Grid>

        <div v-else>Keine Einwahlzeiträume vorhanden.</div>

        <Button
            icon="pi pi-plus"
            label="Neuer Einwahlzeitraum"
            class="mt-4"
            @click="dialogOpen = true"
        />
    </template>

    <Dialog v-model:visible="dialogOpen" header="Neuer Einwahlzeitraum" modal>
        <div class="flex flex-col gap-3">
            <div>
                <label class="block mb-1">Start</label>
                <Calendar
                    v-model="createModel.einwahlStart"
                    showTime
                    hourFormat="24"
                    class="w-full"
                />
            </div>

            <div>
                <label class="block mb-1">Ende</label>
                <Calendar
                    v-model="createModel.einwahlStop"
                    showTime
                    hourFormat="24"
                    class="w-full"
                />
            </div>
        </div>

        <template #footer>
            <Button label="Abbrechen" @click="dialogOpen = false" />
            <Button label="Speichern" icon="pi pi-check" @click="createEinwahlzeitraum" />
        </template>
    </Dialog>
</template>

<style scoped>
:deep(.p-inputtext) {
    width: 100%;
}
</style>
