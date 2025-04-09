<script setup>
const props = defineProps({
  model: {
    type: Array,
    required: true
  },
  separator: {
    type: String,
    default: '/'
  },
  wrap: {
    type: Boolean,
    default: false
  }
});
</script>

<template>
  <div :class="`flex items-baseline ${wrap ? 'flex-wrap' : ''}`">
    <template v-for="(item, index) in model">
      <slot :item="item" name="item"/>
      <span v-if="index < model.length - 1" class="mx-2">
        <slot name="separator">{{ separator }}</slot>
      </span>
    </template>
  </div>
</template>

<style scoped>

</style>
