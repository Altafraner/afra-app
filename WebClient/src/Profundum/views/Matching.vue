<script setup>
import { DataTable, Column, Button, Message, Select } from 'primevue';
import { mande } from 'mande';
import { computed, ref } from 'vue';

const slots = ref(['q1', 'q2', 'q3', 'q4']);
const enrollments = ref([
    {
        person: 'Johann',
        q1: 'swe',
        q2: 'swe',
        q3: 'swe',
        q4: 'swe',
    },
]);

async function getSlots() {
    const getter = mande('/api/profundum/management/slot');
    slots.value = await getter.get();
}

async function getEnrollments() {
    const getter = mande('/api/profundum/management/enrollments');
    enrollments.value = await getter.get();
}

async function autoMatching() {
    const getter = mande('/api/profundum/management/matching');
    await getter.post();
}

getSlots();
getEnrollments();
</script>

<template>
    <h1>Profunda-Matching</h1>

    <Button label="Automatisches Matching aktualisieren" @click="autoMatching" />

    <h2>Fest Eingeschriebene</h2>

    <Message class="mb-2" severity="warn">
        <div>Person X nicht eingeschrieben in Slot 2025-Q2</div>
        <div>Person Y belegt kein Profil Profundum</div>
        <div>Person Z erfüllt nicht die Klassenbeschränkung für SWE</div>
    </Message>

    <DataTable :value="enrollments" value-key="id">
        <Column>
            <template #body>
                <Button icon="pi pi-times-circle" severity="danger" variant="text" />
            </template>
        </Column>
        <Column field="id" header="Person" />
        <Column
            v-for="s of slots"
            :header="`${s.jahr}-${s.quartal}-${s.wochentag}`"
            :field="s.Id"
        >
            <template #body>
                <Select
                    option-label="label"
                    :options="[
                        { label: 'swe', value: 'swe' },
                        { label: 'italienisch', value: 'italienisch' },
                    ]"
                />
            </template>
        </Column>
    </DataTable>

    <h2>Automatische Zuordnungen</h2>

    <DataTable
        :value="[
            {
                person: 'Richard',
                q1: 'swe',
                q2: 'swe',
                q3: 'swe',
                q4: 'swe',
            },
        ]"
    >
        <Column>
            <template #body>
                <Button icon="pi pi-thumbtack" variant="text" />
            </template>
        </Column>
        <Column field="person" header="Person" />
        <Column field="q1" header="2025 Q1" />
        <Column field="q2" header="2025 Q2" />
        <Column field="q3" header="2025 Q3" />
        <Column field="q4" header="2025 Q4" />
    </DataTable>

    <Button label="Matching finalisieren" severity="warn" />
</template>

<style scoped></style>
