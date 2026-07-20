<script setup>
const props = defineProps({
    value: Object,
    minimal: Boolean,
    hideName: Boolean,
    hideIcon: Boolean,
});
</script>

<template>
    <UBadge
        v-if="!minimal"
        :ui="{
            base: 'inline-flex justify-center',
        }"
        color="secondary"
        variant="soft"
    >
        <span
            v-if="value.icon && !hideIcon"
            :class="`ot-angebot-icon ${value.cssColor ? 'ot-angebot-white' : ''}`"
            :style="`background-color: ${value.cssColor ?? 'unset'}`"
        >
            <UIcon :name="value.icon" class="size-3" />
        </span>
        <span v-if="!hideName">
            {{ value.bezeichnung }}
        </span>
    </UBadge>
    <span v-else class="inline-flex items-baseline justify-center gap-1">
        <UIcon
            v-if="value.icon && !hideIcon"
            :name="value.icon"
            :style="`color: ${value.cssColor ?? 'inherit'}`"
            class="size-4"
        />
        <span v-if="!hideName">
            {{ value.bezeichnung }}
        </span>
    </span>
</template>

<style scoped>
.ot-angebot-icon {
    font-size: 0.8em;
    width: 2em;
    height: 2em;
    border-radius: 1em;
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
}

.ot-angebot-icon i {
    font-size: 1.1em;
}

.ot-angebot-white {
    color: #fff;
}
</style>
