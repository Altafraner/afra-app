<script lang="ts" setup>
import { Button, Image, Menubar, useToast } from 'primevue';
import type { MenuItem } from 'primevue/menuitem';
import { computed } from 'vue';

import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { useUser } from '@/stores/user';
import { useRouter } from 'vue-router';
import { isDark } from '@/helpers/isdark';

type GlobalPermissions = 'Otiumsverantwortlich' | 'Profundumsverantwortlich' | 'Admin';
type Role = 'Tutor' | 'Oberstufe' | 'Mittelstufe';

interface Conditions {
    permissions?: GlobalPermissions[] | undefined;
    roles?: Role[] | undefined;
}

interface MenuItemWithCondition extends MenuItem {
    conditions?: Conditions | undefined;
    items?: MenuItemWithCondition[] | undefined;
}

const all_items: MenuItemWithCondition[] = [
    {
        label: 'Übersicht',
        route: '/',
        icon: 'pi pi-home',
    },
    {
        label: 'Otium',
        items: [
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
                conditions: {
                    roles: ['Tutor'],
                },
            },
            {
                label: 'Verwaltung',
                route: {
                    name: 'Verwaltung',
                },
                icon: 'pi pi-wrench',
                conditions: {
                    permissions: ['Otiumsverantwortlich'],
                },
            },
        ],
    },
    {
        label: 'Profundum',
        items: [
            {
                label: 'Profundum',
                route: {
                    name: 'Profundum-Einwahl',
                },
                icon: 'pi pi-check-square',
                conditions: {
                    roles: ['Mittelstufe'],
                },
            },
            {
                label: 'Kriterien',
                route: {
                    name: 'Profundum-Feedback-Kriterien',
                },
                icon: 'pi pi-sliders-h',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
        ],
    },
    {
        label: 'Admin',
        route: {
            name: 'Admin',
        },
        icon: 'pi pi-asterisk',
        conditions: {
            permissions: ['Admin'],
        },
    },
    {
        label: 'Einstellungen',
        route: {
            name: 'Settings',
        },
        icon: 'pi pi-cog',
    },
];

const toast = useToast();
const router = useRouter();
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

function evaluateCondition(item: MenuItemWithCondition): boolean {
    if (item.conditions === undefined) return true;

    if (item.conditions.permissions !== undefined && item.conditions.permissions.length > 0) {
        for (const permission of item.conditions.permissions) {
            if (!user.user.berechtigungen.includes(permission)) return false;
        }
    }

    if (item.conditions.roles !== undefined && item.conditions.roles.length > 0) {
        let success = false;
        for (const role of item.conditions.roles) {
            if (!(user.user.rolle === role)) continue;
            success = true;
            break;
        }
        if (!success) return false;
    }
    return true;
}

function evaluateItems(items: MenuItemWithCondition[]): MenuItem[] {
    const selectedItems: MenuItem[] = [];

    for (const item of items) {
        if (!evaluateCondition(item)) continue;
        let workingCopy = item;
        if (item.items && item.items.length > 0) {
            const children = evaluateItems(item.items);
            workingCopy = Object.assign({}, workingCopy, { items: children });
        }
        if (
            workingCopy.url ||
            workingCopy.target ||
            workingCopy.route ||
            (workingCopy.items && workingCopy.items.length > 0)
        )
            selectedItems.push(workingCopy);
    }
    return selectedItems;
}

const items = computed(() => evaluateItems(all_items));
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
                <span v-if="item.icon" :class="item.icon" />
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
