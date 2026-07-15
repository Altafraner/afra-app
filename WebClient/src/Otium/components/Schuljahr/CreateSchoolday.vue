<script lang="ts" setup>
import { reactive } from 'vue';
import { CalendarDate } from '@internationalized/date';
import { FormError, FormSubmitEvent } from '@nuxt/ui';
import ADatePicker from '@/components/Form/ADatePicker.vue';

const emit = defineEmits<{
    close: [
        {
            datum: string;
            wochentyp: string;
            blocks: string[];
        }[],
    ];
}>();

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (!state.date) {
        errors.push({ name: 'date', message: 'Bitte geben Sie ein Datum an.' });
    }

    if (!state.type) {
        errors.push({ name: 'type', message: 'Bitte wählen Sie einen Wochentyp.' });
    }

    return errors;
}

async function submit(event: FormSubmitEvent<FormSchema>) {
    const data = [];

    data.push({
        datum: event.data.date!.toString(),
        wochentyp: event.data.type!,
        blocks: [] as string[],
    });

    emit('close', data);
}

interface FormSchema {
    date: CalendarDate | undefined;
    type: string | undefined;
}

const state = reactive<FormSchema>({ date: undefined, type: undefined });
</script>

<template>
    <UModal title="Schultag hinzufügen">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Datum" name="date" required>
                    <ADatePicker
                        v-model="state.date as CalendarDate | undefined"
                        class="w-full"
                    />
                </UFormField>
                <UFormField label="Wochentyp" name="type" required>
                    <USelect
                        v-model="state.type"
                        :items="['H-Woche', 'N-Woche']"
                        class="w-full"
                        placeholder="Wochentyp wählen"
                    />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" type="submit" />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
