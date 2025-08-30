<script setup>
import { Button } from 'primevue';
import { ref } from 'vue';

const emit = defineEmits(['update', 'edit']);
const props = defineProps({
    header: String,
    headerClass: {
        type: String,
        default: '',
    },
    hideEdit: Boolean,
});

const editMode = ref(false);

function edit() {
    emit('edit');
    editMode.value = true;
}

function confirm() {
    editMode.value = false;
    emit('update');
}
</script>

<template>
    <span :class="'font-bold ' + headerClass">{{ header }}</span>
    <template v-if="hideEdit">
        <span class="col-span-2"><slot name="body" /></span>
    </template>
    <template v-else>
        <span v-if="!editMode"><slot name="body" /></span>
        <span v-else><slot name="edit" /></span>
        <span class="self-start">
            <Button
                v-if="!editMode"
                icon="pi pi-pencil"
                severity="secondary"
                size="small"
                aria-label="Bearbeiten"
                @click="edit"
            />
            <Button
                v-else
                icon="pi pi-check"
                severity="success"
                size="small"
                aria-label="Bestätigen"
                @click="confirm"
            />
        </span>
    </template>
</template>

<style scoped></style>
