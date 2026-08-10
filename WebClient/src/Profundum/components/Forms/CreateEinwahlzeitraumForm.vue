<script lang="ts" setup>
import { reactive } from 'vue';
import type { CalendarDateTime } from '@internationalized/date';
import { getLocalTimeZone } from '@internationalized/date';
import ADateTimePicker from '@/components/Form/ADateTimePicker.vue';

const emit = defineEmits<{
    close: [{ bezeichnung: string; einwahlStart: string | null; einwahlStop: string | null }];
}>();

interface FormState {
    bezeichnung: string;
    einwahlStart?: CalendarDateTime;
    einwahlStop?: CalendarDateTime;
}

const state = reactive<FormState>({
    bezeichnung: '',
    einwahlStart: undefined,
    einwahlStop: undefined,
});

function submit() {
    emit('close', {
        bezeichnung: state.bezeichnung,
        einwahlStart: state.einwahlStart?.toDate(getLocalTimeZone()).toISOString() ?? null,
        einwahlStop: state.einwahlStop?.toDate(getLocalTimeZone()).toISOString() ?? null,
    });
}
</script>

<template>
    <UModal title="Neuer Einwahlzeitraum">
        <template #body>
            <div class="flex flex-col gap-4">
                <UFormField label="Bezeichnung">
                    <UInput v-model="state.bezeichnung" class="w-full"/>
                </UFormField>
                <UFormField label="Start">
                    <ADateTimePicker
                        v-model="state.einwahlStart as CalendarDateTime | undefined"
                    />
                </UFormField>
                <UFormField label="Ende">
                    <ADateTimePicker
                        v-model="state.einwahlStop as CalendarDateTime | undefined"
                    />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" @click="submit" />
            </div>
        </template>
    </UModal>
</template>

<style scoped></style>
