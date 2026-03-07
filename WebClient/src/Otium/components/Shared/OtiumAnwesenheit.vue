<script setup>
import { Badge, Button, Message } from 'primevue';
import InputGroup from 'primevue/inputgroup';

const props = defineProps({
    mayEdit: Boolean,
    minimal: Boolean,
});
const emit = defineEmits(['valueChanged']);
const status = defineModel({ default: 'Fehlend' });

const toggle = (value) => {
    emit('valueChanged', stati[value]);
};

const buttonSeverities = {
    Fehlend: 'danger',
    Entschuldigt: 'warn',
    Anwesend: 'success',
};

const messageSeverities = {
    Fehlend: 'error',
    Entschuldigt: 'warn',
    Anwesend: 'success',
};

const icons = {
    Fehlend: 'pi pi-times',
    Entschuldigt: 'pi pi-clipboard',
    Anwesend: 'pi pi-check',
};
const stati = ['Fehlend', 'Entschuldigt', 'Anwesend'];
</script>

<template>
    <InputGroup v-if="mayEdit">
        <Button
            :label="stati[0]"
            :severity="status === stati[0] ? 'danger' : 'secondary'"
            size="small"
            @click="() => toggle(0)"
        />
        <Button
            :label="stati[1]"
            :severity="status === stati[1] ? 'warn' : 'secondary'"
            size="small"
            @click="() => toggle(1)"
        />
        <Button
            :label="stati[2]"
            :severity="status === stati[2] ? 'success' : 'secondary'"
            size="small"
            @click="() => toggle(2)"
        />
    </InputGroup>
    <Badge v-else-if="!minimal" :severity="buttonSeverities[status]">{{ status }}</Badge>
    <Message
        v-else
        v-tooltip="status"
        :icon="icons[status]"
        :severity="messageSeverities[status]"
        variant="simple"
    />
</template>

<style scoped></style>
