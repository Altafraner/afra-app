<script setup>
import { reactive } from 'vue';

const props = defineProps({
    zeitraeume: { type: Array, default: () => [] },
});

const emit = defineEmits(['close']);

const weekdayOptions = [
    { label: 'Montag', value: 'Monday' },
    { label: 'Dienstag', value: 'Tuesday' },
    { label: 'Mittwoch', value: 'Wednesday' },
    { label: 'Donnerstag', value: 'Thursday' },
    { label: 'Freitag', value: 'Friday' },
    { label: 'Samstag', value: 'Saturday' },
    { label: 'Sonntag', value: 'Sunday' },
];

const state = reactive({
    jahr: new Date().getFullYear(),
    quartal: 'Q1',
    wochentag: 'Monday',
    einwahlZeitraumId: null,
});

function validate(state) {
    const errors = [];
    if (!state.jahr) errors.push({ name: 'jahr', message: 'Bitte geben Sie ein Jahr an.' });
    if (!state.einwahlZeitraumId)
        errors.push({
            name: 'einwahlZeitraumId',
            message: 'Bitte wählen Sie einen Einwahlzeitraum.',
        });
    return errors;
}

function submit(event) {
    emit('close', {
        jahr: event.data.jahr,
        quartal: event.data.quartal,
        wochentag: event.data.wochentag,
        einwahlZeitraumId: event.data.einwahlZeitraumId,
    });
}
</script>

<template>
    <UModal title="Neuer Slot">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Jahr" name="jahr" required>
                    <UInputNumber v-model="state.jahr" :min="2020" class="w-full" />
                </UFormField>
                <UFormField label="Quartal" name="quartal" required>
                    <USelect
                        v-model="state.quartal"
                        :items="['Q1', 'Q2', 'Q3', 'Q4']"
                        class="w-full"
                    />
                </UFormField>
                <UFormField label="Wochentag" name="wochentag" required>
                    <USelect
                        v-model="state.wochentag"
                        :items="weekdayOptions"
                        label-key="label"
                        value-key="value"
                        class="w-full"
                    />
                </UFormField>
                <UFormField label="Einwahlzeitraum" name="einwahlZeitraumId" required>
                    <USelect
                        v-model="state.einwahlZeitraumId"
                        :items="props.zeitraeume"
                        label-key="einwahlStart"
                        value-key="id"
                        placeholder="Zeitraum auswählen"
                        class="w-full"
                    />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" type="submit" />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
