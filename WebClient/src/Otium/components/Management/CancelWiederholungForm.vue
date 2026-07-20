<script lang="ts" setup>
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { ref } from 'vue';
import OtiumDateSelector from '@/Otium/components/Form/OtiumDateSelector.vue';

const props = defineProps({
    wiederholung: {
        type: Object,
        required: true,
    },
});
const emit = defineEmits<{
    close: [string];
}>();

const settings = useOtiumStore();
const end = ref<string | null>(null);
const loading = ref(true);

const today = new Date();
const startDate = new Date(props.wiederholung.startDate);
const endDate = new Date(props.wiederholung.endDate);
const datesAvailable = (settings.schuljahr ?? ([] as any[])).filter((day) => {
    const datum = new Date(day.datum);
    return (
        datum.getDay() === props.wiederholung.wochentag &&
        day.wochentyp === props.wiederholung.wochentyp &&
        datum >= startDate &&
        datum >= today &&
        datum < endDate
    );
});

const canBeShortened = datesAvailable.length > 0;

function onSubmit() {
    emit('close', end.value!);
}

async function setup() {
    await settings.updateSchuljahr();
    if (canBeShortened) end.value = datesAvailable[datesAvailable.length - 1].datum;
    loading.value = false;
}

setup();
</script>

<template>
    <UModal title="Wiederholung einkürzen">
        <template #body>
            <template v-if="!loading">
                <div v-if="canBeShortened" class="flex flex-col gap-3">
                    <UFormField label="Neuer letzter Termin" required>
                        <OtiumDateSelector
                            v-model="end"
                            :options="datesAvailable"
                            full-size
                            hide-today
                        />
                    </UFormField>
                    <UButton color="error" label="Wiederholung einkürzen" @click="onSubmit" />
                </div>
                <UAlert
                    v-else
                    color="error"
                    icon="i-lucide-triangle-alert"
                    title="Einkürzen nicht möglich"
                    variant="subtle"
                >
                    <template #description>
                        <p>
                            Die Wiederholung kann nicht weiter gekürzt werden, weil nach Heute
                            keine Termine mehr bestehen.
                        </p>
                        <p class="my-2">
                            Sollte für heute noch ein Termin geplant sein, können Sie diesen
                            absagen.
                        </p>
                        <p>
                            Sollte diese Wiederholung aus keinen Terminen bestehen, können Sie
                            diese löschen.
                        </p>
                    </template>
                </UAlert>
            </template>
        </template>
        <template #footer>
            <div class="text-muted text-sm">
                Durch das Einkürzen der Wiederholung werden alle Termine nach dem neuen Enddatum
                abgesagt und gelöscht.
            </div>
        </template>
    </UModal>
</template>

<style scoped></style>
