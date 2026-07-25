<script lang="ts" setup>
import { computed, onMounted } from 'vue';
import { mande } from 'mande';
import { useRouter } from 'vue-router';
import type {
    CommandPaletteGroup,
    CommandPaletteItem,
} from '@nuxt/ui/components/CommandPalette.d.vue.ts';
import { useColorMode } from '@vueuse/core';
import { useFlatNavItems } from '@/composables/navigationItems';
import { fuzzyMatch } from '@/helpers/fuzzy';
import { useLogout } from '@/composables/logout';
import { useUser } from '@/stores/user';
import { usePeople } from '@/stores/people';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { useProfunda } from '@/Profundum/stores/profundaStore';
import { useManagement } from '@/Profundum/composables/verwaltung.ts';
import { formatPerson } from '@/helpers/formatters';
import CreateOtiumForm from '@/Otium/components/Management/CreateOtiumForm.vue';
import CreateProfundumForm from '@/Profundum/components/Forms/CreateProfundumForm.vue';

interface OtiumManagementPreview {
    id: string;
    bezeichnung: string;
}

const emit = defineEmits<{ close: [] }>();

const router = useRouter();
const toast = useToast();
const overlay = useOverlay();
const user = useUser();
const { logout } = useLogout();
const people = usePeople();
const colorMode = useColorMode();
const flatItems = useFlatNavItems();
const otiumStore = useOtiumStore();
const profunda = useProfunda();
const profundumManagement = useManagement();

onMounted(() => {
    if (user.isOtiumsverantwortlich) otiumStore.updateManagementOtia();
    if (user.isProfundumsverantwortlich) profunda.updateProfunda();
    if (user.isAdmin) people.updatePersonen();
});

function fuzzyPostFilter(limit: number) {
    return (term: string, items: CommandPaletteItem[]) => {
        if (!term) return items.slice(0, limit);
        return items
            .map((item) => ({ item, score: fuzzyMatch(term, String(item.label ?? '')) }))
            .filter(
                (scored): scored is { item: CommandPaletteItem; score: number } =>
                    scored.score !== null,
            )
            .sort((a, b) => b.score - a.score)
            .slice(0, limit)
            .map((scored) => scored.item);
    };
}

const seitenGroup = computed<CommandPaletteGroup>(() => ({
    id: 'seiten',
    label: 'Seiten',
    ignoreFilter: true,
    items: flatItems.value.map((item) => ({
        label: item.label,
        icon: item.icon,
        to: item.to,
        onSelect: () => emit('close'),
    })),
    postFilter: fuzzyPostFilter(8),
}));

const MAX_ENTITY_RESULTS = 5;

const otiaGroup = computed<CommandPaletteGroup | null>(() => {
    const managementOtia = otiumStore.managementOtia as OtiumManagementPreview[] | null;
    if (!user.isOtiumsverantwortlich || !managementOtia) return null;
    return {
        id: 'otia',
        label: 'Otia',
        ignoreFilter: true,
        items: managementOtia.map((otium) => ({
            label: otium.bezeichnung,
            icon: 'i-lucide-list',
            to: { name: 'Verwaltung-Otium', params: { otiumId: otium.id } },
            onSelect: () => emit('close'),
        })),
        postFilter: fuzzyPostFilter(MAX_ENTITY_RESULTS),
    };
});

const profundaGroup = computed<CommandPaletteGroup | null>(() => {
    if (!user.isProfundumsverantwortlich || !profunda.profunda) return null;
    return {
        id: 'profunda',
        label: 'Profunda',
        ignoreFilter: true,
        items: profunda.profunda.map((p) => ({
            label: p.bezeichnung,
            icon: 'i-lucide-book-open',
            to: { name: 'Profundum-Edit', params: { profundumId: p.id } },
            onSelect: () => emit('close'),
        })),
        postFilter: fuzzyPostFilter(MAX_ENTITY_RESULTS),
    };
});

const createOtiumDialog = overlay.create(CreateOtiumForm);

async function createOtium() {
    emit('close');
    const data = await createOtiumDialog.open();
    if (!data) return;
    try {
        const id = await mande('/api/otium/management/otium').post<string>(data);
        toast.add({ color: 'success', title: 'Otium angelegt' });
        await otiumStore.updateManagementOtia(true);
        await router.push({ name: 'Verwaltung-Otium', params: { otiumId: id } });
    } catch (e: any) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Otium konnte nicht angelegt werden.',
        });
    }
}

const createProfundumDialog = overlay.create(CreateProfundumForm);

async function createProfundum() {
    emit('close');
    const [categories, fachbereiche] = await Promise.all([
        mande('/api/profundum/management/kategorie').get<unknown[]>(),
        profundumManagement.getFachbereiche(),
    ]);
    const data = await createProfundumDialog.open({
        categories,
        fachbereiche: fachbereiche ?? [],
    });
    if (!data) return;
    try {
        const id = await mande('/api/profundum/management/profundum').post<string>(data);
        toast.add({ color: 'success', title: 'Profundum angelegt' });
        await profunda.updateProfunda(true);
        await router.push({ name: 'Profundum-Edit', params: { profundumId: id } });
    } catch (e: any) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Profundum konnte nicht angelegt werden.',
        });
    }
}

async function impersonate(personId: string) {
    emit('close');
    try {
        await mande(`/api/user/${personId}/impersonate`).get();
        await user.update();
        await router.push('/');
    } catch {
        toast.add({ color: 'error', title: 'Impersonieren fehlgeschlagen' });
    }
}

const impersonateItem = computed<CommandPaletteItem | null>(() => {
    if (!user.isAdmin || !people.personen) return null;
    return {
        label: 'Impersonieren',
        icon: 'i-lucide-user-cog',
        children: people.personen.map((p) => ({
            label: formatPerson(p),
            onSelect: () => impersonate(p.id),
        })),
    };
});

const colorModeItem = computed<CommandPaletteItem>(() => ({
    label: 'Design',
    icon: 'i-lucide-sun-moon',
    children: [
        {
            label: 'Hell',
            icon: 'i-lucide-sun',
            onSelect: () => {
                emit('close');
                colorMode.value = 'light';
            },
        },
        {
            label: 'Dunkel',
            icon: 'i-lucide-moon',
            onSelect: () => {
                emit('close');
                colorMode.value = 'dark';
            },
        },
        {
            label: 'System',
            icon: 'i-lucide-monitor',
            onSelect: () => {
                emit('close');
                colorMode.value = 'auto';
            },
        },
    ],
}));

const aktionenGroup = computed<CommandPaletteGroup | null>(() => {
    const items: CommandPaletteItem[] = [];
    if (user.isOtiumsverantwortlich) {
        items.push({
            label: 'Neues Otium anlegen',
            icon: 'i-lucide-plus',
            onSelect: createOtium,
        });
    }
    if (user.isProfundumsverantwortlich) {
        items.push({
            label: 'Neues Profundum anlegen',
            icon: 'i-lucide-plus',
            onSelect: createProfundum,
        });
    }
    items.push(colorModeItem.value);
    if (impersonateItem.value) items.push(impersonateItem.value);
    items.push({
        label: 'Abmelden',
        icon: 'i-lucide-power',
        onSelect: () => {
            emit('close');
            logout();
        },
    });
    return {
        id: 'aktionen',
        label: 'Aktionen',
        ignoreFilter: true,
        items,
        postFilter: fuzzyPostFilter(items.length),
    };
});

const groups = computed<CommandPaletteGroup[]>(() =>
    [seitenGroup.value, aktionenGroup.value, otiaGroup.value, profundaGroup.value].filter(
        (g) => g !== null,
    ),
);
</script>

<template>
    <UModal>
        <template #content>
            <UCommandPalette
                :groups="groups"
                preserve-group-order
                placeholder="Seite, Profundum/Otium suchen oder Aktion ausführen…"
                close
                @update:open="(open: boolean) => !open && emit('close')"
            />
        </template>
    </UModal>
</template>

<style scoped></style>
