<script lang="ts" setup>
import { onMounted, onUnmounted, shallowRef } from 'vue';

const props = defineProps({
    breakpoint: {
        type: String,
        default: '768px',
    },
});

let mediaQuery: MediaQueryList | null = null;
const isMobile = shallowRef<boolean>(false);

const update = (event: MediaQueryListEvent) => {
    isMobile.value = event.matches;
};

onMounted(() => {
    mediaQuery = window.matchMedia(`(max-width: ${props.breakpoint})`);
    isMobile.value = mediaQuery.matches;
    mediaQuery.addEventListener('change', update);
});

onUnmounted(() => {
    if (mediaQuery) {
        mediaQuery.removeEventListener('change', update);
    }
});
</script>

<template>
    <slot v-if="isMobile" name="small"></slot>
    <slot v-else name="large"></slot>
</template>

<style scoped></style>
