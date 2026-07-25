<script setup>
import { useLogo } from '@/composables/logo';

const logo = useLogo();
</script>

<template>
    <div class="flex flex-col items-center justify-center w-full h-full gap-4">
        <img :src="logo" alt="" height="200" />
        <h1 class="text-highlighted my-0 text-4xl">Seite nicht gefunden</h1>
        <p class="text-muted max-w-100 text-center">
            Die Seite, die sie angefordert haben, konnte nicht gefunden werden.
        </p>
        <UButton label="Zur Startseite" size="lg" to="/" />
    </div>
</template>

<style scoped></style>
