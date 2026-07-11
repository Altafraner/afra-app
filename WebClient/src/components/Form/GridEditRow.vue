<script setup>
import { ref } from 'vue';

const emit = defineEmits(['update', 'edit', 'delete']);

const props = defineProps({
    header: String,
    headerClass: {
        type: String,
        default: '',
    },
    hideEdit: Boolean,
    canDelete: {
        type: Boolean,
        default: false,
    },
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

function remove() {
    emit('delete');
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

        <span class="self-start flex gap-1">
            <UButton
                v-if="!editMode"
                color="neutral"
                icon="i-lucide-pencil"
                aria-label="Bearbeiten"
                variant="ghost"
                @click="edit"
            />
            <UButton
                v-else
                color="success"
                icon="i-lucide-check"
                aria-label="Bestätigen"
                variant="subtle"
                @click="confirm"
            />

            <UButton
                v-if="canDelete && editMode"
                color="error"
                icon="i-lucide-trash"
                aria-label="Löschen"
                variant="subtle"
                @click="remove"
            />
        </span>
    </template>
</template>
