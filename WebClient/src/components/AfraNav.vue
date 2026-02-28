<script lang="ts" setup>
import { computed, onMounted } from 'vue';

import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { useUser } from '@/stores/user';
import { useProfundumEinwahl } from '@/Profundum/stores/profundumEinwahlStore';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useRouter } from 'vue-router';
import { isDark } from '@/helpers/isdark';
import type { NavigationMenuItem } from '@nuxt/ui/components/NavigationMenu.d.vue.ts';

type GlobalPermissions =
    | 'Otiumsverantwortlich'
    | 'Profundumsverantwortlich'
    | 'Admin'
    | 'Sekretariat'
    | 'Schulleiter';
type Role = 'Tutor' | 'Oberstufe' | 'Mittelstufe';

interface Conditions {
    permissions?: GlobalPermissions[] | undefined;
    roles?: Role[] | undefined;
    feature?: (() => boolean) | undefined;
}

interface MenuItemWithCondition extends NavigationMenuItem {
    conditions?: Conditions | undefined;
    children?: MenuItemWithCondition[] | undefined;
}

const all_items: MenuItemWithCondition[] = [
    {
        label: 'Übersicht',
        to: '/',
        icon: 'i-lucide-house',
    },
    {
        label: 'Otium',
        children: [
            {
                label: 'Katalog',
                to: {
                    name: 'Otium-Katalog',
                },
                icon: 'i-lucide-list',
            },
            {
                label: 'Verwaltung',
                to: {
                    name: 'Verwaltung',
                },
                icon: 'i-lucide-wrench',
                conditions: {
                    permissions: ['Otiumsverantwortlich'],
                },
            },
        ],
    },
    {
        label: 'Profundum',
        children: [
            {
                label: 'Einwahl',
                to: {
                    name: 'Profundum-Einwahl',
                },
                icon: 'i-lucide-square-check-big',
                conditions: {
                    roles: ['Mittelstufe'],
                    feature: () => profundumEinwahl.isEinwahlActive,
                },
            },
            {
                label: 'Feedback',
                to: {
                    name: 'Profundum-Feedback-Abgeben',
                },
                icon: 'i-lucide-sliders-horizontal',
                conditions: {
                    roles: ['Tutor'],
                },
            },
            {
                label: 'Verwaltung',
                to: { name: 'Profundum-Verwaltung' },
                icon: 'i-lucide-wrench',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
            {
                label: 'Matching',
                to: { name: 'Profundum-Matching' },
                icon: 'i-lucide-grid-2x2-plus',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
            {
                label: 'Feedback Kriterien',
                to: {
                    name: 'Profundum-Feedback-Kriterien',
                },
                icon: 'i-lucide-wrench',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
            {
                label: 'Feedback Überwachung',
                to: {
                    name: 'Profundum-Feedback-Control',
                },
                icon: 'i-lucide-eye',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
            {
                label: 'Feedback Drucken',
                to: {
                    name: 'Profundum-Feedback-Download',
                },
                icon: 'i-lucide-printer',
                conditions: {
                    permissions: ['Profundumsverantwortlich'],
                },
            },
            {
                label: 'Feedback',
                to: {
                    name: 'Profundum-Feedback-Einsicht',
                },
                icon: 'i-lucide-sliders-horizontal',
                conditions: {
                    roles: ['Mittelstufe', 'Oberstufe'],
                },
            },
        ],
    },
    {
        label: 'Aufsicht',
        to: {
            name: 'Aufsicht',
        },
        icon: 'i-lucide-eye',
        conditions: {
            roles: ['Tutor'],
        },
    },
    {
        label: 'Freistellung',
        children: [
            {
                label: 'Neuer Antrag',
                to: { name: 'Freistellung-Neu' },
                icon: 'i-lucide-file-plus',
                conditions: {
                    roles: ['Oberstufe', 'Mittelstufe'],
                },
            },
            {
                label: 'Meine Anträge',
                to: { name: 'Freistellung-Meine' },
                icon: 'i-lucide-list',
                conditions: {
                    roles: ['Oberstufe', 'Mittelstufe'],
                },
            },
            {
                label: 'Anträge bearbeiten',
                to: { name: 'Freistellung-Lehrer' },
                icon: 'i-lucide-inbox',
                conditions: {
                    roles: ['Tutor'],
                },
            },
            {
                label: 'Sekretariat',
                to: { name: 'Freistellung-Sekretariat' },
                icon: 'i-lucide-square-check',
                conditions: {
                    permissions: ['Sekretariat'],
                },
            },
            {
                label: 'Schulleiter',
                to: { name: 'Freistellung-Schulleiter' },
                icon: 'i-lucide-badge-check',
                conditions: {
                    permissions: ['Schulleiter'],
                },
            },
        ],
    },
    {
        label: 'Admin',
        icon: 'i-lucide-asterisk',
        children: [
            {
                label: 'Impersonieren',
                to: {
                    name: 'Admin-Impersonate',
                },
                conditions: {
                    permissions: ['Admin'],
                },
            },
            {
                label: 'Cevex',
                to: {
                    name: 'Admin-Cevex',
                },
                conditions: {
                    permissions: ['Admin'],
                },
            },
        ],
    },
    {
        label: 'Einstellungen',
        to: {
            name: 'Settings',
        },
        icon: 'i-lucide-settings',
    },
];

const toast = useToast();
const router = useRouter();
const user = useUser();
const profundumEinwahl = useProfundumEinwahl();
const freistellung = useFreistellungStore();

onMounted(() => {
    if (user.isMittelstufe) {
        profundumEinwahl.update();
    }
    if (user.user) {
        freistellung.updateOffeneAnzahl();
    }
});

const logout = async () => {
    try {
        await user.logout();
        await router.push('/');
        toast.add({
            color: 'success',
            title: 'Abgemeldet!',
            description: 'Sie wurden erfolgreich abgemeldet.',
            duration: 3000,
        });
    } catch (error) {
        toast.add({
            color: 'error',
            title: 'Fehler!',
            description: 'Sie konnten nicht abgemeldet werden.',
        });
    }
};

function evaluateCondition(item: MenuItemWithCondition): boolean {
    if (!user.user) return false;
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

    if (item.conditions.feature !== undefined && !item.conditions.feature()) return false;

    return true;
}

function evaluateItems(items: MenuItemWithCondition[]): NavigationMenuItem[] {
    const selectedItems: NavigationMenuItem[] = [];

    for (const item of items) {
        if (!evaluateCondition(item)) continue;
        let workingCopy = item;
        if (item.children && item.children.length > 0) {
            const children = evaluateItems(item.children);
            workingCopy = Object.assign({}, workingCopy, { children: children });
        }
        if (workingCopy.to || (workingCopy.children && workingCopy.children.length > 0))
            selectedItems.push(workingCopy);
    }
    return selectedItems;
}

const items = computed(() => {
    const evaluated = evaluateItems(all_items);
    if (freistellung.offeneAnzahl) {
        const freistellungItem = evaluated.find((item) => item.label === 'Freistellung');
        if (freistellungItem) freistellungItem.badge = freistellung.offeneAnzahl;
    }
    return evaluated;
});
const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
</script>

<template>
    <UHeader>
        <template #title>
            <img :src="logo" alt="Verein der Altafraner" class="h-10 w-auto inline-block" />
        </template>
        <UNavigationMenu :items="items" color="neutral" />
        <template #right
            ><UButton
                class="text-muted hover:text-highlighted"
                color="neutral"
                icon="i-lucide-power"
                variant="ghost"
                @click="logout"
                >Logout</UButton
            ></template
        >
        <template #body>
            <UNavigationMenu :items="items" color="neutral" orientation="vertical" />
        </template>
    </UHeader>
</template>

<style scoped></style>
