<script setup>
import { onMounted, ref } from 'vue';
import { mande } from 'mande';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import CreateSlotForm from '@/Profundum/components/Forms/CreateSlotForm.vue';
import TermineDialog from '@/Profundum/components/Forms/TermineDialog.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();

const apiSlots = mande('/api/profundum/management/slot');
const apiZeitraeume = mande('/api/profundum/management/einwahlzeitraum');

const slots = ref([]);
const zeitraeume = ref([]);
const loading = ref(true);

const weekdayOptions = [
    { label: 'Montag', value: 'Monday' },
    { label: 'Dienstag', value: 'Tuesday' },
    { label: 'Mittwoch', value: 'Wednesday' },
    { label: 'Donnerstag', value: 'Thursday' },
    { label: 'Freitag', value: 'Friday' },
    { label: 'Samstag', value: 'Saturday' },
    { label: 'Sonntag', value: 'Sunday' },
];

async function load() {
    loading.value = true;
    slots.value = await apiSlots.get();
    zeitraeume.value = await apiZeitraeume.get();
    loading.value = false;
}

async function createSlot(data) {
    try {
        await apiSlots.post(data);
        toast.add({ color: 'success', title: 'Slot angelegt' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Slot nicht speichern',
        });
    }
}

const createDialog = overlay.create(CreateSlotForm);

async function openCreateDialog() {
    const data = await createDialog.open({ zeitraeume: zeitraeume.value });
    if (!data) return;
    await createSlot(data);
}

const termineDialog = overlay.create(TermineDialog);

function openTermineDialog(slot) {
    const label = `${slot.jahr} ${slot.quartal} ${
        weekdayOptions.find((d) => d.value === slot.wochentag)?.label ?? slot.wochentag
    }`;
    termineDialog.open({ slotId: slot.id, slotLabel: label });
}

async function updateSlot(slot) {
    try {
        await apiSlots.put(`/${slot.id}`, {
            id: slot.id,
            jahr: slot.jahr,
            quartal: slot.quartal,
            wochentag: slot.wochentag,
            einwahlZeitraumId: slot.einwahlZeitraumId,
        });

        toast.add({ color: 'success', title: 'Slot gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Slot nicht speichern',
        });
    }
}

async function deleteSlot(slot) {
    if (!(await requireConfirm('Möchten Sie diesen Slot wirklich löschen?', 'Slot löschen')))
        return;

    try {
        await apiSlots.delete(`/${slot.id}`);
        toast.add({ color: 'success', title: 'Slot gelöscht' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Slot nicht löschen',
        });
    }
}

onMounted(load);
</script>

<template>
    <h2 class="mt-6">Slots</h2>

    <template v-if="loading">
        <div>Lade …</div>
    </template>

    <template v-else>
        <Grid v-if="slots.length">
            <GridEditRow
                v-for="s in slots"
                :key="s.id"
                :canDelete="true"
                @update="updateSlot(s)"
                @delete="deleteSlot(s)"
            >
                <template #body>
                    <UTooltip text="PDFs aller Profunda (experimentell)">
                        <UButton
                            :href="`/api/profundum/management/instanz/${s.id}.zip`"
                            external
                            aria-label="PDFs aller Profunda (experimentell)"
                            color="info"
                            download
                            icon="i-lucide-file-text"
                            size="sm"
                            variant="ghost"
                        />
                    </UTooltip>
                    <UTooltip text="Termine verwalten">
                        <UButton
                            aria-label="Termine verwalten"
                            color="neutral"
                            icon="i-lucide-calendar-days"
                            size="sm"
                            variant="ghost"
                            @click="openTermineDialog(s)"
                        />
                    </UTooltip>
                    {{ s.jahr }} / {{ s.jahr + 1 }}, {{ s.quartal }},
                    {{
                        weekdayOptions.find((d) => d.value === s.wochentag)?.label ??
                        s.wochentag
                    }}, Einwahl:
                    {{ zeitraeume.find((z) => z.id === s.einwahlZeitraumId)?.bezeichnung }}
                </template>

                <template #edit>
                    <div class="flex flex-col gap-2 w-full border border-muted rounded-md p-4">
                        <UFormField label="Jahr" required>
                            <UInputNumber
                                v-model="s.jahr"
                                :format-options="{ useGrouping: false }"
                                :min="2020"
                                class="w-full"
                                color="neutral"
                            />
                        </UFormField>

                        <UFormField label="Quartal" required>
                            <USelect
                                v-model="s.quartal"
                                :items="['Q1', 'Q2', 'Q3', 'Q4']"
                                class="w-full"
                            />
                        </UFormField>

                        <UFormField label="Wochentag" required>
                            <USelect
                                v-model="s.wochentag"
                                :items="weekdayOptions"
                                label-key="label"
                                value-key="value"
                                class="w-full"
                            />
                        </UFormField>

                        <UFormField label="Einwahlzeitraum" required>
                            <USelect
                                v-model="s.einwahlZeitraumId"
                                :items="zeitraeume"
                                label-key="bezeichnung"
                                value-key="id"
                                placeholder="Zeitraum auswählen"
                                class="w-full"
                            />
                        </UFormField>
                    </div>
                </template>
            </GridEditRow>
        </Grid>

        <div v-else>Keine Slots vorhanden.</div>

        <UButton
            icon="i-lucide-plus"
            label="Neuer Slot"
            class="mt-4"
            @click="openCreateDialog"
        />
    </template>
</template>

<style scoped></style>
