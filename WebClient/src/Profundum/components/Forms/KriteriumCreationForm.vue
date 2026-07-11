<script lang="ts" setup>
import { reactive, ref } from 'vue';
import type { KriteriumCreationModel } from '@/Profundum/components/Forms/kriteriumCreationModel';
import { useManagement } from '@/Profundum/composables/verwaltung';
import type { ProfundumFachbereich } from '@/Profundum/models/verwaltung';
import { FormError, FormSubmitEvent } from '@nuxt/ui';

const props = defineProps<KriteriumCreationModel>();

const emit = defineEmits<{
    close: [{ label: string; fachbereiche: string[]; isFachlich: boolean }];
}>();

const verwaltung = useManagement();

const fachbereiche = ref<ProfundumFachbereich[]>([]);
const kategorienLoading = ref(true);
verwaltung.getFachbereiche().then((result) => {
    if (!result) return;
    fachbereiche.value = result;
    kategorienLoading.value = false;
});

const submit = (event: FormSubmitEvent<FormSchema>) => {
    emit('close', {
        label: event.data.label!,
        fachbereiche: event.data.fachbereiche!,
        isFachlich: event.data.isFachlich!,
    });
};

const isFachlichOptions = [
    {
        label: 'Allgemein',
        description: 'Allgemeine Kategorie',
        value: false,
    },
    {
        label: 'Fachlich',
        description: 'Kategorie mit speziellem fachlichen Bezug',
        value: true,
    },
];

interface FormSchema {
    isFachlich?: boolean;
    fachbereiche?: string[];
    label?: string;
}

const state = reactive<FormSchema>({
    isFachlich: props.isFachlich,
    fachbereiche: props.fachbereiche,
    label: props.label,
});

function validate(state: Partial<FormSchema>): FormError[] {
    const errors = [] as FormError[];

    if (state.label == undefined || state.label.trim().length === 0) {
        errors.push({ name: 'label', message: 'Bitte geben Sie eine Bezeichnung an.' });
    }

    if ((state.label?.length ?? 0) > 200) {
        errors.push({ name: 'label', message: 'Bitte geben Sie max. 200 Zeichen ein.' });
    }

    if (!state.fachbereiche || state.fachbereiche.length === 0) {
        errors.push({
            name: 'fachbereiche',
            message: 'Es muss mindestens ein Bereich angegeben werden.',
        });
    }

    if (state.isFachlich === undefined) {
        errors.push({ name: 'isFachlich', message: 'Bitte treffen Sie eine Auswahl.' });
    }

    return errors;
}
</script>

<template>
    <UModal :title="variant == 'create' ? 'Kategorie hinzufügen' : 'Kategorie bearbeiten'">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Bezeichnung" name="label" required>
                    <UFieldGroup class="w-full">
                        <UBadge
                            v-if="state.isFachlich"
                            color="neutral"
                            label="Fachliche Kompetenz –"
                            variant="subtle"
                        />
                        <UInput
                            v-model="state.label"
                            class="w-full"
                            placeholder="Bezeichnung eingeben"
                        />
                    </UFieldGroup>
                </UFormField>
                <UFormField
                    help="Nur für Profunda der gewählten Bereiche wird diese Kategorie angezeigt."
                    label="Anwendungsbereich"
                    name="fachbereiche"
                    required
                >
                    <USelect
                        v-model="state.fachbereiche"
                        :items="fachbereiche"
                        :loading="kategorienLoading"
                        :ui="{
                            value: 'text-ellipsis',
                        }"
                        class="w-full"
                        label-key="label"
                        multiple
                        placeholder="Profundumsbereiche auswählen"
                        value-key="id"
                    />
                </UFormField>
                <UFormField
                    help="Fachliche Kategorien werden in der Auswertung gesondert dargestellt."
                    label="Art"
                    name="isFachlich"
                    required
                >
                    <URadioGroup
                        v-model="state.isFachlich"
                        :items="isFachlichOptions"
                        :ui="{
                            item: 'flex-1 p-2.5',
                        }"
                        class="w-full"
                        orientation="horizontal"
                        variant="table"
                    />
                </UFormField>
                <UButton
                    :icon="variant == 'create' ? 'i-lucide-plus' : 'i-lucide-check'"
                    :label="variant == 'create' ? 'Erstellen' : 'Ändern'"
                    type="submit"
                />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
