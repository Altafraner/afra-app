<script setup>
import { useUser } from '@/stores/user';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { h, ref, shallowRef, watch } from 'vue';
import { mande } from 'mande';
import { findPath } from '@/helpers/tree.js';
import SimpleBreadcrumb from '@/components/SimpleBreadcrumb.vue';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import CreateOtiumForm from '@/Otium/components/Management/CreateOtiumForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';
import ASkeletonTable from '@/components/Layout/ASkeletonTable.vue';

const user = useUser();
const settings = useOtiumStore();
const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const loading = ref(true);
const showHidden = shallowRef(false);
const overlay = useOverlay();

const otia = shallowRef([]);

async function getOtia() {
    const getter = mande('/api/otium/management/otium');
    otia.value = await getter.get({ query: { includeHidden: showHidden.value } });
}

async function deleteOtium(id) {
    const api = mande('/api/otium/management/otium/' + id);
    try {
        await api.delete();
    } catch {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Löschen des Otiums aufgetreten',
        });
    } finally {
        await getOtia();
    }
}

async function createOtium(data) {
    const api = mande('/api/otium/management/otium');
    try {
        await api.post(data);
    } catch {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Erstellen des Otiums aufgetreten',
        });
    } finally {
        await getOtia();
    }
}

const createDialog = overlay.create(CreateOtiumForm);

async function openCreateDialog() {
    const data = await createDialog.open();
    if (!data) return;
    await createOtium(data);
}

const confirmDelete = async (id) => {
    if (await requireConfirm('Wollen Sie das Otium wirklich löschen?')) await deleteOtium(id);
};

async function setup() {
    try {
        await settings.updateKategorien();
        await getOtia();
        loading.value = false;
    } catch {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        await user.update();
    }
}

async function hide(data, value) {
    try {
        const api = mande(`/api/otium/management/otium/${data.id}/hidden`);
        await api.put(null, { query: { value: value } });
    } catch {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Ein unerwarteter Fehler ist beim Verstecken aufgetreten',
        });
    } finally {
        await getOtia();
    }
}

setup();

watch(showHidden, getOtia);

const columns = [
    {
        header: 'Bezeichnung',
        accessorKey: 'bezeichnung',
        cell: ({ row }) =>
            h(UButton, {
                label: row.getValue('bezeichnung'),
                variant: 'ghost',
                to: { name: 'Verwaltung-Otium', params: { otiumId: row.original.id } },
                ui: { label: 'whitespace-normal' },
            }),
    },
    {
        header: 'Kategorie',
        cell: ({ row }) =>
            h(
                SimpleBreadcrumb,
                {
                    wrap: true,
                    model: findPath(settings.kategorien, row.original.kategorie),
                },
                {
                    item: ({ item }) => h(OtiumKategorieTag, { minimal: true, value: item }),
                },
            ),
    },
    {
        header: 'Termine',
        accessorKey: 'termine',
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
    },
    {
        id: 'action',
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
        header: () =>
            h(
                UTooltip,
                {
                    text: 'Neues Otium',
                },
                () => [
                    h(UButton, {
                        onClick: openCreateDialog,
                        icon: 'i-lucide-plus',
                    }),
                ],
            ),
        cell: ({ row }) =>
            h('span', { class: 'flex gap-1 justify-end' }, [
                !row.original.termine || row.original.termine.length === 0
                    ? h(UTooltip, { text: 'Löschen' }, () => [
                          h(UButton, {
                              variant: 'ghost',
                              icon: 'i-lucide-x',
                              color: 'error',
                              onClick: () => confirmDelete(row.original.id),
                          }),
                      ])
                    : h(
                          UTooltip,
                          { text: 'Nur Otia ohne Termine können gelöscht werden.' },
                          () => [
                              h(UButton, {
                                  variant: 'ghost',
                                  icon: 'i-lucide-x',
                                  color: 'neutral',
                                  disabled: true,
                              }),
                          ],
                      ),
                !row.original.hidden
                    ? h(UTooltip, { text: 'Verstecken' }, () => [
                          h(UButton, {
                              variant: 'ghost',
                              icon: 'i-lucide-eye',
                              color: 'primary',
                              onClick: () => hide(row.original, true),
                          }),
                      ])
                    : h(UButton, {
                          variant: 'ghost',
                          icon: 'i-lucide-eye-off',
                          color: 'warning',
                          onClick: () => hide(row.original, false),
                      }),
            ]),
    },
];
</script>

<template>
    <template v-if="!loading">
        <h2>Alle Otia</h2>
        <p>Klicken sie auf ein Otium, um Details zu sehen oder es zu Bearbeiten.</p>
        <UTable
            :columns="columns"
            :data="otia"
            :ui="{
                td: 'whitespace-normal text-default px-2 py-1.5',
                th: 'px-2 py-1.5',
                root: 'overflow-x-visible',
            }"
        />
        <div class="flex mt-4">
            <UButton
                v-if="!showHidden"
                color="neutral"
                label="Ausgeblendete anzeigen"
                icon="i-lucide-eye"
                @click="showHidden = true"
            />
            <UButton
                v-else
                color="neutral"
                label="Ausgeblendete verbergen"
                icon="i-lucide-eye-off"
                @click="showHidden = false"
            />
        </div>
    </template>
    <template v-else>
        <p><USkeleton class="mb-6 w-full h-12" /></p>
        <p><USkeleton class="mb-4 w-full h-4" /></p>
        <ASkeletonTable />
    </template>
</template>

<style scoped></style>
