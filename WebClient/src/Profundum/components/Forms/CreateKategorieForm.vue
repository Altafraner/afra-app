<script setup>
import { reactive } from 'vue';

const emit = defineEmits(['close']);

const state = reactive({
    bezeichnung: '',
    profilProfundum: false,
});

function validate(state) {
    const errors = [];
    if (!state.bezeichnung || state.bezeichnung.trim().length === 0) {
        errors.push({ name: 'bezeichnung', message: 'Bitte geben Sie eine Bezeichnung an.' });
    }
    if (state.bezeichnung?.length > 50) {
        errors.push({ name: 'bezeichnung', message: 'Bitte geben Sie max. 50 Zeichen ein.' });
    }
    return errors;
}

function submit(event) {
    emit('close', {
        bezeichnung: event.data.bezeichnung.trim(),
        profilProfundum: event.data.profilProfundum,
    });
}
</script>

<template>
    <UModal title="Neue Kategorie">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Bezeichnung" name="bezeichnung" required>
                    <UInput v-model="state.bezeichnung" class="w-full" maxlength="50" />
                </UFormField>
                <UFormField name="profilProfundum">
                    <USwitch v-model="state.profilProfundum" label="Profilprofundum" />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" type="submit" />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
