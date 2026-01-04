<script setup>
import Menubar from 'primevue/menubar';
import { computed, ref } from 'vue';
import { Button, Image, useToast } from 'primevue';

import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { useUser } from '@/stores/user';
import { useRouter } from 'vue-router';
import { isDark } from '@/helpers/isdark.js';

const groups = [
    (_) => ({
        label: 'Allgemein',
        icon: 'pi pi-home',
        items: [
            {
                label: 'Übersicht',
                route: '/',
                icon: 'pi pi-user',
            },
        ],
    }),
    (_) => ({
        label: 'Otium',
        icon: 'pi pi-list',
        items: [
            {
                label: 'Otium Katalog',
                route: { name: 'Katalog' },
                icon: 'pi pi-list',
            },
        ],
    }),
    (u) =>
        user.isTeacher
            ? {
                  label: 'Lehrkräfte',
                  icon: 'pi pi-briefcase',
                  items: [
                      {
                          label: 'Otium Aufsicht',
                          route: { name: 'Aufsicht' },
                          icon: 'pi pi-eye',
                      },
                  ],
              }
            : null,
    (u) =>
        user.isStudent && user.isMittelstufe
            ? {
                  label: 'Schüler',
                  icon: 'pi pi-graduation-cap',
                  items: [
                      {
                          label: 'Profundum Einwahl',
                          route: { name: 'Profundum-Einwahl' },
                          icon: 'pi pi-check-square',
                      },
                  ],
              }
            : null,

    (u) => ({
        label: 'Verwaltung',
        icon: 'pi pi-wrench',
        items: [
            ...(user.isOtiumsverantwortlich
                ? [
                      {
                          label: 'Otia Verwaltung',
                          route: { name: 'Verwaltung' },
                          icon: 'pi pi-wrench',
                      },
                  ]
                : []),
            ...(user.isProfundumsverantwortlich
                ? [
                      {
                          label: 'Profunda Verwaltung',
                          route: { name: 'Profundum-Verwaltung' },
                          icon: 'pi pi-wrench',
                      },
                      {
                          label: 'Profunda Matching',
                          route: { name: 'Profundum-Matching' },
                          icon: 'pi pi-map',
                      },
                  ]
                : []),

            ...(user.isAdmin
                ? [
                      {
                          label: 'Admin',
                          route: { name: 'Admin' },
                          icon: 'pi pi-asterisk',
                      },
                  ]
                : []),
        ],
    }),

    (_) => ({
        label: 'System',
        icon: 'pi pi-cog',
        items: [
            {
                label: 'Einstellungen',
                route: { name: 'Settings' },
                icon: 'pi pi-cog',
            },
        ],
    }),
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

function collapseSingleItemGroups(menu) {
    return menu.flatMap((group) => {
        if (group.items && group.items.length === 1) {
            return group.items[0];
        }
        return group;
    });
}

async function setup(update = true) {
    if (update) await user.update();
    if (user.loading) return;

    items.value = collapseSingleItemGroups(
        groups.map((g) => g(user)).filter((x) => x && x.items.length > 0),
    );
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
