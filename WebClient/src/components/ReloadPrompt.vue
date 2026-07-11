<script setup>
import { useRegisterSW } from 'virtual:pwa-register/vue';

const { needRefresh, updateServiceWorker } = useRegisterSW();
</script>

<template>
    <UBanner
        v-if="needRefresh"
        class="fixed z-51"
        color="neutral"
        icon="i-lucide-triangle-alert"
        title="Für die Afra-App ist ein Update verfügbar."
        @click="updateServiceWorker"
    >
        <template #close
            ><UButton
                color="neutral"
                icon="i-lucide-rotate-cw"
                label="Aktualisieren"
                size="sm"
                variant="outline"
                @click="updateServiceWorker"
        /></template>
    </UBanner>
</template>
