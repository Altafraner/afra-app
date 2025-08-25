<script setup>
import { Breadcrumb } from 'primevue';
import { RouterLink } from 'vue-router';

const home = {
    icon: 'pi pi-home',
    route: '/',
};

const props = defineProps({
    items: Array,
});
</script>

<template>
    <Breadcrumb :home="home" :model="props.items" class="p-0">
        <template #item="{ item, props }">
            <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
                <a :href="href" v-bind="props.action" @click="navigate">
                    <i v-if="item.icon" :class="[item.icon, 'text-surface-700']" />
                    <span class="text-surface-700">{{ item.label }}</span>
                </a>
            </router-link>
            <a v-else :href="item.url" :target="item.target" v-bind="props.action">
                <i v-if="item.icon" :class="[item.icon, 'text-surface-700']" />
                <span class="text-surface-700">{{ item.label }}</span>
            </a>
        </template>
    </Breadcrumb>
</template>

<style scoped></style>
