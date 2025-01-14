<template>
  <v-app>
    <SideBar />
    <v-app-bar title="Verwaltung" />
    <v-main class="">
      <UserManagement />
    </v-main>
  </v-app>
</template>

<script setup>
  //
import SideBar from "@/components/SideBar.vue";
  import UserManagement from "@/components/UserManagement.vue";
</script>
