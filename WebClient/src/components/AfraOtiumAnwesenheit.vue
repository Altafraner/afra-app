<script setup>
import {ref, defineProps} from "vue";
import {Button} from "primevue";

const props=defineProps({initialOn: Boolean})
const emit=defineEmits(["toggle"])
const isOn=ref(props.initialOn)

const toggle = () => {
  isOn.value = !isOn.value
  emit("toggle")
};
</script>

<template>
  <Button v-if="isOn" severity="success" size="small" label="Bestätigt" @click="toggle"/>
  <Button v-else severity="danger" size="small" label="Unbestätigt" @click="toggle"/>
</template>

<style scoped>

</style>
