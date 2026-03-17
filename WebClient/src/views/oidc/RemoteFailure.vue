<script lang="ts" setup>
import { Button } from 'primevue';
import { RouterLink } from 'vue-router';
</script>

<template>
    <h1 class="mt-20">Authentifizierungsfehler</h1>
    <p>Der Authentifizierungsdienst hat einen Fehler gemeldet.</p>
    <p>
        Probieren Sie es später erneut. Sollte der Fehler bestehen bleiben, wenden Sie sich
        bitte an den Administrator.
    </p>
    <Button :as="RouterLink" class="mt-2" label="Zurück zur Startseite" to="/" />
</template>

<style scoped></style>
