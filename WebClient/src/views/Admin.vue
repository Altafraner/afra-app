<script setup>
import {useUser} from "@/stores/user.js";
import {useOtiumStore} from "@/Otium/stores/otium.js";
import {computed} from "vue";
import {formatTutor} from "@/helpers/formatters.js";
import {Button} from "primevue";
import {mande} from "mande";
import {useRouter} from "vue-router";

const user = useUser();
const otium = useOtiumStore();
const router = useRouter();

await otium.updatePersonen();

const isAdmin = computed(() => user.loggedIn && user.user.berechtigungen.includes('Admin'));
const personen = computed(() => otium.personen.sort((a, b) =>
  formatTutor(a).toLowerCase() < formatTutor(b).toLowerCase() ? -1 : 1));
const impersonate = async (userToImpersonate) => {
  console.log(userToImpersonate);
  await mande('/api/user/' + userToImpersonate.id + '/impersonate').get();
  await user.update();
  await router.push('/');
}
</script>

<template>
  <template v-if="isAdmin">
    <h1>Admin-Bereich</h1>
    <h2>Impersonieren</h2>
    <ul>
      <li v-for="user in personen" :key="user.id">
        <Button :label="formatTutor(user)" class="mt-2 min-w-[15rem]"
                size="small" variant="secondary" @click="() => impersonate(user)"/>
      </li>
    </ul>
  </template>
  <template v-else>
    <h1>Kein Zugriff</h1>
    <p>Du hast keine Berechtigung, auf den Admin-Bereich zuzugreifen.</p>
  </template>
</template>

<style scoped>

</style>
