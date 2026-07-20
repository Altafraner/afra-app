<script setup>
import { computed } from 'vue';
import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { isDark } from '@/helpers/isdark';

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
</script>

<template>
    <div class="flex justify-center">
        <img :src="logo" alt="" height="200" />
    </div>
    <h1>Seite nicht gefunden</h1>
    <p>Die Seite, die sie angefordert haben, konnte nicht gefunden werden.</p>
    <p>Möglicherweise ist die Funktion, die Sie suchen, noch nicht fertiggestellt.</p>
    <UButton class="mt-2" label="Zurück zur Startseite" to="/" />
</template>

<style scoped></style>
