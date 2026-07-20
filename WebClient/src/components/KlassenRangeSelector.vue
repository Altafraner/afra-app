<script lang="ts" setup>
import type { SelectItem } from '@nuxt/ui/components/Select.d.vue.ts';

defineProps({
    options: Array<SelectItem>,
});

const min = defineModel<string | undefined>('min');
const max = defineModel<string | undefined>('max');
</script>

<template>
    <UFieldGroup class="flex">
        <USelect
            v-model="min"
            :items="options"
            class="flex-1"
            placeholder="Niedrigste Klassenstufe wählen"
        />
        <UBadge color="neutral" label="—" variant="outline" />
        <USelect
            v-model="max"
            :items="options"
            class="flex-1"
            placeholder="Höchste Klassenstufe wählen"
        />
    </UFieldGroup>
</template>
