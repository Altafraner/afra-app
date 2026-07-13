<script setup>
import { useUser } from '@/stores/user';
import OtiumOverview from '@/Otium/components/Management/Overview.vue';
import SchuljahrOverview from '@/Otium/components/Schuljahr/Overview.vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { ref } from 'vue';

const user = useUser();

const navItems = ref([
    {
        label: 'Otium',
        to: {
            name: 'Otium-Katalog',
        },
    },
    {
        label: 'Verwaltung',
        to: {
            name: 'Verwaltung',
        },
    },
]);

const tabItems = [
    {
        label: 'Otium',
        slot: 'otium',
    },
    {
        label: 'Tage + Blöcke',
        slot: 'schuljahr',
    },
];
</script>

<template>
    <template v-if="user.user.rolle !== 'Tutor'">
        <h1>Sie sind nicht Autorisiert, diese Seite zu nutzen.</h1>
    </template>
    <template v-else>
        <NavBreadcrumb :items="navItems" />
        <h1>Otia-Verwaltung</h1>

        <UTabs :items="tabItems">
            <template #otium>
                <OtiumOverview />
            </template>
            <template #schuljahr>
                <SchuljahrOverview />
            </template>
        </UTabs>
    </template>
</template>

<style scoped></style>
