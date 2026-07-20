<script lang="ts" setup>
import { convertMarkdownToHtml } from '@/composables/markdown';
import type { Note } from '@/Attendance/models/note';
import { formatDateTime, formatPerson } from '@/helpers/formatters';

defineProps<{
    note: Note;
}>();
</script>

<template>
    <UCard
        :ui="{
            body: 'sm:px-4 sm:py-3 px-2 py-2',
            footer: 'sm:px-4 px-2 py-3',
            header: 'sm:px-4 px-2 py-3',
        }"
        variant="subtle"
    >
        <template #default>
            <div class="m-trim" v-html="convertMarkdownToHtml(note.content)" />
        </template>
        <template #footer>
            <div class="flex flex-col justify-between flex-wrap text-sm gap-1">
                <div class="text-primary font-medium">{{ formatPerson(note.creator) }}</div>
                <div class="text-muted">
                    Erstellt: {{ formatDateTime(new Date(note.created)) }}
                    <template v-if="note.created !== note.changed">
                        <br />
                        Geändert: {{ formatDateTime(new Date(note.changed)) }}
                    </template>
                </div>
            </div>
        </template>
    </UCard>
</template>

<style scoped></style>
