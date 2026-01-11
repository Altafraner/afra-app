<script setup>
import { formatTutor } from '@/helpers/formatters';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { ref } from 'vue';
import { FloatLabel, Select, MultiSelect } from 'primevue';

const model = defineModel();

const settings = useOtiumStore();
const personen = ref(null);
const loading = ref(true);

const props = defineProps({
    multi: Boolean,
    id: {
        type: String,
        default: 'betreuerSelect',
        required: false,
    },
});

async function getPersonen() {
    const personenMapper = (person) => {
        return {
            id: person.id,
            name: `${formatTutor(person)} (${person.rolle})`,
        };
    };

    await settings.updatePersonen();
    personen.value = settings.personen.map(personenMapper);
    loading.value = false;
}

getPersonen();
</script>

<template>
    <FloatLabel class="w-full" variant="on">
        <template v-if="!props.multi">
            <Select
                :id="id"
                v-model="model"
                :loading="loading"
                :options="personen"
                filter
                fluid
                option-label="name"
                option-value="id"
                v-bind="$attrs"
            />
        </template>
        <template v-else>
            <MultiSelect
                :id="id"
                v-model="model"
                :loading="loading"
                :options="personen"
                display="chip"
                option-label="name"
                option-value="id"
                v-bind="$attrs"
                filter
            />
        </template>
        <label :for="id">
            <slot name="label">Betreuer:in</slot>
        </label>
    </FloatLabel>
</template>

<style scoped>
.multiselect-wrap :deep(.p-multiselect-label-container) {
    height: auto;
}

.multiselect-wrap :deep(.p-multiselect-label) {
    display: flex;
    flex-wrap: wrap;
    white-space: normal;
    gap: 0.25rem;
    padding-top: 0.25rem;
    padding-bottom: 0.25rem;
}

.multiselect-wrap :deep(.p-multiselect-token) {
    margin-bottom: 0.25rem;
}
</style>
