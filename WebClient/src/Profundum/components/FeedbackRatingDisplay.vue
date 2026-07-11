<script lang="ts" setup>
import type { rating } from '@/Profundum/models/feedback';

const props = defineProps<{
    value: rating;
}>();

const legend: Record<rating, string> = {
    1: 'Nicht ausgeprägt',
    2: 'Wenig ausgeprägt',
    3: 'deutlich ausgeprägt',
    4: 'hervorragend ausgeprägt',
};
</script>

<template>
    <UTooltip :text="legend[value]">
        <span class="flex flex-row gap-1">
            <span
                :class="{
                    'bg-primary': value === 1,
                }"
                class="size-5 rounded-sm border-primary border"
            />
            <span
                :class="{
                    'bg-primary': value === 2,
                }"
                class="size-5 rounded-sm border-primary border"
            />
            <span
                :class="{
                    'bg-primary': value === 3,
                }"
                class="size-5 rounded-sm border-primary border"
            />
            <span
                :class="{
                    'bg-primary': value === 4,
                }"
                class="size-5 rounded-sm border-primary border"
            />
        </span>
    </UTooltip>
</template>

<style scoped></style>
