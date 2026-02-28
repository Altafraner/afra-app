<script lang="ts" setup>
import { reactive } from 'vue';
import { FormError, FormSubmitEvent } from '@nuxt/ui';

const props = defineProps<{
    title: string;
    description?: string;
    default?: string;
    maxLength?: number;
    minLength?: number;
    placeholder: string;
    label: string;
    buttonText?: string;
    buttonColor?: string;
    disclaimer?: string;
}>();

const emit = defineEmits<{
    close: [value: string | undefined];
}>();

interface FormSchema {
    value: string | undefined;
}

const state = reactive<FormSchema>({
    value: props.default ?? '',
});

function validate(state: Partial<FormSchema>): FormError[] {
    const errors: FormError[] = [];

    if (
        props.minLength !== undefined &&
        props.minLength > 0 &&
        (state.value === undefined || state.value === '')
    )
        errors.push({ name: 'value', message: `Geben Sie einen Wert ein` });
    if (
        props.minLength !== undefined &&
        props.minLength > 0 &&
        state.value !== undefined &&
        state.value.length < props.minLength
    )
        errors.push({
            name: 'value',
            message: `Geben Sie mindestens ${props.minLength} Zeichen ein`,
        });
    if (
        props.maxLength !== undefined &&
        props.maxLength > 0 &&
        state.value !== undefined &&
        state.value.length > props.maxLength
    )
        errors.push({
            name: 'value',
            message: `Geben Sie maximal ${props.maxLength} Zeichen ein`,
        });

    return errors;
}

function submit(event: FormSubmitEvent<FormSchema>) {
    emit('close', event.data.value);
}
</script>

<template>
    <UModal :description="description" :title="title">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField
                    :label="label"
                    :required="minLength !== undefined && minLength > 0"
                    name="value"
                >
                    <UInput v-model="state.value" :placeholder="placeholder" class="w-full" />
                </UFormField>
                <UButton
                    :color="buttonColor"
                    :label="buttonText ?? 'Bestätigen'"
                    type="submit"
                />
            </UForm>
        </template>
        <template v-if="disclaimer" #footer>
            <span class="text-sm text-muted">{{ disclaimer }}</span>
        </template>
    </UModal>
</template>

<style scoped></style>
