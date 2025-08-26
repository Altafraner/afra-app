<script setup>
import { Button, Column, DataTable, Skeleton, Tag, useToast } from 'primevue';
import { formatDate, formatStudent } from '@/helpers/formatters.js';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user.js';
import AuslastungsTag from '@/Otium/components/Shared/AuslastungsTag.vue';

const toast = useToast();
const user = useUser();
const termine = ref([]);
const mentees = ref([]);
const loading = ref(true);

const severity = {
    Okay: {
        severity: 'success',
        label: 'Ok',
    },
    Auffaellig: {
        severity: 'danger',
        label: 'Auffällig',
    },
};

async function update() {
    const getter = mande('/api/otium/teacher');
    loading.value = true;
    try {
        const result = await getter.get();
        termine.value = result.termine;
        mentees.value = result.mentees;
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Ein unerwarteter Fehler ist beim Laden der Daten aufgetreten',
        });
        await user.update();
    } finally {
        loading.value = false;
    }
}

update();
</script>

<template>
    <h1>Guten Tag {{ formatStudent(user.user) }}</h1>
    <h2>Betreute Otia</h2>
    <DataTable :value="termine" v-if="!loading">
        <Column header="Otium">
            <template #body="{ data }">
                <Button
                    :label="data.otium"
                    :to="{ name: 'Verwaltung-Termin', params: { terminId: data.id } }"
                    as="RouterLink"
                    variant="text"
                />
            </template>
        </Column>
        <Column header="Datum">
            <template #body="{ data }">
                {{ formatDate(new Date(data.datum)) }}
            </template>
        </Column>
        <Column header="Block">
            <template #body="{ data }">
                {{ data.block }}
            </template>
        </Column>
        <Column header="Auslastung">
            <template #body="{ data }">
                <auslastungs-tag :auslastung="data.auslastung" />
            </template>
        </Column>
        <template #empty>
            <div class="flex justify-center">
                Sie betreuen in den nächsten zwei Wochen keine Otia.
            </div>
        </template>
    </DataTable>
    <DataTable v-else :value="new Array(3)">
        <Column header="Otium">
            <template #body>
                <Skeleton height="1.5rem" />
            </template>
        </Column>
        <Column header="Datum">
            <template #body>
                <Skeleton />
            </template>
        </Column>
        <Column header="Auslastung">
            <template #body>
                <Skeleton height="1.5rem" width="8rem" />
            </template>
        </Column>
    </DataTable>
    <h2>Ihre Mentees</h2>
    <DataTable :value="mentees" v-if="!loading">
        <Column header="Name">
            <template #body="{ data }">
                <Button
                    :label="formatStudent(data.mentee)"
                    :to="{ name: 'Mentee', params: { studentId: data.mentee.id } }"
                    as="RouterLink"
                    variant="text"
                />
            </template>
        </Column>
        <Column
            v-for="field in [
                { field: 'letzteWoche', header: 'Letzte' },
                { field: 'dieseWoche', header: 'Diese' },
                { field: 'nächsteWoche', header: 'Nächste' },
            ]"
            :key="field.field"
            :header="field.header"
        >
            <template #body="{ data }">
                <Tag
                    v-if="data.mentee.rolle !== 'Oberstufe'"
                    :severity="severity[data[field.field]].severity"
                >
                    {{ severity[data[field.field]].label }}
                </Tag>
                <Tag
                    v-else
                    v-tooltip="
                        'Schüler:innen der Oberstufe sind nicht an die üblichen Regeln gebungen.'
                    "
                    severity="secondary"
                >
                    N/A
                </Tag>
            </template>
        </Column>
        <template #empty>
            <div class="flex justify-center">Sie betreuen aktuell keine Schüler:innen.</div>
        </template>
    </DataTable>
    <DataTable v-else :value="new Array(3)">
        <Column header="Name">
            <template #body>
                <Skeleton height="1.5rem" />
            </template>
        </Column>
        <Column header="Letzte">
            <template #body>
                <Skeleton height="1.5rem" />
            </template>
        </Column>
        <Column header="Diese">
            <template #body>
                <Skeleton height="1.5rem" />
            </template>
        </Column>
        <Column header="Nächste">
            <template #body>
                <Skeleton height="1.5rem" />
            </template>
        </Column>
    </DataTable>
</template>

<style scoped></style>
