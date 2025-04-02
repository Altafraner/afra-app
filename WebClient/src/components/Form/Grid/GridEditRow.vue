<script setup>
import {Button} from "primevue"
import {ref} from "vue";

const emit = defineEmits(['update'])
const props = defineProps({
  header: String,
  headerClass: {
    type: String,
    default: ''
  },
})

const editMode = ref(false)

function edit() {
  editMode.value = true;
}

function confirm() {
  editMode.value = false;
  emit('update')
}

</script>

<template>
  <span :class="'font-bold ' + headerClass">{{ header }}</span>
  <span v-if="!editMode"><slot name="body"/></span>
  <span v-else><slot name="edit"/></span>
  <span class="self-start">
    <Button v-if="!editMode" icon="pi pi-pencil" severity="secondary"
            size="small" @click="edit"/>
    <Button v-else icon="pi pi-check" severity="success"
            size="small"
            @click="confirm"/>
  </span>
</template>

<style scoped>

</style>
