﻿<script setup>
import Menubar from 'primevue/menubar';
import {ref} from "vue";
import {Button, Image, useToast} from "primevue";

import wappen from '/vdaa/Vereinswappen.jpg?url';
import {useUser} from "@/stores/user.js";
import {useRouter} from "vue-router";

const items_teacher = [
  {
    label: "Übersicht",
    route: "/",
    icon: "pi pi-user"
  },
  {
    label: "Katalog",
    route: {
      name: "Katalog"
    },
    icon: "pi pi-list"
  },
  {
    label: "Aufsicht",
    route: {
      name: "Aufsicht"
    },
    icon: "pi pi-eye"
  }
]

const items_student = [
  {
    label: "Übersicht",
    route: "/",
    icon: "pi pi-user"
  },
  {
    label: "Katalog",
    route: {
      name: "Katalog"
    },
    icon: "pi pi-list"
  },
]

const items_otium_manager = [
  {
    label: "Verwaltung",
    route: {
      name: "Verwaltung"
    },
    icon: "pi pi-cog"
  }
]

const toast = useToast();
const router = useRouter();
const items = ref([])
const user = useUser();

const logout = async () => {
  const user = useUser();
  try {
    await user.logout();
    await router.push("/");
    toast.add({
      severity: "success",
      summary: "Abgemeldet!",
      detail: "Sie wurden erfolgreich abgemeldet.",
      life: 3000
    });
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Fehler!",
      detail: "Sie konnten nicht abgemeldet werden."
    });
  }
}

async function setup(update = true) {
  if (update) await user.update()
  if (user.loading) return;
  if (user.isStudent) {
    items.value = items_student
  } else if (user.isTeacher) {
    items.value = items_teacher
  } else {
    items.value = []
  }

  if (user.user.berechtigungen.includes("Otiumsverantwortlich")) {
    items.value = [...items.value, ...items_otium_manager];
  }
}

setup()

user.$subscribe(() => {
  setup(false);
})
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
      <Button label="Logout" icon="pi pi-power-off" @click="logout" variant="text"
              severity="secondary"/>
    </template>
  </Menubar>
</template>

<style scoped>

</style>
