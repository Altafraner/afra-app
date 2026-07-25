<script setup>
import { mande } from 'mande';
import { onMounted, ref } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { formatSlot, formatStudent } from '@/helpers/formatters.ts';
import InstanzForm from '@/Profundum/components/Forms/InstanzForm.vue';

const props = defineProps({ profundumId: String });
const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();

const apiInstanz = mande('/api/profundum/management/instanz');
const apiSlots = mande('/api/profundum/management/slot');

const instanzen = ref([]);
const slots = ref([]);
const loading = ref(true);

async function load() {
    slots.value = (await apiSlots.get()).map((slot) => ({
        ...slot,
        label: formatSlot(slot),
    }));
    instanzen.value = (await apiInstanz.get()).filter(
        (x) => x.profundumId === props.profundumId,
    );
    loading.value = false;
}

async function createInstanz(data) {
    try {
        await apiInstanz.post({ ...data, profundumId: props.profundumId });
        toast.add({ color: 'success', title: 'Instanz erstellt' });
        await load();
    } catch (e) {
        toast.add({ color: 'error', title: 'Fehler', description: e.body });
    }
}

const createDialog = overlay.create(InstanzForm);

async function openCreateDialog() {
    const data = await createDialog.open({ slots: slots.value, variant: 'create' });
    if (!data) return;
    await createInstanz(data);
}

async function updateInstanz(inst, data) {
    try {
        await apiInstanz.put(`/${inst.id}`, { ...inst, ...data });
        toast.add({ color: 'success', title: 'Gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Angebot nicht speichern',
        });
    }
}

const editDialog = overlay.create(InstanzForm);

async function openEditDialog(angebot) {
    const data = await editDialog.open({
        slots: slots.value,
        variant: 'edit',
        maxEinschreibungen: angebot.maxEinschreibungen,
        wantedEinschreibungen: angebot.wantedEinschreibungen,
        slotIds: angebot.slots,
        ort: angebot.ort,
        verantwortlicheIds: angebot.verantwortlicheIds ?? [],
    });
    if (!data) return;
    await updateInstanz(angebot, data);
}

async function deleteInstanz(id) {
    if (
        !(await requireConfirm(
            'Wollen Sie das Angebot wirklich löschen? Das Löschen von Angeboten mit Einschreibungen kann für Probleme bei der nächsten Einwahl sorgen.',
            'Angebot Löschen',
        ))
    )
        return;
    await apiInstanz.delete(`/${id}`);
    toast.add({ color: 'success', title: 'Instanz gelöscht' });
    await load();
}

onMounted(load);
</script>

<template>
    <div class="flex justify-between mt-8 items-baseline">
        <h2>Angebote</h2>
        <UButton icon="i-lucide-plus" label="Neues Angebot" @click="openCreateDialog" />
    </div>

    <div class="grid grid-cols-[auto_auto_auto_1fr_auto] gap-4 items-baseline mt-4 text-sm">
        <template v-for="angebot in instanzen" :key="angebot.id">
            <span class="grid grid-cols-2 gap-1">
                <UBadge
                    v-for="slotId in angebot.slots"
                    :key="slotId"
                    color="neutral"
                    variant="subtle"
                    class="text-sm px-1.5"
                >
                    {{ slots.find((s) => s.id === slotId)?.label }}
                </UBadge>
            </span>
            <span>
                {{ angebot.maxEinschreibungen }} Plätze
                <template v-if="angebot.wantedEinschreibungen">
                    (Ziel: {{ angebot.wantedEinschreibungen }})
                </template>
            </span>
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
                <UTooltip text="PDF (experimentell)">
                    <UButton
                        :href="`/api/profundum/management/instanz/${angebot.id}.pdf`"
                        aria-label="PDF (experimentell)"
                        color="info"
                        download
                        icon="i-lucide-file-text"
                        size="sm"
                        variant="ghost"
                    />
                </UTooltip>
                <UTooltip text="Angebot bearbeiten">
                    <UButton
                        aria-label="Angebot bearbeiten"
                        color="neutral"
                        icon="i-lucide-pencil"
                        size="sm"
                        variant="ghost"
                        @click="openEditDialog(angebot)"
                    />
                </UTooltip>
                <UTooltip text="Angebot löschen">
                    <UButton
                        aria-label="Angebot löschen"
                        color="error"
                        icon="i-lucide-trash"
                        size="sm"
                        variant="ghost"
                        @click="deleteInstanz(angebot.id)"
                    />
                </UTooltip>
            </span>
        </template>
    </div>
</template>

<style scoped></style>
