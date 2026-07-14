<script lang="ts" setup>
import { formatPerson } from '@/helpers/formatters';
import AuslastungsTag from '@/Otium/components/Shared/AuslastungsTag.vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';
import OtiumKategorieTag from '@/Otium/components/Shared/OtiumKategorieTag.vue';
import Termin from '@/Otium/components/Katalog/Termin.vue';
import { computed, h, ref, resolveComponent } from 'vue';
import MobileSwitch from '@/components/MobileSwitch.vue';
import MobileTerminCard from '@/Otium/components/Katalog/MobileTerminCard.vue';
import type { TableColumn } from '@nuxt/ui/components/Table.d.vue.ts';

const UButton = resolveComponent('UButton');
const UIcon = resolveComponent('UIcon');

const props = defineProps({
    otia: Array,
    terminId: {
        type: String,
        required: false,
        default: undefined,
    },
});

const emit = defineEmits(['reload']);

const settings = useOtiumStore();
const rowsExpanded = ref<Record<string, boolean>>({});
if (props.terminId) {
    rowsExpanded.value[props.terminId] = true;
}

function findKategorie(otium: any) {
    const katsAsAny = settings.kategorien as any[] | null;
    return katsAsAny?.find((k) => otium.kategorien.includes(k.id)) ?? null;
}

const processedOtia = computed(() => {
    return (
        props.otia?.map((ot: any) => {
            return Object.assign(
                {
                    kategorieFound: findKategorie(ot),
                    tutorName: ot.tutor ? formatPerson(ot.tutor) : '',
                    terminId: ot.id,
                },
                ot,
            );
        }) ?? []
    );
});

const columns: TableColumn<any>[] = [
    {
        accessorKey: 'otium',
        header: 'Angebot',
        cell: ({ row }) => {
            return h(
                UButton,
                {
                    label: row.getValue('otium'),
                    disabled: row.original.istAbgesagt,
                    color: !row.original.istEingeschrieben ? 'primary' : 'success',
                    size: 'lg',
                    variant: 'ghost',
                    onClick: () => row.toggleExpanded(),
                },
                () => [
                    !row.original.istAbgesagt
                        ? h(UIcon, {
                              name: row.getIsExpanded()
                                  ? 'i-lucide-chevron-down'
                                  : 'i-lucide-chevron-right',
                              class: 'size-5',
                          })
                        : null,
                    (row.original.kategorieFound?.icon ?? false)
                        ? h(OtiumKategorieTag, {
                              value: row.original.kategorieFound,
                              class: 'w-4',
                              hideName: true,
                              minimal: true,
                          })
                        : null,
                    h('span', { class: 'text-left' }, row.getValue('otium')),
                ],
            );
        },
    },
    {
        accessorKey: 'ort',
        header: 'Raum',
        cell: ({ row }) => row.getValue('ort'),
    },
    {
        accessorKey: 'tutorName',
        header: 'Betreuer:in',
        cell: ({ row }) => row.getValue('tutorName'),
    },
    {
        header: 'Auslastung',
        cell: ({ row }) =>
            h(AuslastungsTag, {
                auslastung: row.original.auslastung,
                istAbgesagt: row.original.istAbgesagt,
            }),
    },
];
</script>

<template>
    <MobileSwitch>
        <template #large>
            <UTable
                :columns="columns"
                :data="processedOtia"
                :ui="{
                    td: 'whitespace-normal text-default px-2 py-1.5',
                    th: 'px-2 py-1.5',
                    root: 'overflow-x-visible',
                }"
            >
                <template #expanded="{ row }">
                    <div class="w-full pl-4 text-default">
                        <Suspense>
                            <Termin
                                :termin-id="row.original.id"
                                @update="() => emit('reload')"
                            />
                            <template #fallback>
                                <div>
                                    <h1>
                                        <USkeleton class="h-12 w-[60%]" />
                                    </h1>
                                    <p>
                                        <USkeleton class="h-[1em] w-[40%]" />
                                    </p>
                                    <h3 class="mt-12">
                                        <USkeleton class="h-8 w-[55%]" />
                                    </h3>
                                </div>
                            </template>
                        </Suspense>
                    </div>
                </template>
                <template #empty>
                    <div class="flex justify-center">Keine Angebote verfügbar.</div>
                </template>
            </UTable>
        </template>
        <template #small>
            <template v-for="(termin, i) in processedOtia" :key="termin.id">
                <MobileTerminCard :termin="termin" @reload="() => emit('reload')" />
                <USeparator v-if="i !== processedOtia.length - 1" class="my-2" size="md" />
            </template>
        </template>
    </MobileSwitch>
</template>

<style scoped></style>
