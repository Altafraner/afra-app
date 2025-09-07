<script setup>
import { Button, Image } from 'primevue';
import { computed } from 'vue';
import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { isDark } from '@/helpers/isdark.js';

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
</script>

<template>
    <div class="flex justify-center">
        <Image :src="logo" height="200"></Image>
    </div>
    <h1>Seite nicht gefunden</h1>
    <p>Die Seite, die sie angefordert haben, konnte nicht gefunden werden.</p>
    <p>Möglicherweise ist die Funktion, die Sie suchen, noch nicht fertiggestellt.</p>
    <Button as="RouterLink" to="/" label="Zurück zur Startseite" class="mt-2" />
</template>

<style scoped></style>
