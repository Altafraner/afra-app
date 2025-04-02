<script setup>
import {Tag} from "primevue";

const props = defineProps({
  value: Object,
  minimal: Boolean
})

</script>

<template>
  <Tag v-if="!minimal" severity="secondary">
    <span v-if="props.value.icon"
          :class="`ot-angebot-icon ${props.value.cssColor ? 'ot-angebot-white' : ''}`"
          :style="`background-color: ${props.value.cssColor ?? 'unset'}`">
      <i :class="props.value.icon"/>
    </span>
    <span>
      {{ props.value.bezeichnung }}
    </span>
  </Tag>
  <span v-else class="inline-flex items-baseline justify-center gap-1">
    <i :class="props.value.icon" :style="`color: ${props.value.cssColor ?? 'inherit'}`"/>
    <span>
      {{ props.value.bezeichnung }}
    </span>
  </span>
</template>

<style scoped>
.ot-angebot-icon {
  font-size: 0.8em;
  width: 2em;
  height: 2em;
  border-radius: 1em;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.ot-angebot-icon i {
  font-size: 1.1em;
}

.ot-angebot-white {
  color: #fff;
}
</style>
