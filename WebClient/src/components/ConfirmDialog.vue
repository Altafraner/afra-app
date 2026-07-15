<script lang="ts" setup>
defineProps<{
    color: 'info' | 'secondary' | 'neutral' | 'success' | 'warning' | 'error';
    header: string;
    message: string;
}>();

defineEmits<{ close: [boolean] }>();
</script>

<template>
    <UModal :title="header">
        <template #body>
            {{ message }}
        </template>
        <template #footer>
            <div class="flex justify-end gap-4 w-full">
                <UButton
                    :ui="{
                        base: 'justify-center',
                    }"
                    class="min-w-20"
                    color="neutral"
                    label="Nein"
                    size="lg"
                    variant="subtle"
                    @click="$emit('close', false)"
                />
                <UButton
                    :color="color"
                    :ui="{
                        base: 'justify-center',
                    }"
                    class="min-w-20"
                    label="Ja"
                    size="lg"
                    @click="$emit('close', true)"
                />
            </div>
        </template>
    </UModal>
</template>

<style scoped></style>
