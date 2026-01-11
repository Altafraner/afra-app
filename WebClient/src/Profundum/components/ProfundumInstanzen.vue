<script setup>
import { mande } from 'mande';
import { ref, onMounted } from 'vue';
import {
    InputText,
    InputNumber,
    Button,
    MultiSelect,
    useToast,
    Tag,
    FloatLabel,
} from 'primevue';
import { useConfirmPopover } from '@/composables/confirmPopover.js';
import Dialog from 'primevue/dialog';
import { formatStudent } from '@/helpers/formatters.ts';
import AfraPersonSelector from '@/Otium/components/Form/AfraPersonSelector.vue';

const props = defineProps({ profundumId: String });
const toast = useToast();
const confirm = useConfirmPopover();

const apiInstanz = mande('/api/profundum/management/instanz');
const apiSlots = mande('/api/profundum/management/slot');

const instanzen = ref([]);
const slots = ref([]);
const loading = ref(true);

const newInstanz = ref({
    profundumId: props.profundumId,
    maxEinschreibungen: 15,
    slots: [],
    verantwortliche: [],
    ort: '',
});

const dialogVisible = ref(null);
const createDialogVisible = ref(false);

async function load() {
    slots.value = (await apiSlots.get()).map((slot) => ({
        ...slot,
        label: `${slot.wochentag} ${slot.quartal} ${slot.jahr}`,
    }));
    instanzen.value = (await apiInstanz.get()).filter(
        (x) => x.profundumId === props.profundumId,
    );
    loading.value = false;
}

async function createInstanz() {
    try {
        const id = await apiInstanz.post(newInstanz.value);
        toast.add({ severity: 'success', summary: 'Instanz erstellt' });
        newInstanz.value = {
            profundumId: props.profundumId,
            maxEinschreibungen: 15,
            slots: [],
            ort: '',
        };
        await load();
    } catch (e) {
        toast.add({ severity: 'error', summary: 'Fehler', detail: e.body });
    } finally {
        createDialogVisible.value = false;
    }
}

async function updateInstanz(inst) {
    dialogVisible.value = true;
    await apiInstanz.put(`/${inst.id}`, inst);
    toast.add({ severity: 'success', summary: 'Gespeichert', life: 10000 });
    await load();
}

function deleteInstanz(event, id) {
    confirm.openConfirmDialog(
        event,
        doDelete,
        'Angebot Löschen',
        'Wollen Sie das Angebot wirklich löschen? Das Löschen von Angeboten mit Einschreibungen kann für Probleme bei der nächsten Einwahl sorgen.',
        'danger',
    );
    async function doDelete() {
        await apiInstanz.delete(`/${id}`);
        toast.add({ severity: 'success', summary: 'Instanz gelöscht' });
        await load();
    }
}

onMounted(load);
</script>

<template>
    <div class="flex justify-between mt-8 items-baseline">
        <h2>Angebote</h2>
        <Button icon="pi pi-plus" label="Neues Angebot" @click="createDialogVisible = true" />
    </div>
    <Dialog v-model:visible="createDialogVisible" header="Neues Angebot erstellen" modal>
        <div class="flex gap-2 flex-col mt-2">
            <FloatLabel variant="on">
                <InputNumber
                    v-model="newInstanz.maxEinschreibungen"
                    placeholder="max. Schüler"
                    class="multiselect-wrap"
                    showButtons
                    buttonLayout="horizontal"
                    fluid
                    id="newSpace"
                >
                    <template #incrementbuttonicon>
                        <i class="pi pi-plus" />
                    </template>
                    <template #decrementbuttonicon>
                        <i class="pi pi-minus" />
                    </template>
                </InputNumber>
                <label for="newSpace">Plätze</label>
            </FloatLabel>

            <FloatLabel variant="on">
                <MultiSelect
                    id="newSlots"
                    v-model="newInstanz.slots"
                    :options="slots"
                    optionLabel="label"
                    optionValue="id"
                    placeholder="Slots auswählen"
                    display="chip"
                    class="multiselect-wrap"
                    fluid
                />
                <label for="newSlots">Slots</label>
            </FloatLabel>

            <FloatLabel variant="on">
                <InputText id="newOrt" v-model="newInstanz.ort" maxlength="20" fluid />
                <label for="newOrt">Ort</label>
            </FloatLabel>

            <AfraPersonSelector
                v-model="newInstanz.verantwortlicheIds"
                multi
                name="tutor"
                class="multiselect-wrap"
                fluid
                id="newVerantwortliche"
            >
                <template #label>Verantwortliche</template>
            </AfraPersonSelector>

            <Button label="Anlegen" icon="pi pi-plus" @click="createInstanz" fluid />
        </div>
    </Dialog>

    <div class="grid grid-cols-[auto_auto_auto_1fr_auto] gap-4 items-baseline mt-4 text-sm">
        <template v-for="angebot in instanzen">
            <span class="grid grid-cols-2 gap-1">
                <Tag
                    severity="secondary"
                    v-for="slotId in angebot.slots"
                    :key="slotId"
                    class="text-sm px-1.5"
                >
                    {{ slots.find((s) => s.id === slotId)?.label }}
                </Tag>
            </span>
            <span> {{ angebot.maxEinschreibungen }} Plätze </span>
            <span> Raum: {{ angebot.ort ? angebot.ort : '–' }} </span>
            <span>
                <template v-if="(angebot.verantwortlicheInfo?.length ?? 0) === 0">
                    Keine Verantwortlichen
                </template>
                <template v-else>
                    {{ angebot.verantwortlicheInfo.map((v) => formatStudent(v)).join(', ') }}
                </template>
            </span>
            <span class="inline-flex gap-2 items-baseline">
                <Button
                    as="a"
                    :href="`/api/profundum/management/instanz/${angebot.id}.pdf`"
                    icon="pi pi-file-pdf"
                    variant="text"
                    size="small"
                    download
                    severity="info"
                    v-tooltip.left="'PDF (experimentell)'"
                    aria-label="PDF (experimentell)'"
                />
                <Button
                    icon="pi pi-pencil"
                    variant="text"
                    size="small"
                    severity="secondary"
                    v-tooltip.left="'Angebot bearbeiten'"
                    aria-label="Angebot bearbeiten"
                    @click="dialogVisible = angebot.id"
                />
                <Button
                    icon="pi pi-trash"
                    variant="text"
                    size="small"
                    severity="danger"
                    v-tooltip.left="'Angebot löschen'"
                    aria-label="Angebot löschen"
                    @click="deleteInstanz($event, angebot.id)"
                />
                <Dialog
                    :visible="dialogVisible === angebot.id"
                    header="Angebot bearbeiten"
                    modal
                >
                    <div class="flex flex-col gap-2 mt-2">
                        <FloatLabel variant="on">
                            <InputText
                                :id="`max-${angebot.id}`"
                                type="number"
                                v-model="angebot.maxEinschreibungen"
                                placeholder="max. Schüler"
                            />
                            <label :id="`max-${angebot.id}`">Plätze</label>
                        </FloatLabel>

                        <FloatLabel variant="on">
                            <MultiSelect
                                :id="`slots-${angebot.id}`"
                                v-model="angebot.slots"
                                :options="slots"
                                optionLabel="label"
                                optionValue="id"
                                filter
                                placeholder="Slots auswählen"
                                class="multiselect-wrap"
                            />
                            <label :id="`slots-${angebot.id}`">Slots</label>
                        </FloatLabel>

                        <FloatLabel variant="on">
                            <InputText
                                :id="`ort-${angebot.id}`"
                                v-model="angebot.ort"
                                maxlength="20"
                                fluid
                            />
                            <label :for="`ort-${angebot.id}`">Ort</label>
                        </FloatLabel>

                        <AfraPersonSelector
                            v-model="angebot.verantwortlicheIds"
                            multi
                            name="tutor"
                            class="multiselect-wrap"
                            fluid
                            :id="`verantwortliche-${angebot.id}`"
                        >
                            <template #label>Verantwortliche</template>
                        </AfraPersonSelector>

                        <Button label="Speichern" @click="updateInstanz(angebot)" />
                    </div>
                </Dialog>
            </span>
        </template>
    </div>
</template>
<style scoped>
.multiselect-wrap :deep(.p-multiselect-label-container) {
    height: auto;
}

.multiselect-wrap :deep(.p-multiselect-label) {
    display: flex;
    flex-wrap: wrap;
    white-space: normal;
    gap: 0.25rem;
    padding-top: 0.25rem;
    padding-bottom: 0.25rem;
}

.multiselect-wrap :deep(.p-multiselect-token) {
    margin-bottom: 0.25rem;
}
</style>
