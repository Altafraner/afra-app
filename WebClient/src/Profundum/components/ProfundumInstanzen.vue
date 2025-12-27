<script setup>
import { mande } from 'mande';
import { ref, onMounted } from 'vue';
import { InputText, Button, MultiSelect, useToast, Tag } from 'primevue';
import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';

const props = defineProps({ profundumId: String });
const toast = useToast();

const apiInstanz = mande('/api/profundum/management/instanz');
const apiSlots = mande('/api/profundum/management/slot');

const instanzen = ref([]);
const slots = ref([]);
const loading = ref(true);

const newInstanz = ref({
    profundumId: props.profundumId,
    maxEinschreibungen: 12,
    slots: [],
});

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
        await load();
    } catch (e) {
        toast.add({ severity: 'error', summary: 'Fehler', detail: e.body });
    }
}

async function updateInstanz(inst) {
    await apiInstanz.put(`/${inst.id}`, inst);
    toast.add({ severity: 'success', summary: 'Gespeichert' });
}

async function deleteInstanz(id) {
    await apiInstanz.delete(`/${id}`);
    toast.add({ severity: 'success', summary: 'Instanz gelöscht' });
    await load();
}

onMounted(load);
</script>

<template>
    <h2>Instanzen</h2>

    <h3>Neue Instanz</h3>
    <div class="flex gap-2 items-center mb-6">
        <InputText
            type="number"
            v-model="newInstanz.maxEinschreibungen"
            placeholder="max. Schüler"
            class="w-32"
        />

        <MultiSelect
            v-model="newInstanz.slots"
            :options="slots"
            optionLabel="label"
            optionValue="id"
            placeholder="Slots auswählen"
            class="w-140"
            display="chip"
        />

        <Button label="Anlegen" icon="pi pi-plus" @click="createInstanz" />
    </div>

    <Grid>
        <GridEditRow
            v-for="inst in instanzen"
            :key="inst.id"
            :header="`Instanz`"
            @update="updateInstanz(inst)"
            @delete="deleteInstanz(inst.id)"
            can-delete
        >
            <template #body>
                <span class="inline-flex gap-4">
                    <Tag severity="warn"> {{ inst.maxEinschreibungen }} Plätze </Tag>

                    <template v-for="slotId in inst.slots" :key="slotId">
                        <Tag> {{ slots.find((s) => s.id === slotId)?.label }} </Tag>
                    </template>
                </span>
            </template>

            <template #edit>
                <div class="flex flex-col gap-2">
                    <label>Plätze: </label>
                    <InputText
                        type="number"
                        v-model="inst.maxEinschreibungen"
                        placeholder="max. Schüler"
                    />

                    <label>Slots: </label>
                    <MultiSelect
                        v-model="inst.slots"
                        :options="slots"
                        optionLabel="label"
                        optionValue="id"
                        filter
                        placeholder="Slots auswählen"
                    />

                    <label>Voraussetzungen: </label>
                    <MultiSelect
                        v-model="inst.dependencyIds"
                        :options="instanzen.filter(i=>i.id !== inst.id).map(
                            i => (
                            { ...i, slotLabels: `Instanz: ${i.maxEinschreibungen} Plätze,
                            ${i.slots.map(s=>slots.find((s2)=>s2.id===s)).map((s)=>s.label) } Slots
                            `}))"
                        placeholder="Benötigt zuerst"
                        optionLabel="slotLabels"
                        optionValue="id"
                        display="chip"
                    />
                </div>
            </template>
        </GridEditRow>
    </Grid>
</template>
