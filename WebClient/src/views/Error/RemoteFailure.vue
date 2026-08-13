<script lang="ts" setup></script>

<template>
    <div class="flex flex-col items-center justify-center w-full h-full gap-4">
        <h1 class="text-highlighted my-0 text-4xl">Externer Fehler</h1>
        <p class="text-muted max-w-100 text-center">
            Der Authentifizierungsdienst hat einen Fehler gemeldet. Probieren Sie es später
            erneut. Sollte dieser Fehler bestehen bleiben, wenden Sie sich bitte an den
            Administrator.
        </p>
        <UButton label="Zur Startseite" size="lg" to="/" />
    </div>
</template>

<style scoped></style>
