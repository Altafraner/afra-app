<script lang="ts" setup>
import { convertMarkdownToHtml } from '@/composables/markdown';
import type { Note } from '@/Attendance/models/note';
import { formatCalendarDateTime, formatPerson } from '@/helpers/formatters';
import { getLocalTimeZone, parseAbsolute } from '@internationalized/date';

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
            <div class="m-trim prosa" v-html="convertMarkdownToHtml(note.content)" />
        </template>
        <template #footer>
            <div class="flex flex-col justify-between flex-wrap text-sm gap-1">
                <div class="text-primary font-medium">{{ formatPerson(note.creator) }}</div>
                <div class="text-muted">
                    Erstellt:
                    {{
                        formatCalendarDateTime(parseAbsolute(note.created, getLocalTimeZone()))
                    }}
                    <template v-if="note.created !== note.changed && note.changed">
                        <br />
                        Geändert:
                        {{
                            formatCalendarDateTime(
                                parseAbsolute(note.changed, getLocalTimeZone()),
                            )
                        }}
                    </template>
                </div>
            </div>
        </template>
    </UCard>
</template>

<style scoped></style>
