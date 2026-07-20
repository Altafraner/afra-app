<script setup>
import { formatTutor } from '@/helpers/formatters';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { computed, ref } from 'vue';

const model = defineModel();

const settings = useOtiumStore();
const loading = ref(true);

const props = defineProps({
    multi: Boolean,
    hideRolle: Boolean,
    filter: {
        type: Function,
        default: () => true,
        required: false,
    },
});

async function getPersonen() {
    await settings.updatePersonen();
    loading.value = false;
}

getPersonen();

const personenMapper = (person) => {
    return {
        id: person.id,
        label: props.hideRolle
            ? formatTutor(person)
            : `${formatTutor(person)} (${person.rolle})`,
    };
};

const personenMapped = computed(() => {
    return settings.personen?.filter(props.filter).map(personenMapper) ?? [];
});
</script>

<template>
    <USelectMenu v-model="model" :items="personenMapped" :loading="loading" value-key="id" />
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
