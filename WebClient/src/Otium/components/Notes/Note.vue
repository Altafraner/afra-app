<script lang="ts" setup>
import { Card } from 'primevue';
import { convertMarkdownToHtml } from '@/composables/markdown';
import UserPeek from '@/components/UserPeek.vue';
import { formatDateTime } from '@/helpers/formatters';
import type { Note } from '@/Otium/models/note.ts';

defineProps<{
    note: Note;
}>();
</script>

<template>
    <Card>
        <template #content>
            <div v-html="convertMarkdownToHtml(note.content)" />
        </template>
        <template #footer>
            <div class="flex justify-between flex-wrap">
                <UserPeek :person="note.person" />
                <span
                    >Erstellt am {{ formatDateTime(new Date(note.created))
                    }}<template v-if="note.created !== note.changed"
                        >, Geändert {{ formatDateTime(new Date(note.changed)) }}</template
                    >.</span
                >
            </div>
        </template>
    </Card>
</template>

<style scoped></style>
