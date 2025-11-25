<script setup>
import { formatTutor } from '@/helpers/formatters.ts';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { ref } from 'vue';
import { FloatLabel, Select } from 'primevue';

const model = defineModel();

const settings = useOtiumStore();
const personen = ref(null);
const loading = ref(true);

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
        <Select
            id="betreuerSelect"
            v-model="model"
            :loading="loading"
            :options="personen"
            filter
            fluid
            option-label="name"
            option-value="id"
            v-bind="$attrs"
        />
        <label for="betreuerSelect">
            <slot name="label">Betreuer:in</slot>
        </label>
    </FloatLabel>
</template>

<style scoped></style>
