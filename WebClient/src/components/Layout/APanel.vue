<script lang="ts" setup>
import { shallowRef } from 'vue';

const props = defineProps<{
    label?: string;
    defaultOpen?: boolean;
}>();

const open = shallowRef(props.defaultOpen);
</script>

<template>
    <UCollapsible
        v-model:open="open"
        :ui="{
            root: 'w-full border border-accented rounded-md pb-2',
            content: 'px-3 sm:px-4 py-2',
        }"
    >
        <div class="pt-4 px-3 sm:px-4 pb-2 flex justify-between">
            <span class="font-medium text-highlighted"
                ><slot name="label">{{ label }}</slot></span
            >
            <span>
                <UIcon v-if="open" class="size-6" name="i-lucide-minus" />
                <UIcon v-else class="size-6" name="i-lucide-plus" />
            </span>
        </div>
        <template #content>
            <slot></slot>
        </template>
    </UCollapsible>
</template>

<style scoped></style>
