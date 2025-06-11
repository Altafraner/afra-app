<script setup>
import {Badge, Button} from "primevue";
import InputGroup from "primevue/inputgroup";

const props = defineProps({
  mayEdit: Boolean
})
const emit = defineEmits(["valueChanged"])
const status = defineModel({default: "Fehlend"})

const toggle = (value) => {
  emit("valueChanged", stati[value])
};

const severities = {
  'Fehlend': 'danger',
  'Entschuldigt': 'warn',
  'Anwesend': 'success'
}
const stati = ['Fehlend', 'Entschuldigt', 'Anwesend']
</script>

<template>
  <InputGroup v-if="mayEdit">
    <Button :label="stati[0]" :severity="status===stati[0] ? 'danger' : 'secondary'" size="small"
            @click="() => toggle(0)"/>
    <Button :label="stati[1]" :severity="status===stati[1] ? 'warn' : 'secondary'" size="small"
            @click="() => toggle(1)"/>
    <Button :label="stati[2]" :severity="(status===stati[2]) ? 'success' : 'secondary'" size="small"
            @click="() => toggle(2)"/>
  </InputGroup>
  <Badge v-else :severity="severities[status]">{{ status }}</Badge>
</template>

<style scoped>
</style>
