<script lang="ts" setup>
import { watch, type ComputedRef, computed } from 'vue';
import type { DropdownMenuItem } from '@nuxt/ui/components/DropdownMenu.d.vue.ts';
import { useUser } from '@/stores/user';
import { useProfundumEinwahl } from '@/Profundum/stores/profundumEinwahlStore';
import { useLogo } from '@/composables/logo';
import { useNavItems } from '@/composables/navigationItems';
import { useCommandPalette } from '@/composables/commandPalette';
import { useLogout } from '@/composables/logout';

const user = useUser();
const profundumEinwahl = useProfundumEinwahl();
const commandPalette = useCommandPalette();
const { logout } = useLogout();

watch(
    () => user.isMittelstufe,
    (isMittelstufe) => {
        if (isMittelstufe) {
            profundumEinwahl.update();
        }
    },
    { immediate: true },
);

const items = useNavItems();
const logo = useLogo();

const userMenuItems: ComputedRef<DropdownMenuItem[][]> = computed(() => [
  [
    {
      label: 'Account',
      icon: 'i-lucide-user',
      target: '_blank',
      to: user.user?.accountManagementUrl,
      disabled: !user.user?.accountManagementUrl,
    },
    {
      label: 'Schnellzugriff',
      icon: 'i-lucide-search',
      onSelect: () => commandPalette.open(),
      kbds: ['STRG', 'K']
    },
    {
      label: 'Einstellungen',
      icon: 'i-lucide-settings',
      to: {
        name: 'Settings',
      },
    },
  ],
  [
    {
      label: 'Logout',
      icon: 'i-lucide-log-out',
      onSelect: logout,
    },
  ],
]);
</script>

<template>
    <UHeader>
        <template #title>
            <img :src="logo" alt="Verein der Altafraner" class="h-10 w-auto inline-block" />
        </template>
        <UNavigationMenu :items="items" color="neutral" content-orientation="vertical" />
        <template #right>
            <UDropdownMenu
                :items="userMenuItems"
                :ui="{ content: 'w-(--reka-dropdown-menu-trigger-width)' }"
            >
                <UButton
                    :avatar="{
                        alt: `${user.user?.vorname} ${user.user?.nachname}`,
                    }"
                    :label="
                        user.user ? `${user.user?.vorname} ${user.user?.nachname}` : 'Nutzer'
                    "
                    :ui="{
                        label: 'max-w-[16ch]',
                    }"
                    color="secondary"
                    variant="ghost"
                />
            </UDropdownMenu>
        </template>
        <template #body>
            <UNavigationMenu :items="items" color="neutral" orientation="vertical" />
        </template>
    </UHeader>
</template>

<style scoped></style>
