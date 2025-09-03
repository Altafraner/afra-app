<script setup>
import Menubar from 'primevue/menubar';
import { computed, ref } from 'vue';
import { Button, Image, useToast } from 'primevue';

import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { useUser } from '@/stores/user.js';
import { useRouter } from 'vue-router';
import { isDark } from '@/helpers/isdark.js';

const items_teacher = [
    {
        label: 'Übersicht',
        route: '/',
        icon: 'pi pi-user',
    },
    {
        label: 'Katalog',
        route: {
            name: 'Katalog',
        },
        icon: 'pi pi-list',
    },
    {
        label: 'Aufsicht',
        route: {
            name: 'Aufsicht',
        },
        icon: 'pi pi-eye',
    },
    {
        label: 'Einstellungen',
        route: {
            name: 'Settings',
        },
        icon: 'pi pi-cog',
    },
];

const items_student = [
    {
        label: 'Übersicht',
        route: '/',
        icon: 'pi pi-user',
    },
    {
        label: 'Katalog',
        route: {
            name: 'Katalog',
        },
        icon: 'pi pi-list',
    },
    {
        label: 'Einstellungen',
        route: {
            name: 'Settings',
        },
        icon: 'pi pi-cog',
    },
];

const items_mittelstufe = [
    {
        label: 'Profundum',
        route: {
            name: 'Profundum-Einwahl',
        },
        icon: 'pi pi-check-square',
    },
];

const items_otium_manager = [
    {
        label: 'Verwaltung',
        route: {
            name: 'Verwaltung',
        },
        icon: 'pi pi-wrench',
    },
];

const items_admin = [
    {
        label: 'Admin',
        route: {
            name: 'Admin',
        },
        icon: 'pi pi-asterisk',
    },
];

const toast = useToast();
const router = useRouter();
const items = ref([]);
const user = useUser();

const logout = async () => {
    const user = useUser();
    try {
        await user.logout();
        await router.push('/');
        toast.add({
            severity: 'success',
            summary: 'Abgemeldet!',
            detail: 'Sie wurden erfolgreich abgemeldet.',
            life: 3000,
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler!',
            detail: 'Sie konnten nicht abgemeldet werden.',
        });
    }
};

async function setup(update = true) {
    if (update) await user.update();
    if (user.loading) return;
    if (user.isStudent) {
        items.value = items_student;
        if (user.isMittelstufe) {
            items.value = [...items.value, ...items_mittelstufe];
        }
    } else if (user.isTeacher) {
        items.value = items_teacher;
    } else {
        items.value = [];
    }

    if (user.isOtiumsverantwortlich) {
        items.value = [...items.value, ...items_otium_manager];
    }
    if (user.isAdmin) {
        items.value = [...items.value, ...items_admin];
    }
}

setup();

user.$subscribe(() => {
    setup(false);
});

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
</script>

<template>
    <Menubar :model="items">
        <template #start>
            <Image :src="logo" alt="Verein der Altafraner" height="50"></Image>
        </template>
        <template #item="{ item, props, hasSubmenu }">
            <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
                <a :href="href" v-bind="props.action" @click="navigate">
                    <span v-if="item.icon" :class="item.icon" />
                    <span>{{ item.label }}</span>
                </a>
            </router-link>
            <a v-else :href="item.url" :target="item.target" v-bind="props.action">
                <span :class="item.icon" />
                <span>{{ item.label }}</span>
                <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down" />
            </a>
        </template>
        <template #end>
            <Button
                label="Logout"
                icon="pi pi-power-off"
                @click="logout"
                variant="text"
                severity="secondary"
            />
        </template>
    </Menubar>
</template>

<style scoped></style>
