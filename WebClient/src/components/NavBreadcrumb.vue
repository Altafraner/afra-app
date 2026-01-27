<script lang="ts" setup>
import { Breadcrumb } from 'primevue';
import { RouterLink } from 'vue-router';
import type { NavBreadcrumbElement } from '@/models/navBreadcrumb';

const home: NavBreadcrumbElement = {
    icon: 'pi pi-home',
    ariaLabel: 'Startseite',
    route: '/',
};

const props = defineProps<{
    items: NavBreadcrumbElement[];
}>();
</script>

<template>
    <Breadcrumb :home="home" :model="props.items" class="p-0">
        <template #item="{ item, props }">
            <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
                <a
                    :aria-label="item.label ?? item.ariaLabel"
                    :href="href"
                    v-bind="props.action"
                    @click="navigate"
                >
                    <i v-if="item.icon" :class="[item.icon, 'text-color']" />
                    <span v-if="item.label" class="text-color">{{ item.label }}</span>
                </a>
            </router-link>
            <a
                v-else
                :aria-label="item.label ?? item.ariaLabel"
                :href="item.url"
                :target="item.target"
                v-bind="props.action"
            >
                <i v-if="item.icon" :class="[item.icon, 'text-color']" />
                <span v-if="item.label" class="text-color">{{ item.label }}</span>
            </a>
        </template>
    </Breadcrumb>
</template>

<style scoped></style>
