<script lang="ts" setup>
import { formatTutor } from '@/helpers/formatters';
import { computed, ref } from 'vue';
import { UserInfoMinimal } from '@/models/user/user';
import { usePeople } from '@/stores/people.ts';

const model = defineModel<string | string[] | undefined>();

const people = usePeople();
const loading = ref(true);

const props = withDefaults(
    defineProps<{
        hideRolle?: boolean;
        filter?: (student: UserInfoMinimal) => boolean;
        multiple?: boolean;
    }>(),
    {
        hideRolle: false,
        filter: () => true,
        multiple: false,
    },
);

async function getPersonen() {
    await people.updatePersonen();
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
        (people.personen as UserInfoMinimal[] | null)
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
        :multiple="multiple"
        value-key="id"
    />
</template>
