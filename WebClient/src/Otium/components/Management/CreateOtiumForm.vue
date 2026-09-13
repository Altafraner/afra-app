<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import OtiumKategorySelector from '@/Otium/components/Form/OtiumKategorySelector.vue';
import MarkdownEditor from '@/components/MarkdownEditor.vue';
import { FormError, FormSubmitEvent } from '@nuxt/ui';

const emits = defineEmits<{
    close: [{ bezeichnung: string; beschreibung: string; kategorie: string }];
}>();

const settings = useOtiumStore();
const loading = ref(false);

function submit(event: FormSubmitEvent<FormState>) {
    emits('close', {
        bezeichnung: event.data.label!,
        beschreibung: event.data.description!,
        kategorie: event.data.category!.id,
    });
}

async function setup() {
    loading.value = true;
    await settings.updateKategorien();
    loading.value = false;
}

setup();

interface FormState {
    label: string | undefined;
    description: string | undefined;
    category: { id: string } | undefined;
}

const state = reactive<FormState>({
    category: undefined,
    description: undefined,
    label: undefined,
});

function validate(state: Partial<FormState>): FormError[] {
    const errors: FormError[] = [];

    if (!state.label || state.label.length < 1)
        errors.push({ name: 'label', message: 'Es muss eine Bezeichnung gesetzt sein' });
    if (state.label && state.label.length > 70)
        errors.push({
            name: 'label',
            message: 'Die Bezeichnung darf maximal 70 Zeichen lang sein',
        });
    if (!state.description || state.description.length < 1)
        errors.push({ name: 'description', message: 'Es muss eine Beschreibung gesetzt sein' });
    if (state.description && state.description.length > 500)
        errors.push({
            name: 'description',
            message: 'Die Beschreibung darf maximal 500 Zeichen lang sein',
        });
    if (!state.category)
        errors.push({ name: 'category', message: 'Es muss eine Kategorie ausgewählt sein' });

    return errors;
}
</script>

<template>
    <UModal title="Neues Otium erstellen">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Bezeichnung" name="label" required>
                    <UInput
                        v-model="state.label"
                        class="w-full"
                        placeholder="Bezeichnung eingeben"
                    />
                </UFormField>
                <UFormField label="Beschreibung" name="description" required>
                    <MarkdownEditor
                        v-model="state.description"
                        :maxlength="500"
                        placeholder="Beschreibung eingeben"
                    />
                </UFormField>
                <UFormField label="Kategorie" name="category" required>
                    <OtiumKategorySelector
                        v-model="state.category"
                        fullSize
                        hide-clear
                        :options="settings.kategorien as any"
                        color="secondary"
                        placeholder="Kategorie wählen"
                    />
                </UFormField>
                <UButton icon="i-lucide-plus" label="Erstellen" type="submit" />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
