<script setup>
import FloatLabel from 'primevue/floatlabel';
import { Select } from 'primevue';

const props = defineProps({
    options: {
        type: Array,
        required: true,
    },
    label: String,
    disabled: Boolean,
    preSelected: {
        type: Object,
        required: false,
    },
});

const model = defineModel({
    default: null,
});
</script>

<template>
    <div class="w-full">
        <FloatLabel variant="on">
            <Select
                v-if="!disabled"
                id="erstwunsch"
                v-model="model"
                :options="props.options"
                fluid
                name="erstwunsch"
                option-label="label"
                option-value="value"
                show-clear
            />
            <template v-else>
                <Select
                    id="erstwunsch"
                    v-model="model"
                    :options="[preSelected]"
                    disabled
                    fluid
                    name="erstwunsch"
                    option-label="label"
                    option-value="value"
                    show-clear
                />
            </template>
            <label for="erstwunsch">{{ label }}</label>
        </FloatLabel>
    </div>
</template>

<style scoped></style>
