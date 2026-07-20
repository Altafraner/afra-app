<script lang="ts" setup>
import type { BreadcrumbItem } from '@nuxt/ui/components/Breadcrumb.d.vue.ts';

const home = {
    icon: 'i-lucide-home',
    ariaLabel: 'Startseite',
    to: '/',
} satisfies BreadcrumbItem;

const props = defineProps<{
    items: BreadcrumbItem[];
}>();
</script>

<template>
    <UBreadcrumb :home="home" :items="[home, ...props.items]" class="p-0" color="neutral" />
</template>

<style scoped></style>
