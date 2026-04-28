<script lang="ts" setup>
import type { rating } from '@/Profundum/models/feedback';

const props = defineProps<{
    value: rating;
}>();
</script>

<template>
    <span class="flex flex-row gap-1">
        <span
            :class="{
                'bg-primary-500 dark:bg-primary-400': value === 1,
            }"
            class="size-5 rounded-sm border-primary-500 dark:border-primary-400 border"
        />
        <span
            :class="{
                'bg-primary-500 dark:bg-primary-400': value === 2,
            }"
            class="size-5 rounded-sm border-primary-500 dark:border-primary-400 border"
        />
        <span
            :class="{
                'bg-primary-500 dark:bg-primary-400': value === 3,
            }"
            class="size-5 rounded-sm border-primary-500 dark:border-primary-400 border"
        />
        <span
            :class="{
                'bg-primary-500 dark:bg-primary-400': value === 4,
            }"
            class="size-5 rounded-sm border-primary-500 dark:border-primary-400 border"
        />
    </span>
</template>

<style scoped></style>
