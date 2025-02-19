<script setup>

const props = defineProps({
  color: String,
  label: String,
  icon: String,
  to: String
})

import {Button} from "primevue";
</script>

<template>
  <Button :aria-label="props.label" :icon="props.icon" variant="outlined" severity="contrast"
          style="width: 100%; text-decoration: none" as="router-link" :to="props.to">
    <div class="flex gap-2 align-center">
      <span class="ot-angebot-icon" :style="`background-color: ${props.color}`">
        <i :class="props.icon"/>
      </span>
      <span>{{ props.label }}</span>
    </div>
  </Button>
</template>

<style scoped>
.ot-angebot-icon {
  width: 2rem;
  height: 2rem;
  border-radius: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #fff;
}
</style>
