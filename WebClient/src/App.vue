<script setup>
import '@/assets/main.css'
import 'primeicons/primeicons.css'

import AfraNav from "@/components/AfraNav.vue";
import {useUser} from "@/stores/useUser.js";
import wappen from '/favicon.svg?url'
import {Image, Message, Skeleton, Toast, useToast} from "primevue";
import Login from "@/components/Login.vue";

const user = useUser();
const toast = useToast();
user.update().catch(() => {
  toast.add({
    severity: "error",
    summary: "Fehler",
    detail: "Ein unerwarteter Fehler ist beim Laden der Nutzerdaten aufgetreten"
  });
})
</script>

<template>
  <Toast/>
  <Message severity="warn" closable>
    Sie sehen eine Testversion der Otiums-App. Alle Daten können jederzeit gelöscht werden.
  </Message>
  <template v-if="!user.loading">
    <afra-nav v-if="user.loggedIn"/>
    <main class="flex justify-center min-h-[90vh] mt-4">
      <div v-if="user.loggedIn" class="container">
        <RouterView/>
      </div>
      <div class="min-container" v-else>
        <div class="flex justify-center">
          <Image :src="wappen" height="200"></Image>
        </div>
        <h1>Willkommen bei der Otiumsverwaltung</h1>
        <p>Bitte logge dich ein, um die Otiumsverwaltung zu nutzen.</p>
        <Login></Login>
      </div>
    </main>
  </template>
  <template v-else>
    <Skeleton width="100%" height="4rem"/>
    <main class="flex justify-center min-h-[90vh] mt-4">
      <div class="container">
        <h1>
          <Skeleton width="60%" height="3rem"/>
        </h1>
        <p class="flex gap-2 flex-col">
          <Skeleton width="100%" height="1rem"/>
          <Skeleton width="100%" height="1rem"/>
          <Skeleton width="60%" height="1rem"/>
        </p>
        <p class="flex gap-2 flex-col">
          <Skeleton width="100%" height="1rem"/>
          <Skeleton width="100%" height="1rem"/>
          <Skeleton width="100%" height="1rem"/>
          <Skeleton width="30%" height="1rem"/>
        </p>
      </div>
    </main>
  </template>
  <footer class="bg-primary w-full p-6 mt-[1rem] text-center text-primary-contrast">
    <p class="h-[1.2em]">Provided by <a
      class="font-bold inline-block text-primary-contrast underline decoration-primary hover:decoration-primary-contrast transition-all"
      href="https://verein-der-altafraner.de" target="_blank">Verein der Altafraner</a></p>
  </footer>
</template>

<style scoped>
.min-container {
  max-width: min(95%, 50rem);
  margin-top: 5rem;
}

.container {
  width: 50rem;
}

@media screen and (width < 55rem) {
  .container {
    width: 95%;
  }
}
</style>
