<script setup>
import { chooseColorNuxtUi } from '@/helpers/formatters';

const props = defineProps({
    istAbgesagt: Boolean,
    auslastung: Number,
});

const ui = {
    base: 'flex justify-center',
};
</script>

<template>
    <UBadge v-if="istAbgesagt" :ui="ui" color="error" variant="soft">Abgesagt</UBadge>
    <UBadge
        v-else-if="auslastung != null"
        :color="chooseColorNuxtUi(auslastung)"
        :ui="ui"
        variant="soft"
    >
        {{ auslastung }} %
    </UBadge>
    <UBadge v-else :ui="ui" color="success" variant="soft">&mdash;</UBadge>
</template>

<style scoped></style>
