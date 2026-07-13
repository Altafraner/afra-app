<script lang="ts" setup>
import { h, resolveComponent } from 'vue';

const USkeleton = resolveComponent('USkeleton');

const props = withDefaults(
    defineProps<{
        n?: number;
    }>(),
    { n: 3 },
);

const data = new Array(props.n);

const columns = [
    {
        id: 'a',
        header: () => h(USkeleton, { class: 'w-full h-[1em]' }),
        cell: () => h(USkeleton, { class: 'w-full h-[1em]' }),
    },
    {
        id: 'a',
        header: () => h(USkeleton, { class: 'w-full h-[1em]' }),
        cell: () => h(USkeleton, { class: 'w-full h-[1em]' }),
    },
    {
        id: 'a',
        header: () => h(USkeleton, { class: 'w-full h-[1em]' }),
        cell: () => h(USkeleton, { class: 'w-full h-[1em]' }),
    },
];
</script>

<template>
    <UTable :columns="columns" :data="data" />
</template>

<style scoped></style>
