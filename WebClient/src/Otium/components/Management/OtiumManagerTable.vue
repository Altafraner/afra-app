<script setup>
import { Button, Column, DataTable } from 'primevue';

const props = defineProps({
    managers: Array,
});
</script>

<template>
    <DataTable :value="managers">
        <Column field="vorname" header="Nachname" />
        <Column field="nachname" header="Vorname" />
        <Column class="text-right afra-col-action">
            <template #header>
                <Button aria-label="Person hinzufügen" size="small" icon="pi pi-plus"></Button>
            </template>
            <template #body>
                <span class="inline-flex gap-1">
                    <Button
                        aria-label="Zugriff entfernen"
                        severity="danger"
                        size="small"
                        variant="text"
                        icon="pi pi-times"
                    ></Button>
                </span>
            </template>
        </Column>
    </DataTable>
</template>

<style scoped></style>
