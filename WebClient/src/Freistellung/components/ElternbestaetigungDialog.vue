<script lang="ts" setup>
import { computed, reactive } from 'vue';
import type { FormError, FormSubmitEvent } from '@nuxt/ui';

const props = defineProps<{
    grund: string;
    /** Whether the "ist erforderlich?" question should be asked, or is already settled as true. */
    asksErforderlich: boolean;
}>();

const emit = defineEmits<{
    close: [{ erforderlich: boolean; vorhanden: boolean; hinweis: string | null } | undefined];
}>();

interface FormSchema {
    erforderlich: boolean;
    vorhanden: boolean;
    hinweis: string;
}

const state = reactive<FormSchema>({ erforderlich: true, vorhanden: true, hinweis: '' });

const istErforderlich = computed(() => (props.asksErforderlich ? state.erforderlich : true));

const jaNein = [
    { label: 'Ja', value: true },
    { label: 'Nein', value: false },
];

function validate(formState: Partial<FormSchema>): FormError[] {
    const erforderlich = props.asksErforderlich ? formState.erforderlich : true;
    if (erforderlich && formState.vorhanden === false && !formState.hinweis?.trim())
        return [{ name: 'hinweis', message: 'Bitte einen Hinweis angeben, was noch fehlt.' }];
    return [];
}

function submit(event: FormSubmitEvent<FormSchema>) {
    const erforderlich = props.asksErforderlich ? event.data.erforderlich : true;
    emit('close', {
        erforderlich,
        vorhanden: erforderlich ? event.data.vorhanden : false,
        hinweis: erforderlich && !event.data.vorhanden ? event.data.hinweis.trim() : null,
    });
}

const konsequenz = computed(() => {
    if (istErforderlich.value && !state.vorhanden)
        return 'Der Antrag wird mit deinem Hinweis an den Schüler / die Schülerin zurückgesendet.';
    return 'Der Antrag wird an die Schulleitung zur abschließenden Entscheidung weitergeleitet.';
});
</script>

<template>
    <UModal title="Elternbestätigung" :description="`Antrag „${grund}“`">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField
                    v-if="asksErforderlich"
                    label="Ist eine Elternbestätigung erforderlich?"
                    name="erforderlich"
                >
                    <URadioGroup
                        v-model="state.erforderlich"
                        :items="jaNein"
                        orientation="horizontal"
                    />
                </UFormField>

                <p v-else class="text-sm text-muted">
                    Für diesen Antrag wurde bereits festgestellt, dass eine Elternbestätigung
                    erforderlich ist.
                </p>

                <UFormField
                    v-if="istErforderlich"
                    label="Liegt die Elternbestätigung vor?"
                    name="vorhanden"
                >
                    <URadioGroup
                        v-model="state.vorhanden"
                        :items="jaNein"
                        orientation="horizontal"
                    />
                </UFormField>

                <UFormField
                    v-if="istErforderlich && !state.vorhanden"
                    label="Hinweis an den Schüler / die Schülerin"
                    name="hinweis"
                    required
                >
                    <UTextarea
                        v-model="state.hinweis"
                        :rows="3"
                        :maxlength="500"
                        class="w-full"
                        placeholder="Was fehlt noch?"
                    />
                </UFormField>

                <UAlert color="neutral" variant="soft" :description="konsequenz" />

                <UButton label="Speichern" type="submit" />
            </UForm>
        </template>
        <template #footer>
            <UButton
                color="neutral"
                label="Abbrechen"
                variant="subtle"
                @click="$emit('close', undefined)"
            />
        </template>
    </UModal>
</template>
