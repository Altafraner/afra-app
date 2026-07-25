<script setup>
import { reactive } from 'vue';
import MarkdownEditor from '@/components/MarkdownEditor.vue';

const props = defineProps({
    categories: { type: Array, default: () => [] },
    fachbereiche: { type: Array, default: () => [] },
});

const emit = defineEmits(['close']);

const state = reactive({
    bezeichnung: '',
    beschreibung: '',
    kategorieId: null,
    minKlasse: null,
    maxKlasse: null,
    fachbereichIds: [],
});

function validate(state) {
    const errors = [];
    if (!state.bezeichnung || state.bezeichnung.trim().length === 0) {
        errors.push({ name: 'bezeichnung', message: 'Bitte geben Sie eine Bezeichnung an.' });
    }
    if (!state.kategorieId) {
        errors.push({ name: 'kategorieId', message: 'Bitte wählen Sie eine Kategorie.' });
    }
    return errors;
}

function submit(event) {
    emit('close', {
        ...event.data,
        bezeichnung: event.data.bezeichnung.trim(),
        beschreibung: event.data.beschreibung?.trim(),
    });
}
</script>

<template>
    <UModal title="Neues Profundum anlegen">
        <template #body>
            <UForm
                :state="state"
                :validate="validate"
                class="flex flex-col gap-4"
                @submit="submit"
            >
                <UFormField label="Bezeichnung" name="bezeichnung" required>
                    <UInput v-model="state.bezeichnung" maxlength="80" class="w-full" />
                </UFormField>

                <UFormField label="Kategorie" name="kategorieId" required>
                    <USelect
                        v-model="state.kategorieId"
                        :items="props.categories"
                        label-key="bezeichnung"
                        value-key="id"
                        placeholder="Kategorie auswählen"
                        class="w-full"
                    />
                </UFormField>

                <UFormField label="Fachbereiche" name="fachbereichIds">
                    <USelect
                        v-model="state.fachbereichIds"
                        :items="props.fachbereiche"
                        label-key="label"
                        value-key="id"
                        multiple
                        placeholder="Fachbereiche wählen"
                        class="w-full"
                    />
                </UFormField>

                <UFormField label="Beschreibung" name="beschreibung">
                    <MarkdownEditor v-model="state.beschreibung" :maxlength="2000" />
                </UFormField>

                <UFormField label="Jahrgänge" name="jahrgaenge">
                    <div class="flex gap-2 items-center">
                        <UInputNumber
                            v-model="state.minKlasse"
                            placeholder="min"
                            class="w-20"
                        />
                        –
                        <UInputNumber
                            v-model="state.maxKlasse"
                            placeholder="max"
                            class="w-20"
                        />
                    </div>
                </UFormField>

                <UButton icon="i-lucide-plus" label="Anlegen" type="submit" />
            </UForm>
        </template>
    </UModal>
</template>

<style scoped></style>
