<script setup>
import Menubar from 'primevue/menubar';
import {ref} from "vue";
import {Image, Button, useToast, Toast} from "primevue";

import wappen from '/Vereinswappen.jpg?url'
import {useUser} from "@/stores/useUser.js";

const items = ref([
  {
    label: "Dashboard",
    route: "/",
    icon: "pi pi-user"
  },
  {
    label: "Katalog",
    route: "/katalog",
    icon: "pi pi-list"
  },
  /*{
    label: "Test",
    route: "/test",
    icon: "pi pi-code"
  },*/
  {
    label: "Aufsicht",
    route: "/aufsicht",
    icon: "pi pi-eye"
  }
]);
const toast = useToast();

const logout = async () => {
  const user = useUser();
  try {
    await user.logout();
    toast.add({severity: "success", summary: "Abgemeldet!", detail: "Sie wurden erfolgreich abgemeldet.", life: 3000});
  } catch (error) {
    toast.add({severity: "error", summary: "Fehler!", detail: "Sie konnten nicht abgemeldet werden."});
  }
}
</script>


<template>
  <Menubar :model="items">
    <template #start>
      <Image :src="wappen" height="50"></Image>
    </template>
    <template #item="{item, props, hasSubmenu}">
      <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
        <a :href="href" v-bind="props.action" @click="navigate">
          <span v-if="item.icon" :class="item.icon"/>
          <span>{{ item.label }}</span>
        </a>
      </router-link>
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <span :class="item.icon"/>
        <span>{{ item.label }}</span>
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down"/>
      </a>
    </template>
    <template #end>
      <Button label="Logout" icon="pi pi-power-off" @click="logout" variant="text" severity="secondary"/>
    </template>
  </Menubar>
</template>

<style scoped>

</style>
