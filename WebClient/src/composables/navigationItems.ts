import { computed } from 'vue';
import type { NavigationMenuItem } from '@nuxt/ui/components/NavigationMenu.d.vue.ts';
import { useUser } from '@/stores/user';
import { useProfundumEinwahl } from '@/Profundum/stores/profundumEinwahlStore';

export type GlobalPermissions = 'Otiumsverantwortlich' | 'Profundumsverantwortlich' | 'Admin';
export type Role = 'Tutor' | 'Oberstufe' | 'Mittelstufe';

export interface Conditions {
    permissions?: GlobalPermissions[] | undefined;
    roles?: Role[] | undefined;
    feature?: (() => boolean) | undefined;
}

export interface NavItem extends NavigationMenuItem {
    conditions?: Conditions | undefined;
    children?: NavItem[] | undefined;
}

const allNavItems: NavItem[] = [
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
                    feature: () => useProfundumEinwahl().isEinwahlActive,
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

function evaluateCondition(item: NavItem, user: ReturnType<typeof useUser>): boolean {
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

function evaluateItems(items: NavItem[], user: ReturnType<typeof useUser>): NavItem[] {
    const selectedItems: NavItem[] = [];

    for (const item of items) {
        if (!evaluateCondition(item, user)) continue;
        let workingCopy = item;
        if (item.children && item.children.length > 0) {
            const children = evaluateItems(item.children, user);
            workingCopy = Object.assign({}, workingCopy, { children: children });
        }
        if (workingCopy.to || (workingCopy.children && workingCopy.children.length > 0))
            selectedItems.push(workingCopy);
    }
    return selectedItems;
}

export function useNavItems() {
    const user = useUser();
    return computed(() => evaluateItems(allNavItems, user));
}

export function useFlatNavItems() {
    const nested = useNavItems();
    return computed(() => flatten(nested.value));
}

function flatten(items: NavItem[], prefix = ''): NavItem[] {
    const result: NavItem[] = [];
    for (const item of items) {
        if (item.children && item.children.length > 0) {
            result.push(...flatten(item.children, item.label ? `${item.label}: ` : prefix));
        } else if (item.to) {
            result.push({ ...item, label: `${prefix}${item.label}` });
        }
    }
    return result;
}
