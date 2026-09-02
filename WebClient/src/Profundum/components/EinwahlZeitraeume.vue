<script setup>
import { onMounted, ref } from 'vue';
import { mande } from 'mande';
import { getLocalTimeZone, parseAbsolute } from '@internationalized/date';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import ADateTimePicker from '@/components/Form/ADateTimePicker.vue';
import CreateEinwahlzeitraumForm from '@/Profundum/components/Forms/CreateEinwahlzeitraumForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { formatCalendarDateTime } from '@/helpers/formatters.ts';

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();
const api = mande('/api/profundum/management/einwahlzeitraum');

const zeitraeume = ref([]);
const loading = ref(true);

async function load() {
    loading.value = true;
    const res = await api.get();
    zeitraeume.value = res.map((z) => ({
        ...z,
        einwahlStartDate: parseAbsolute(z.einwahlStart),
        einwahlStopDate: parseAbsolute(z.einwahlStop),
        veroeffentlichungsdatumDate: z.veroeffentlichungsdatum
            ? parseAbsolute(z.veroeffentlichungsdatum)
            : undefined,
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
            bezeichnung: z.bezeichnung,
            einwahlStart: z.einwahlStartDate
                ? z.einwahlStartDate.toDate(getLocalTimeZone()).toISOString()
                : null,
            einwahlStop: z.einwahlStopDate
                ? z.einwahlStopDate.toDate(getLocalTimeZone()).toISOString()
                : null,
            veroeffentlichungsdatum: z.veroeffentlichungsdatumDate
                ? z.veroeffentlichungsdatumDate.toDate(getLocalTimeZone()).toISOString()
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
                header=""
                :canDelete="true"
                @update="updateEinwahlzeitraum(z)"
                @delete="deleteEinwahlzeitraum(z)"
            >
                <template #body>
                    <span>
                        {{ z.bezeichnung }}: {{ formatCalendarDateTime(z.einwahlStartDate) }}
                        –
                        {{ formatCalendarDateTime(z.einwahlStopDate) }}
                        <template v-if="z.veroeffentlichungsdatumDate">
                            · veröffentlicht ab
                            {{ formatCalendarDateTime(z.veroeffentlichungsdatumDate) }}
                        </template>
                        <template v-else> · nicht veröffentlicht </template>
                    </span>
                </template>

                <template #edit>
                    <div class="flex gap-2 w-full items-end">
                        <UFormField label="Bezeichnung">
                            <UInput v-model="z.bezeichnung" />
                        </UFormField>
                        <UFormField label="Start" required>
                            <ADateTimePicker v-model="z.einwahlStartDate" />
                        </UFormField>
                        <span class="mb-2">–</span>
                        <UFormField label="Ende" required>
                            <ADateTimePicker v-model="z.einwahlStopDate" />
                        </UFormField>
                        <UFormField label="Veröffentlichungsdatum">
                            <ADateTimePicker v-model="z.veroeffentlichungsdatumDate" />
                        </UFormField>
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
