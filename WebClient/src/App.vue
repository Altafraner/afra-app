<script setup>
import '@/assets/main.css'
import 'primeicons/primeicons.css'

import AfraNav from "@/components/AfraNav.vue";
</script>

<template>
  <afra-nav />
  <main class="flex justify-center">
    <div class="container">
      <RouterView></RouterView>
    </div>
  </main>
  <footer>
    <p>Provided by <a class="font-bold" href="https://verein-der-altafraner.de" target="_blank">Verein der Altafraner</a></p>
  </footer>
</template>

<style scoped>
  .container{
    width: 50rem;
  }

  @media screen and (width < 55rem) {
    .container{
      width: 100%;
    }
  }
</style>
