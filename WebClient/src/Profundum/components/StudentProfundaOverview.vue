<script lang="ts" setup>
import { mande } from 'mande';
import { ref } from 'vue';

const api = mande('/api/profundum/sus/enrollments');

const loading = ref(true);
const profunda = ref<EnrollmentInfo[]>([]);

api.get<EnrollmentInfo[]>().then((data) => {
    profunda.value = data;
    loading.value = false;
});
</script>
<script lang="ts">
import { TableColumn } from '@nuxt/ui';
import { formatSlotId } from '@/helpers/formatters.ts';

interface EnrollmentInfo {
    slotId: string;
    label?: string;
    location?: string;
}

const columns: TableColumn<EnrollmentInfo>[] = [
    {
        header: 'Slot',
        accessorFn: (data) => formatSlotId(data.slotId),
    },
    {
        header: 'Angebot',
        accessorFn: (data) => data.label ?? 'Keine Einschreibung',
    },
    {
        header: 'Ort',
        accessorFn: (data) => data.location ?? '–',
    },
];
</script>

<template>
    <UCard
        description="Hier siehst du die Profunda, in die du eingeschrieben bist."
        title="Deine Profunda"
    >
        <ASkeletonTable v-if="loading" />
        <template v-else>
            <UTable
                :columns="columns"
                :data="profunda"
                :ui="{
                    td: 'whitespace-normal text-default px-2 py-1.5',
                    th: 'px-2 py-1.5',
                    root: 'overflow-x-visible',
                }"
            />
        </template>
        <template #footer>
            <div class="text-sm text-muted">
                Du siehst deine Einschreibungen erst, wenn Sie durch die verantwortlichen
                veröffentlicht wurden.
            </div>
        </template>
    </UCard>
</template>

<style scoped></style>
