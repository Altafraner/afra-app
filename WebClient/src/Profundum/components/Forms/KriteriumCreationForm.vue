<script lang="ts" setup>
import { inject, ref, type Ref } from 'vue';
import type { DynamicDialogInstance } from 'primevue/dynamicdialogoptions';
import { Form, type FormResolverOptions, type FormSubmitEvent } from '@primevue/forms';
import {
    Button,
    FloatLabel,
    InputGroup,
    InputGroupAddon,
    InputText,
    Message,
    MultiSelect,
    ToggleSwitch,
} from 'primevue';
import type { KriteriumCreationModel } from '@/Profundum/components/Forms/kriteriumCreationModel';
import { useManagement } from '@/Profundum/composables/verwaltung';
import type { ProfundumFachbereich } from '@/Profundum/models/verwaltung';

const dialogRef = inject<Ref<DynamicDialogInstance, DynamicDialogInstance>>('dialogRef');
const data: KriteriumCreationModel = dialogRef.value.data;

const initialValue = {
    label: data.label ?? '',
    fachbereiche: data.fachbereiche ?? [],
    isFachlich: data.isFachlich ?? false,
};

const verwaltung = useManagement();

const fachbereiche = ref<ProfundumFachbereich[]>([]);
const kategorienLoading = ref(true);
verwaltung.getFachbereiche().then((result) => {
    fachbereiche.value = result;
    kategorienLoading.value = false;
});

const resolver = (e: FormResolverOptions): Record<string, any> => {
    const errors: Record<string, string[]> = {
        label: [],
        fachbereiche: [],
        isFachlich: [],
    };

    if (e.values['label'].length < 2) {
        errors['label'].push(`Es werden mindestens 2 Zeichen benötigt.`);
    }

    if (e.values['label'].length > 200) {
        errors['label'].push(`Es sind maximal 200 Zeichen erlaubt.`);
    }

    if (e.values['fachbereiche'].length === 0) {
        errors['fachbereiche'].push('Es muss mindestens eine Kategorie angegeben werden!');
    }

    return { values: e.values, errors };
};

const submit = (evt: FormSubmitEvent) => {
    if (!evt.valid) return;
    dialogRef.value.close({
        label: evt.values['label'],
        fachbereiche: evt.values['fachbereiche'],
        isFachlich: evt.values['isFachlich'],
    });
};
</script>

<template>
    <Form
        v-slot="$form"
        :initial-values="initialValue"
        :resolver="resolver"
        class="flex flex-col gap-2 pt-2"
        @submit="submit"
    >
        <FloatLabel variant="on">
            <label for="label">Bezeichnung</label>
            <input-group>
                <input-group-addon v-if="$form.isFachlich?.value ?? false"
                    >Fachliche Kompetenz –
                </input-group-addon>
                <InputText id="label" fluid name="label" />
            </input-group>
        </FloatLabel>
        <Message v-if="$form.label?.invalid" severity="error" size="small" variant="simple">
            {{ $form.label.error }}
        </Message>
        <div class="flex justify-between flex-row-reverse mt-1">
            <ToggleSwitch
                id="isFachlich"
                v-tooltip.left="
                    'Fachliche Kategorien werden auf dem Feedback-Bogen in einer gesonderten Kategorie dargestellt.'
                "
                name="isFachlich"
            />
            <label for="isFachlich">Fachliche Kategorie</label>
        </div>
        <Message
            v-if="$form.isFachlich?.invalid"
            severity="error"
            size="small"
            variant="simple"
        >
            {{ $form.isFachlich.error }}
        </Message>
        <FloatLabel class="mt-3" variant="on">
            <MultiSelect
                id="fachbereiche"
                :loading="kategorienLoading"
                :options="fachbereiche"
                fluid
                name="fachbereiche"
                optionLabel="label"
                optionValue="id"
            />
            <label for="fachbereiche">Findet Anwendung auf:</label>
        </FloatLabel>
        <Message
            v-if="$form.fachbereiche?.invalid"
            severity="error"
            size="small"
            variant="simple"
        >
            {{ $form.fachbereiche.error }}
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
