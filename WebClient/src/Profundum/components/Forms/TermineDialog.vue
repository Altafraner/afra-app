<script setup>
import { onMounted, ref } from 'vue';
import { mande } from 'mande';
import { parseDate, parseTime } from '@internationalized/date';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import ADatePicker from '@/components/Form/ADatePicker.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { formatCalendarDate } from '@/helpers/formatters.ts';

const props = defineProps({
    slotId: { type: String, required: true },
    slotLabel: { type: String, default: '' },
});

defineEmits(['close']);

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const api = mande(`/api/profundum/management/slot/${props.slotId}/termin`);

const termine = ref([]);
const loading = ref(true);

function toRow(t) {
    return {
        day: parseDate(t.day),
        startTime: parseTime(t.startTime),
        endTime: parseTime(t.endTime),
    };
}

async function load() {
    loading.value = true;
    termine.value = (await api.get()).map(toRow);
    loading.value = false;
}

function toDto(row) {
    return {
        day: row.day.toString(),
        startTime: `${row.startTime}`,
        endTime: `${row.endTime}`,
    };
}

const newTermin = ref({ day: undefined, startTime: undefined, endTime: undefined });

async function createTermin() {
    if (!newTermin.value.day || !newTermin.value.startTime || !newTermin.value.endTime) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Bitte Tag, Start- und Endzeit angeben.',
        });
        return;
    }

    try {
        await api.post(toDto(newTermin.value));
        toast.add({ color: 'success', title: 'Termin angelegt' });
        newTermin.value = { day: undefined, startTime: undefined, endTime: undefined };
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body?.error ?? 'Konnte Termin nicht speichern',
        });
    }
}

async function updateTermin(row, originalDay) {
    try {
        await api.put(`/${originalDay}`, toDto(row));
        toast.add({ color: 'success', title: 'Termin gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body?.error ?? 'Konnte Termin nicht speichern',
        });
    }
}

async function deleteTermin(row) {
    if (!(await requireConfirm(`Termin am ${row.day} wirklich löschen?`, 'Termin löschen')))
        return;

    try {
        await api.delete(`/${row.day}`);
        toast.add({ color: 'success', title: 'Termin gelöscht' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body?.error ?? 'Konnte Termin nicht löschen',
        });
    }
}

onMounted(load);
</script>

<template>
    <UModal :title="`Termine${slotLabel ? ' – ' + slotLabel : ''}`">
        <template #body>
            <template v-if="loading">
                <div>Lade …</div>
            </template>

            <template v-else>
                <Grid v-if="termine.length">
                    <GridEditRow
                        v-for="t in termine"
                        :key="t.day.toString()"
                        :canDelete="true"
                        @edit="t._originalDay = t.day.toString()"
                        @update="updateTermin(t, t._originalDay)"
                        @delete="deleteTermin(t)"
                    >
                        <template #body>
                            {{ formatCalendarDate(t.day, false) }}, {{ t.startTime }}–{{
                                t.endTime
                            }}
                            Uhr
                        </template>

                        <template #edit>
                            <div class="flex flex-col gap-2 w-full">
                                <ADatePicker v-model="t.day" />
                                <UFieldGroup class="w-full">
                                    <UInputTime v-model="t.startTime" class="flex-1" />
                                    <UBadge color="neutral" label="-" variant="outline" />
                                    <UInputTime v-model="t.endTime" class="flex-1" />
                                </UFieldGroup>
                            </div>
                        </template>
                    </GridEditRow>
                </Grid>

                <div v-else class="mb-4">Keine Termine vorhanden.</div>

                <UFieldGroup class="w-full">
                    <UFormField class="w-full" label="Datum">
                        <ADatePicker v-model="newTermin.day" class="w-full rounded-r-none" />
                    </UFormField>
                    <UFormField label="Von">
                        <UInputTime v-model="newTermin.startTime" class="rounded-none" />
                    </UFormField>
                    <UFormField label="Bis">
                        <UInputTime v-model="newTermin.endTime" class="rounded-l-none" />
                    </UFormField>
                </UFieldGroup>
                <UButton
                    class="mt-2 w-full"
                    icon="i-lucide-plus"
                    label="Termin hinzufügen"
                    @click="createTermin"
                />
            </template>
        </template>
    </UModal>
</template>

<style scoped></style>
