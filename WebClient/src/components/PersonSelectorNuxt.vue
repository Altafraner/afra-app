<script lang="ts" setup>
import { formatTutor } from '@/helpers/formatters';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { computed, ref } from 'vue';
import { UserInfoMinimal } from '@/models/user/user';

const model = defineModel<string | string[] | undefined>();

const settings = useOtiumStore();
const loading = ref(true);

const props = withDefaults(
    defineProps<{
        hideRolle?: boolean;
        filter?: (student: UserInfoMinimal) => boolean;
    }>(),
    {
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
    <USelectMenu
        v-model="model as any"
        :items="personenMapped"
        :loading="loading"
        value-key="id"
    />
</template>
