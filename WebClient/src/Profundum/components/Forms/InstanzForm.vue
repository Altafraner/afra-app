<script setup>
import { reactive } from 'vue';
import PersonSelector from '@/components/PersonSelector.vue';

const props = defineProps({
    slots: { type: Array, default: () => [] },
    maxEinschreibungen: { type: Number, default: 15 },
    slotIds: { type: Array, default: () => [] },
    ort: { type: String, default: '' },
    verantwortlicheIds: { type: Array, default: () => [] },
    variant: { type: String, default: 'create' },
});

const emit = defineEmits(['close']);

const state = reactive({
    maxEinschreibungen: props.maxEinschreibungen,
    slotIds: [...props.slotIds],
    ort: props.ort,
    verantwortlicheIds: [...props.verantwortlicheIds],
});

function validate(state) {
    const errors = [];
    if (!state.maxEinschreibungen || state.maxEinschreibungen < 1) {
        errors.push({
            name: 'maxEinschreibungen',
            message: 'Bitte geben Sie die Platzzahl an.',
        });
    }
    return errors;
}

function submit(event) {
    emit('close', {
        maxEinschreibungen: event.data.maxEinschreibungen,
        slots: event.data.slotIds,
        ort: event.data.ort?.trim() ?? '',
        verantwortlicheIds: event.data.verantwortlicheIds,
    });
}
</script>

<template>
    <UModal :title="variant === 'create' ? 'Neues Angebot erstellen' : 'Angebot bearbeiten'">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Plätze" name="maxEinschreibungen" required>
                    <UInputNumber v-model="state.maxEinschreibungen" :min="1" class="w-full" />
                </UFormField>
                <UFormField label="Slots" name="slotIds">
                    <USelect
                        v-model="state.slotIds"
                        :items="props.slots"
                        label-key="label"
                        value-key="id"
                        multiple
                        placeholder="Slots auswählen"
                        class="w-full"
                    />
                </UFormField>
                <UFormField label="Ort" name="ort">
                    <UInput v-model="state.ort" maxlength="20" class="w-full" />
                </UFormField>
                <UFormField label="Verantwortliche" name="verantwortlicheIds">
                    <PersonSelector
                        v-model="state.verantwortlicheIds"
                        multiple
                        class="w-full"
                    />
                </UFormField>
                <UButton
                    :icon="variant === 'create' ? 'i-lucide-plus' : 'i-lucide-check'"
                    :label="variant === 'create' ? 'Anlegen' : 'Speichern'"
                    type="submit"
                />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
