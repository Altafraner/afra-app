<script setup>
import { useUser } from '@/stores/user';
import OtiumOverview from '@/Otium/components/Management/Overview.vue';
import SchuljahrOverview from '@/Otium/components/Schuljahr/Overview.vue';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { ref } from 'vue';

const user = useUser();

const navItems = ref([
    {
        label: 'Verwaltung',
        route: {
            name: 'Verwaltung',
        },
    },
]);
</script>

<template>
    <template v-if="user.user.rolle !== 'Tutor'">
        <h1>Sie sind nicht Autorisiert, diese Seite zu nutzen.</h1>
    </template>
    <template v-else>
        <NavBreadcrumb :items="navItems" />
        <h1>Otia-Verwaltung</h1>

        <Tabs lazy value="0">
            <TabList>
                <Tab value="0">Otium</Tab>
                <Tab value="1">Schultage</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <OtiumOverview />
                </TabPanel>
                <TabPanel value="1">
                    <SchuljahrOverview />
                </TabPanel>
            </TabPanels>
        </Tabs>
    </template>
</template>

<style scoped></style>
