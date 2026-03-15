<script lang="ts" setup>
import { Button } from 'primevue';
import { RouterLink } from 'vue-router';
</script>

<template>
    <h1 class="mt-20">Zugriff verweigert</h1>
    <p>Der Authentifizierungsdienst hat den Zugriff verweigert.</p>
    <p>Wenn dieser Fehler unerwartet ist, wenden Sie sich bitte an den Administrator.</p>
    <Button :as="RouterLink" class="mt-2" label="Zurück zur Startseite" to="/" />
</template>

<style scoped></style>
