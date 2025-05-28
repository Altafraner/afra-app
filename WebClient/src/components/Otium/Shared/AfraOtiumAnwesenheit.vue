<script setup>
import {Badge, Button} from "primevue";
import InputGroup from "primevue/inputgroup";

const props = defineProps({
  mayEdit: Boolean
})
const emit = defineEmits(["valueChanged"])
const status = defineModel({default: "Fehlend"})

const toggle = (value) => {
  status.value = stati[value]
  emit("valueChanged")
};

const severities = {
  'Fehlend': 'danger',
  'Entschuldigt': 'warn',
  'Bestätigt': 'success'
}
const stati = ['Fehlend', 'Entschuldigt', 'Bestätigt']
</script>

<template>
  <InputGroup v-if="mayEdit">
    <Button :severity="status===stati[0] ? 'danger' : 'secondary'" label="Fehlend" size="small"
            @click="() => toggle(0)"/>
    <Button :severity="status===stati[1] ? 'warn' : 'secondary'" label="Entschuldigt" size="small"
            @click="() => toggle(1)"/>
    <Button :severity="(status===stati[2] || status==='anwesend') ? 'success' : 'secondary'"
            size="small"
            label="Bestätigt" @click="() => toggle(2)"/>
  </InputGroup>
  <Badge v-else :severity="severities[status]">{{ status }}</Badge>
</template>

<style scoped>
</style>
