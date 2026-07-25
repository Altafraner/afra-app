<script setup>
import { computed } from 'vue';

const props = defineProps({
    enrollment: { type: Object, default: null },
    wuensche: { type: Array, default: () => [] },
    options: { type: Array, required: true },
    editing: { type: Boolean, default: false },
});

const wishForOption = (option) =>
    props.wuensche.find((w) => w.id === option.profundumId) ?? null;

const selectedInstanz = computed(
    () => props.options.find((o) => o.id === props.enrollment?.profundumInstanzId) ?? null,
);

const bezeichnung = computed(() => selectedInstanz.value?.profundumInfo.bezeichnung ?? '—');

const wishForSelected = computed(() =>
    selectedInstanz.value ? wishForOption(selectedInstanz.value) : null,
);

const sortedOptions = computed(() => {
    const selectedId = props.enrollment?.profundumInstanzId;

    return props.options.toSorted((a, b) => {
        const wishA = wishForOption(a);
        const wishB = wishForOption(b);

        const score = (opt, wish) => {
            if (opt.id === selectedId) return 0;
            if (wish) return 10 + wish.rang;
            return 100;
        };

        return score(a, wishA) - score(b, wishB);
    });
});
</script>

<template>
    <span class="flex gap-1 items-center">
        <template v-if="editing">
            <UCheckbox v-model="enrollment.isFixed" size="xs" />

            <USelectMenu
                v-model="enrollment.profundumInstanzId"
                :items="sortedOptions"
                label-key="profundumInfo.bezeichnung"
                value-key="id"
                clear
                class="w-60"
                :disabled="!enrollment.isFixed"
            >
                <template #item="{ item }">
                    <span class="option-row gap-2">
                        <span v-if="wishForOption(item)">
                            ★ {{ wishForOption(item).rang }}
                        </span>
                        <span>{{ item.profundumInfo.bezeichnung }}</span>
                        <span
                            >({{ item.numEinschreibungen }} /
                            {{ item.maxEinschreibungen }})</span
                        >
                    </span>
                </template>
            </USelectMenu>
        </template>
        <template v-else>
            <span class="readonly-value w-60 min-w-0 flex items-center gap-2">
                <span v-if="wishForSelected" class="wish-indicator text-success shrink-0">
                    <UIcon name="i-lucide-crown" />
                    {{ wishForSelected.rang }}
                </span>

                <UTooltip
                    :text="bezeichnung"
                    :ui="{ content: 'max-w-80 h-auto whitespace-normal' }"
                >
                    <span
                        class="flex items-center gap-1 min-w-0"
                        :class="{ 'text-warning font-semibold': enrollment?.isFixed }"
                    >
                        <UIcon
                            v-if="enrollment?.isFixed"
                            name="i-lucide-lock"
                            class="shrink-0"
                        />
                        <span class="truncate">{{ bezeichnung }}</span>
                    </span>
                </UTooltip>
            </span>
        </template>
    </span>
</template>

<style scoped>
.option-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.option-row :last-child {
    font-style: italic;
}

.readonly-value {
    display: inline-flex;
}

.wish-indicator {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    font-weight: 700;
    white-space: nowrap;
}
</style>
