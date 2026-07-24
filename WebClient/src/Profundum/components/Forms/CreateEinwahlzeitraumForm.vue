<script lang="ts" setup>
import { reactive } from 'vue';
import type { CalendarDateTime } from '@internationalized/date';
import { getLocalTimeZone } from '@internationalized/date';
import ADateTimePicker from '@/components/Form/ADateTimePicker.vue';

const emit = defineEmits<{
    close: [{ einwahlStart: string | null; einwahlStop: string | null }];
}>();

interface FormState {
    einwahlStart?: CalendarDateTime;
    einwahlStop?: CalendarDateTime;
}

const state = reactive<FormState>({
    einwahlStart: undefined,
    einwahlStop: undefined,
});

function submit() {
    emit('close', {
        einwahlStart: state.einwahlStart?.toDate(getLocalTimeZone()).toISOString() ?? null,
        einwahlStop: state.einwahlStop?.toDate(getLocalTimeZone()).toISOString() ?? null,
    });
}
</script>

<template>
    <UModal title="Neuer Einwahlzeitraum">
        <template #body>
            <div class="flex flex-col gap-4">
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
