<script setup>
import {ref} from "vue";
import {Button, Badge} from "primevue";
import InputGroup from "primevue/inputgroup";

const props=defineProps({
  mayEdit: Boolean
})
const emit=defineEmits(["valueChanged"])
const status=defineModel({default: 0})

const toggle = (value) => {
  status.value = value
  emit("valueChanged")
};

const severities = ['danger', 'warn', 'success']
const labels = ['Fehlend', 'Entschuldigt', 'Bestätigt']
</script>

<template>
  <InputGroup v-if="mayEdit">
    <Button :severity="status===0 ? 'danger' : 'secondary'" size="small" label="Fehlend" @click="() => toggle(0)"/>
    <Button :severity="status===1 ? 'warn' : 'secondary'" size="small" label="Entschuldigt" @click="() => toggle(1)"/>
    <Button :severity="status===2 ? 'success' : 'secondary'" size="small" label="Bestätigt" @click="() => toggle(2)"/>
  </InputGroup>
  <Badge v-else :severity="severities[status]">{{labels[status]}}</Badge>
</template>

<style scoped>
</style>
