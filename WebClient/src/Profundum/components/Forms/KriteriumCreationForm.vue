<script lang="ts" setup>
import { inject, ref, type Ref } from 'vue';
import type { DynamicDialogInstance } from 'primevue/dynamicdialogoptions';
import { Form, type FormResolverOptions, type FormSubmitEvent } from '@primevue/forms';
import { Button, FloatLabel, InputText, Message, MultiSelect } from 'primevue';
import type { KriteriumCreationModel } from '@/Profundum/components/Forms/kriteriumCreationModel';
import { useManagement } from '@/Profundum/composables/verwaltung';
import type { ProfundumKategorie } from '@/Profundum/models/verwaltung';

const dialogRef = inject<Ref<DynamicDialogInstance, DynamicDialogInstance>>('dialogRef');
const data: KriteriumCreationModel = dialogRef.value.data;

const initialValue = {
    label: data.label ?? '',
    categories: data.categories ?? [],
};

const verwaltung = useManagement();

const kategorien = ref<ProfundumKategorie[]>([]);
const kategorienLoading = ref(true);
verwaltung.getKategorien().then((result) => {
    kategorien.value = result;
    kategorienLoading.value = false;
});

const resolver = (e: FormResolverOptions): Record<string, any> => {
    const errors: Record<string, string[]> = {
        label: [],
        categories: [],
    };

    if (e.values['label'].length < 2) {
        errors['label'].push(`Es werden mindestens 2 Zeichen benötigt.`);
    }

    if (e.values['label'].length > 200) {
        errors['label'].push(`Es sind maximal 200 Zeichen erlaubt.`);
    }

    if (e.values['categories'].length === 0) {
        errors['categories'].push('Es muss mindestens eine Kategorie angegeben werden!');
    }

    return { values: e.values, errors };
};

const submit = (evt: FormSubmitEvent) => {
    if (!evt.valid) return;
    dialogRef.value.close({
        label: evt.values['label'],
        categories: evt.values['categories'],
    });
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
            <label for="label">Bezeichnung</label>
            <InputText id="label" fluid name="label" />
        </FloatLabel>
        <Message v-if="$form.label?.invalid" severity="error" size="small" variant="simple">
            {{ $form.label.error }}
        </Message>
        <FloatLabel variant="on">
            <MultiSelect
                id="kategorien"
                :loading="kategorienLoading"
                :options="kategorien"
                fluid
                name="categories"
                optionLabel="bezeichnung"
                optionValue="id"
            />
            <label for="kategorien">Findet Anwendung auf:</label>
        </FloatLabel>
        <Message
            v-if="$form.categories?.invalid"
            severity="error"
            size="small"
            variant="simple"
        >
            {{ $form.categories.error }}
        </Message>
        <Button
            :label="data.variant == 'create' ? 'Erstellen' : 'Speichern'"
            fluid
            severity="primary"
            type="submit"
        />
    </Form>
</template>

<style scoped></style>
