<script setup>
import {Button, Image} from "primevue";
import wappen from '/vdaa/favicon.svg?url'
</script>

<template>
  <div class="flex justify-center">
    <Image :src="wappen" height="200"></Image>
  </div>
  <h1>Seite nicht gefunden</h1>
  <p>Die Seite, die sie angefordert haben, konnte nicht gefunden werden.</p>
  <p>Möglicherweise ist die Funktion, die Sie suchen, noch nicht fertiggestellt.</p>
  <Button as="RouterLink" to="/" label="Zurück zur Startseite" class="mt-2"/>
</template>

<style scoped>

</style>
