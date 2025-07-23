<script setup>
import {computed, inject} from "vue";
import {Button, Menu} from "primevue";

const dialogRef = inject("dialogRef");
const items = computed(() => {
  return dialogRef.value.data.items.map(item => {
    return Object.assign({
      command: () => {
        dialogRef.value.close(item);
      }
    }, item)
  })
});

const cancel = () => dialogRef.value.close();
</script>

<template>
  <div class="flex flex-col gap-3">
    <Menu :model="items" class="w-full"/>
    <Button fluid label="Abbrechen" severity="secondary" @click="cancel"/>
  </div>
</template>

<style scoped>

</style>
