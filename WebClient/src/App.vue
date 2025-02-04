<script setup>
import '@/assets/main.css'
import 'primeicons/primeicons.css'

import AfraNav from "@/components/AfraNav.vue";
</script>

<template>
  <afra-nav />
  <main>
    <RouterView></RouterView>
  </main>
  <footer>
    <p>Provided by <a class="font-bold" href="https://verein-der-altafraner.de" target="_blank">Verein der Altafraner</a></p>
  </footer>
</template>

<style scoped>

</style>
