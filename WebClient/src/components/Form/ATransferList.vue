<script lang="ts" setup>
import type { ListboxItem } from '@nuxt/ui';
import { computed, ref } from 'vue';

const props = defineProps<{
    items: ListboxItem[];
    filter?: boolean;
}>();

const targetItems = defineModel<ListboxItem[]>({ default: () => [] });
const sourceSelection = ref<ListboxItem[]>([]);
const targetSelection = ref<ListboxItem[]>([]);

const sourceItems = computed(() =>
    props.items.filter((item) => !targetItems.value.some((t) => t.value === item.value)),
);

function transferSelected() {
    targetItems.value = targetItems.value.concat(sourceSelection.value as ListboxItem[]);
    sourceSelection.value = [];
}

function removeSelected() {
    targetItems.value = targetItems.value.filter(
        (item) => !targetSelection.value.some((t) => t.value === item.value),
    );
    targetSelection.value = [];
}

function selectAll() {
    targetItems.value = [...targetItems.value, ...sourceItems.value];
}

function deselectAll() {
    targetItems.value = [];
}
</script>

<template>
    <div class="flex items-stretch gap-4 w-full">
        <div class="flex flex-col flex-1 gap-1">
            <span class="text-sm font-medium text-highlighted">Verfügbar</span>

            <UListbox
                v-model="sourceSelection"
                :filter="props.filter"
                :items="sourceItems"
                class="size-full"
                multiple
            />
        </div>

        <div class="flex flex-col items-center justify-center gap-1">
            <UButton
                :disabled="!sourceItems.length"
                color="neutral"
                icon="i-lucide-chevrons-right"
                variant="outline"
                @click="selectAll"
            />
            <UButton
                :disabled="!sourceSelection.length"
                color="neutral"
                icon="i-lucide-chevron-right"
                variant="outline"
                @click="transferSelected"
            />
            <UButton
                :disabled="!targetSelection.length"
                color="neutral"
                icon="i-lucide-chevron-left"
                variant="outline"
                @click="removeSelected"
            />
            <UButton
                :disabled="!targetItems.length"
                color="neutral"
                icon="i-lucide-chevrons-left"
                variant="outline"
                @click="deselectAll"
            />
        </div>

        <div class="flex flex-col flex-1 gap-1">
            <span class="text-sm font-medium text-highlighted">Ausgewählt</span>
            <UListbox
                v-model="targetSelection"
                :filter="props.filter"
                :items="targetItems"
                class="size-full"
                multiple
            />
        </div>
    </div>
</template>
