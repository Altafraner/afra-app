<script setup>
import {ref} from 'vue'

const title = ref('');
const content = ref('');

const props = defineProps({
  title: String,
  content: String
})
title.value = props['title'];
content.value = props['content'];

</script>

<template>
  <h1>{{title}}</h1>
  <p>{{content}}</p>
</template>

<style scoped>

</style>
