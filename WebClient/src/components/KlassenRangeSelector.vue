<script setup>
import { Select } from 'primevue';

const props = defineProps({
    min: Number,
    max: Number,
    options: Array,
});

const emit = defineEmits(['update:min', 'update:max']);
</script>

<template>
    <span class="flex gap-2 items-center">
        <Select
            :modelValue="min"
            @update:modelValue="(value) => emit('update:min', value)"
            :options="options"
            placeholder="min"
            optionLabel="label"
            optionValue="value"
            fluid
        />
        –
        <Select
            :modelValue="max"
            @update:modelValue="(value) => emit('update:max', value)"
            :options="options"
            placeholder="max"
            optionLabel="label"
            optionValue="value"
            fluid
        />
    </span>
</template>
