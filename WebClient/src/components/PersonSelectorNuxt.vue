<script lang="ts" setup>
import { formatTutor } from '@/helpers/formatters';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { computed, ref } from 'vue';
import { UserInfoMinimal } from '@/models/user/user';

const model = defineModel<string | undefined>();

const settings = useOtiumStore();
const loading = ref(true);

const props = withDefaults(
    defineProps<{
        multi?: boolean;
        hideRolle?: boolean;
        filter?: (student: UserInfoMinimal) => boolean;
    }>(),
    {
        multi: false,
        hideRolle: false,
        filter: () => true,
    },
);

async function getPersonen() {
    await settings.updatePersonen();
    loading.value = false;
}

getPersonen();

const personenMapper = (person: UserInfoMinimal) => {
    return {
        id: person.id,
        label: props.hideRolle
            ? formatTutor(person)
            : `${formatTutor(person)} (${person.rolle})`,
    };
};

const personenMapped = computed(() => {
    return (
        (settings.personen as UserInfoMinimal[] | null)
            ?.filter(props.filter)
            .map(personenMapper) ?? []
    );
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
