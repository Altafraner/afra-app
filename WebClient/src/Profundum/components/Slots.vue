<script setup>
import { ref, onMounted, computed } from 'vue';
import { mande } from 'mande';
import { Button, Dropdown, InputNumber, Dialog, useToast } from 'primevue';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';

const toast = useToast();

const apiSlots = mande('/api/profundum/management/slot');
const apiZeitraeume = mande('/api/profundum/management/einwahlzeitraum');

const slots = ref([]);
const zeitraeume = ref([]);
const loading = ref(true);

const dialogOpen = ref(false);
const createModel = ref({
    jahr: new Date().getFullYear(),
    quartal: 1,
    wochentag: 1,
    einwahlZeitraumId: null,
});

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

async function createSlot() {
    try {
        await apiSlots.post({
            jahr: createModel.value.jahr,
            quartal: createModel.value.quartal,
            wochentag: createModel.value.wochentag,
            einwahlZeitraumId: createModel.value.einwahlZeitraumId,
        });

        toast.add({ severity: 'success', summary: 'Slot angelegt' });
        dialogOpen.value = false;

        createModel.value = {
            jahr: new Date().getFullYear(),
            quartal: 1,
            wochentag: 1,
            einwahlZeitraumId: null,
        };

        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Slot nicht speichern',
        });
    }
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

        toast.add({ severity: 'success', summary: 'Slot gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Slot nicht speichern',
        });
    }
}

async function deleteSlot(slot) {
    if (!confirm('Möchten Sie diesen Slot wirklich löschen?')) return;

    try {
        await apiSlots.delete(`/${slot.id}`);
        toast.add({ severity: 'success', summary: 'Slot gelöscht' });
        await load();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: e?.body ?? 'Konnte Slot nicht löschen',
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
                <Button
                    as="a"
                    :href="`/api/profundum/management/instanz/${s.id}.zip`"
                    icon="pi pi-file-pdf"
                    variant="text"
                    size="small"
                    download
                    severity="info"
                    v-tooltip.left="'PDFs aller Profunda (experimentell)'"
                    aria-label="PDFs aller Profunda (experimentell)'"
                />
                    Jahr: {{ s.jahr }}, Quartal: {{ s.quartal }}, Wochentag:
                    {{
                        weekdayOptions.find((d) => d.value === s.wochentag)?.label ??
                        s.wochentag
                    }}, Einwahl:
                    {{
                        zeitraeume.find((z) => z.id === s.einwahlZeitraumId)?.einwahlStart ??
                        '–'
                    }}
                </template>

                <template #edit>
                    <div class="flex flex-col gap-2 w-full">
                        <div>
                            <label class="block mb-1">Jahr</label>
                            <InputNumber
                                v-model.number="s.jahr"
                                :min="2020"
                                class="w-full"
                                :useGrouping="false"
                            />
                        </div>

                        <div>
                            <label class="block mb-1">Quartal</label>
                            <Dropdown
                                v-model="s.quartal"
                                :options="['Q1', 'Q2', 'Q3', 'Q4']"
                                placeholder="Quartal"
                                class="w-full"
                            />
                        </div>

                        <div>
                            <label class="block mb-1">Wochentag</label>
                            <Dropdown
                                v-model="s.wochentag"
                                :options="weekdayOptions"
                                optionLabel="label"
                                optionValue="value"
                                class="w-full"
                            />
                        </div>

                        <div>
                            <label class="block mb-1">Einwahlzeitraum</label>
                            <Dropdown
                                v-model="s.einwahlZeitraumId"
                                :options="zeitraeume"
                                optionLabel="einwahlStart"
                                optionValue="id"
                                placeholder="Zeitraum auswählen"
                                class="w-full"
                            />
                        </div>
                    </div>
                </template>
            </GridEditRow>
        </Grid>

        <div v-else>Keine Slots vorhanden.</div>

        <Button icon="pi pi-plus" label="Neuer Slot" class="mt-4" @click="dialogOpen = true" />
    </template>

    <Dialog v-model:visible="dialogOpen" header="Neuer Slot" modal>
        <div class="flex flex-col gap-3 w-full">
            <div>
                <label class="block mb-1">Jahr</label>
                <InputNumber v-model.number="createModel.jahr" class="w-full" />
            </div>

            <div>
                <label class="block mb-1">Quartal</label>
                <Dropdown
                    v-model="createModel.quartal"
                    :options="['Q1', 'Q2', 'Q3', 'Q4']"
                    class="w-full"
                />
            </div>

            <div>
                <label class="block mb-1">Wochentag</label>
                <Dropdown
                    v-model="createModel.wochentag"
                    :options="weekdayOptions"
                    optionLabel="label"
                    optionValue="value"
                    class="w-full"
                />
            </div>

            <div>
                <label class="block mb-1">Einwahlzeitraum</label>
                <Dropdown
                    v-model="createModel.einwahlZeitraumId"
                    :options="zeitraeume"
                    optionLabel="einwahlStart"
                    optionValue="id"
                    placeholder="Zeitraum auswählen"
                    class="w-full"
                />
            </div>
        </div>

        <template #footer>
            <Button label="Abbrechen" @click="dialogOpen = false" />
            <Button label="Speichern" icon="pi pi-check" @click="createSlot" />
        </template>
    </Dialog>
</template>

<style scoped>
:deep(.p-inputtext) {
    width: 100%;
}
</style>
