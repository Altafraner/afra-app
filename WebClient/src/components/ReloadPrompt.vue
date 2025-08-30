<script setup>
import { useRegisterSW } from 'virtual:pwa-register/vue';
import { Button, Message } from 'primevue';

const { needRefresh, updateServiceWorker } = useRegisterSW();
</script>

<template>
    <Message
        v-if="needRefresh"
        severity="info"
        :closable="false"
        class="!rounded-none fixed bottom-0 left-0 right-0 z-50 flex items-center justify-between px-4 py-3 bg-surface-0 dark:bg-surface-900"
    >
        <span> Für die Afra-App ist ein Update verfügbar. </span>

        <Button
            label="Aktualisieren"
            icon="pi pi-refresh"
            severity="success"
            class="ml-4 shrink-0"
            @click="updateServiceWorker()"
        />
    </Message>
</template>
