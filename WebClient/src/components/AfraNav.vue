<script setup>
import Menubar from 'primevue/menubar';
import {ref} from "vue";
import {Image} from "primevue";

const items = ref([
  {
    label: "Mein Tag",
    route: "/me",
    icon: "pi pi-user"
  },
  {
    label: "Katalog",
    route: "/katalog",
    icon: "pi pi-list"
  },
  {
    label: "Test",
    route: "/test",
    icon: "pi pi-code"
  }
]);
</script>

<template>
  <Menubar :model="items">
    <template #start>
      <Image src="/Vereinswappen.jpg" height="50"></Image>
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
  </Menubar>
</template>

<style scoped>

</style>
