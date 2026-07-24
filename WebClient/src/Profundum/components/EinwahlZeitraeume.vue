<script setup>
import { onMounted, ref } from 'vue';
import { mande } from 'mande';
import { fromDate, getLocalTimeZone, toCalendarDateTime } from '@internationalized/date';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import ADateTimePicker from '@/components/Form/ADateTimePicker.vue';
import CreateEinwahlzeitraumForm from '@/Profundum/components/Forms/CreateEinwahlzeitraumForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();
const api = mande('/api/profundum/management/einwahlzeitraum');

const zeitraeume = ref([]);
const loading = ref(true);

function toCalendarDateTimeOrNull(value) {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : toCalendarDateTime(fromDate(d, getLocalTimeZone()));
}

async function load() {
    loading.value = true;
    const res = await api.get();
    zeitraeume.value = res.map((z) => ({
        ...z,
        einwahlStartDate: toCalendarDateTimeOrNull(z.einwahlStart),
        einwahlStopDate: toCalendarDateTimeOrNull(z.einwahlStop),
    }));
    loading.value = false;
}

async function createEinwahlzeitraum(data) {
    try {
        await api.post(data);
        toast.add({ color: 'success', title: 'Einwahlzeitraum angelegt' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Einwahlzeitraum nicht speichern',
        });
    }
}

const createDialog = overlay.create(CreateEinwahlzeitraumForm);

async function openCreateDialog() {
    const data = await createDialog.open();
    if (!data) return;
    await createEinwahlzeitraum(data);
}

async function updateEinwahlzeitraum(z) {
    try {
        await api.put(`/${z.id}`, {
            id: z.id,
            einwahlStart: z.einwahlStartDate
                ? z.einwahlStartDate.toDate(getLocalTimeZone()).toISOString()
                : null,
            einwahlStop: z.einwahlStopDate
                ? z.einwahlStopDate.toDate(getLocalTimeZone()).toISOString()
                : null,
        });

        toast.add({ color: 'success', title: 'Einwahlzeitraum gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Einwahlzeitraum nicht speichern',
        });
    }
}

async function deleteEinwahlzeitraum(z) {
    if (
        !(await requireConfirm(
            'Möchten Sie diesen Einwahlzeitraum wirklich löschen?',
            'Einwahlzeitraum löschen',
        ))
    )
        return;

    try {
        await api.delete(`/${z.id}`);
        toast.add({
            color: 'success',
            title: 'Gelöscht',
            description: 'Einwahlzeitraum wurde entfernt',
        });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Einwahlzeitraum nicht löschen',
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
                        {{
                            z.einwahlStartDate
                                ?.toDate(getLocalTimeZone())
                                .toLocaleString('de-DE') ?? '–'
                        }}
                        –
                        {{
                            z.einwahlStopDate
                                ?.toDate(getLocalTimeZone())
                                .toLocaleString('de-DE') ?? '–'
                        }}
                    </span>
                </template>

                <template #edit>
                    <div class="flex flex-col gap-2 w-full">
                        <div>
                            <label class="block mb-1">Start</label>
                            <ADateTimePicker v-model="z.einwahlStartDate" />
                        </div>
                        <div>
                            <label class="block mb-1">Ende</label>
                            <ADateTimePicker v-model="z.einwahlStopDate" />
                        </div>
                    </div>
                </template>
            </GridEditRow>
        </Grid>

        <div v-else>Keine Einwahlzeiträume vorhanden.</div>

        <UButton
            icon="i-lucide-plus"
            label="Neuer Einwahlzeitraum"
            class="mt-4"
            @click="openCreateDialog"
        />
    </template>
</template>

<style scoped></style>
