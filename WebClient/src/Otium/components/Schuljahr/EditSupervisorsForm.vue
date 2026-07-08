<script setup>
import { mande } from 'mande';
import { computed, inject, shallowRef } from 'vue';
import { Button, Skeleton } from 'primevue';
import UserPeek from '@/components/UserPeek.vue';
import { usePeople } from '@/stores/people.js';
import PersonSelector from '@/components/PersonSelector.vue';

const dialogRef = inject('dialogRef');

const store = usePeople();

const date = dialogRef.value.data.date;
const blockId = dialogRef.value.data.blockId;
const result = shallowRef([]);
const loading = shallowRef(true);

const selectedPerson = shallowRef(null);

const requestApi = mande('/api/schuljahr/' + date);
const block = computed(() => result.value.find((r) => r.id === blockId));

async function setup() {
    result.value = await requestApi.get();
    await store.updatePersonen();
    loading.value = false;
}

async function remove(supervisor) {
    const api = mande(
        '/api/management/schuljahr/block/' + blockId + '/supervisors/' + supervisor.id,
    );
    await api.delete();
    result.value = await requestApi.get();
}

async function add() {
    if (selectedPerson.value == null) return;
    const api = mande('/api/management/schuljahr/block/' + blockId + '/supervisors');
    await api.post({
        value: selectedPerson.value,
    });
    result.value = await requestApi.get();
}

setup();
</script>

<template>
    <div v-if="loading" class="w-full">
        <Skeleton />
        <Skeleton />
        <Skeleton />
    </div>
    <div v-else class="flex flex-col gap-2">
        <div class="grid grid-cols-[1fr_auto] gap-2">
            <template v-for="supervisor in block.supervisors">
                <UserPeek :person="supervisor" />
                <span>
                    <Button
                        icon="pi pi-times"
                        severity="danger"
                        size="small"
                        variant="text"
                        @click="() => remove(supervisor)"
                    />
                </span>
            </template>
        </div>
        <div v-if="block.supervisors.length === 0" class="text-center">Keine Aufsichten</div>
        <PersonSelector
            v-model="selectedPerson"
            :filter="(p) => p.rolle === 'Tutor'"
            class="mt-2"
            fluid
            hide-rolle
        >
            <template #label> Neue Aufsicht </template>
        </PersonSelector>
        <Button fluid label="Hinzufügen" severity="primary" @click="add" />
    </div>
</template>

<style scoped></style>
