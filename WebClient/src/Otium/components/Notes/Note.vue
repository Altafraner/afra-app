<script lang="ts" setup>
import { Card, Message } from 'primevue';
import { convertMarkdownToHtml } from '@/composables/markdown';
import type { Note } from '@/Otium/models/note';
import { formatDateTime, formatPerson } from '@/helpers/formatters';

defineProps<{
    note: Note;
}>();
</script>

<template>
    <Card
        :pt="{
            body: {
                class: ['p-4'],
            },
            content: {
                class: ['mb-2'],
            },
        }"
        class="dark:bg-surface-800 bg-surface-100 p-0"
    >
        <template #content>
            <div class="m-trim" v-html="convertMarkdownToHtml(note.content)" />
        </template>
        <template #footer>
            <div class="flex justify-between flex-wrap">
                <Message severity="info" size="small" variant="simple">{{
                    formatPerson(note.creator)
                }}</Message>
                <Message severity="secondary" size="small" variant="simple"
                    >Erstellt am {{ formatDateTime(new Date(note.created))
                    }}<template v-if="note.created !== note.changed"
                        >, Geändert {{ formatDateTime(new Date(note.changed)) }}</template
                    >.</Message
                >
            </div>
        </template>
    </Card>
</template>

<style scoped></style>
