<script setup>
import {ref} from "vue";
import {Button} from "primevue";

const props=defineProps({
  initialStatus: Number,
  mayEdit: Boolean
})
const emit=defineEmits(["valueChanged"])
const status=ref(props.initialStatus ?? 0)

const toggle = (value) => {
  console.info("Clicked Button", value)
  status.value = value
  emit("valueChanged")
};
</script>

<template>
  <div class="inline-flex gap-1 justify-stretch" style="width: 100%">
    <Button v-if="status===0 || mayEdit" :disabled="!props.mayEdit" :severity="status===0 ? 'danger' : 'secondary'" size="small" label="Fehlend" @click="() => toggle(0)"/>
    <Button v-if="status===1 || mayEdit" :disabled="!props.mayEdit" :severity="status===1 ? 'warn' : 'secondary'" size="small" label="Entschuldigt" @click="() => toggle(1)"/>
    <Button v-if="status===2 || mayEdit" :disabled="!props.mayEdit" :severity="status===2 ? 'success' : 'secondary'" size="small" label="Bestätigt" @click="() => toggle(2)"/>
  </div>
</template>

<style scoped>
button {
}
</style>
