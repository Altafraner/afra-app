<script setup>
import {Tag} from "primevue";

const props = defineProps({
  value: Object
})

console.log(props.value)
</script>

<template>
  <Tag severity="secondary">
    <span v-if="props.value.icon"
      :class="`ot-angebot-icon ${props.value.color ? 'ot-angebot-white' : ''}`"
      :style="`background-color: ${props.value.color ?? 'unset'}`">
      <i :class="props.value.icon"/>
    </span>
    <span>
      {{ props.value.name }}
    </span>
  </Tag>
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

.ot-angebot-icon i{
  font-size: 1.1em;
}

.ot-angebot-white{
  color: #fff;
}
</style>
