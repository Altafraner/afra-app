<script lang="ts" setup>
import { watch } from 'vue';

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
</script>

<template>
    <UHeader>
        <template #title>
            <img :src="logo" alt="Verein der Altafraner" class="h-10 w-auto inline-block" />
        </template>
        <UNavigationMenu :items="items" color="neutral" content-orientation="vertical" />
        <template #right>
            <UTooltip text="Seite suchen (Strg+K)">
                <UButton
                    class="text-muted hover:text-highlighted"
                    color="neutral"
                    icon="i-lucide-search"
                    variant="ghost"
                    aria-label="Seite suchen"
                    @click="commandPalette.open()"
                />
            </UTooltip>
            <UButton
                class="text-muted hover:text-highlighted"
                color="neutral"
                icon="i-lucide-power"
                variant="ghost"
                @click="logout"
                >Logout</UButton
            >
        </template>
        <template #body>
            <UNavigationMenu :items="items" color="neutral" orientation="vertical" />
        </template>
    </UHeader>
</template>

<style scoped></style>
