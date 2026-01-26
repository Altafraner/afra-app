<script lang="ts" setup>
import { inject, type Ref } from 'vue';
import type { DynamicDialogInstance } from 'primevue/dynamicdialogoptions';
import type { SimpleTextDialogModel } from '@/components/Form/simpleTextDialogModel';
import { Form, type FormResolverOptions, type FormSubmitEvent } from '@primevue/forms';
import { Button, FloatLabel, InputText, Message } from 'primevue';

const dialogRef = inject<Ref<DynamicDialogInstance, DynamicDialogInstance>>('dialogRef');
const data: SimpleTextDialogModel = dialogRef.value.data;

const initialValue = {
    input: data.default ?? '',
};

const resolver = (e: FormResolverOptions): Record<string, any> => {
    const errors: Record<string, string[]> = {
        input: [],
    };

    if (
        data.minLength !== 0 &&
        data.minLength !== undefined &&
        e.values['input'].length < data.minLength
    ) {
        errors['input'].push(`Es werden mindestens ${data.minLength} Zeichen benötigt.`);
    }

    if (
        data.maxLength !== 0 &&
        data.maxLength !== undefined &&
        e.values['input'].length > data.maxLength
    ) {
        errors['input'].push(`Es sind maximal ${data.maxLength} Zeichen erlaubt.`);
    }

    return { values: e.values, errors };
};

const submit = (evt: FormSubmitEvent) => {
    if (!evt.valid) return;
    dialogRef.value.close({ result: evt.values['input'] });
};
</script>

<template>
    <Form
        v-slot="$form"
        :initial-values="initialValue"
        :resolver="resolver"
        class="flex flex-col gap-2"
        @submit="submit"
    >
        <FloatLabel variant="on">
            <label for="input">{{ data.label ?? 'Eingabe' }}</label>
            <InputText id="input" autofocus fluid name="input" />
        </FloatLabel>
        <Message v-if="$form.input?.invalid" severity="error" size="small" variant="simple">
            {{ $form.input.error }}
        </Message>
        <Button
            :label="data.buttonLabel ?? 'Absenden'"
            :severity="data.buttonSeverity ?? 'primary'"
            fluid
            type="submit"
        />
    </Form>
</template>

<style scoped></style>
