<script setup>

</script>

<template>
  <v-navigation-drawer
    expand-on-hover
    rail
  >
    <v-list>
      <v-list-item
        prepend-avatar="https://randomuser.me/api/portraits/men/6.jpg"
        title="Richard Schlossarek"
        subtitle="r.schlossarek@verein-der-altafraner.de"
      ></v-list-item>
    </v-list>
    <v-divider></v-divider>
    <v-list density="compact" nav>
      <v-list-item
        prepend-icon="mdi-folder"
        title="Katalog"
        value="katalog" />
      <v-list-item
        prepend-icon="mdi-account-multiple"
        title="Einladungen"
        value="katalog" />
      <v-list-item
        prepend-icon="mdi-star"
        title="Favoriten"
        value="katalog" />
    </v-list>
  </v-navigation-drawer>
</template>

<style scoped>

</style>
