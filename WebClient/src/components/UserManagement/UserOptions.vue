<script setup>
const props = defineProps(['item'])
const emit = defineEmits(['user-deleted'])
const user = props.item

async function onDelete(evt) {
  const res = await fetch("http://localhost:5043/api/Person/" + user.id, {
    method: "DELETE"
  })

  if(res.ok) emit("user-deleted")
}
</script>

<template>
  <v-speed-dial location="left center" offset="8">
    <template v-slot:activator="{ props: activatorProps}">
      <v-btn
        v-bind="activatorProps"
        icon="mdi-dots-vertical"
        variant="plain"
        density="compact"
        size="small"
      ></v-btn>
    </template>

    <v-btn key="2" variant="elevated" icon="mdi-magnify" density="comfortable"></v-btn>
    <v-btn key="3" variant="elevated" icon="mdi-pencil" density="comfortable"></v-btn>
    <v-btn key="1" variant="elevated" icon="mdi-delete-outline" density="comfortable" @click="onDelete"></v-btn>
  </v-speed-dial>
</template>

<style scoped>

</style>
