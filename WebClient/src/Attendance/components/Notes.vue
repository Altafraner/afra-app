<script lang="ts" setup>
import { computed, ComputedRef, isRef, Ref, ref, toRef, unref } from 'vue';
import NoteElement from './Note.vue';
import type { Note, NoteCreationRequest } from '@/Attendance/models/note';
import { mande, MandeError } from 'mande';

const props = defineProps<{
    notes: Note[] | ComputedRef<Note[]>;
    myNote?: Note | ComputedRef<Note | null>;
    scope: string;
    slotId: string;
    studentId: string;
    updateSelf?: boolean;
}>();

const toast = useToast();

const currentNote = ref<string>(
    isRef(props.myNote)
        ? ((props.myNote as ComputedRef<Note | null>).value?.content ?? '')
        : (props.myNote?.content ?? ''),
);
const disabled = ref<boolean>(false);
const currentNotes: Ref<Note[]> = isRef(props.notes)
    ? toRef(props.notes)
    : ref<Note[]>(props.notes as Note[]);
const effectiveNotes = computed(() =>
    (props.updateSelf ?? false) ? currentNotes.value : unref(props.notes),
);

async function save() {
    disabled.value = true;
    const api = mande('/api/attendance/notes');
    const request: NoteCreationRequest = {
        content: currentNote.value,
        slotId: props.slotId,
        scope: props.scope,
        studentId: props.studentId,
    };
    try {
        const result = await api.put<Note[]>(request);
        toast.add({
            color: 'success',
            title: 'Notiz gespeichert',
            duration: 10000,
        });
        currentNotes.value = result;
    } catch (error) {
        const mandeError = error as MandeError;
        toast.add({
            color: 'error',
            title: 'Fehler',
            description:
                'Die Notiz konnte nicht gespeichert werden\nFehlercode: ' +
                mandeError.response?.status,
        });
    } finally {
        disabled.value = false;
    }
}
</script>

<template>
    <UModal title="Notizen">
        <template #footer>
            <span class="text-muted text-sm">
                Notizen sind jeweils für die betroffenen Schüler:innen sowie die
                aufsichtsführenden Lehrer:innen sichtbar.
            </span>
        </template>
        <template #body>
            <div class="flex flex-col gap-4">
                <NoteElement v-for="note in effectiveNotes" :key="note.id" :note="note" />
                <span v-if="effectiveNotes.length === 0" class="text-center"
                    >Bisher gibt es keine Notizen.</span
                >
            </div>
            <div class="flex flex-col gap-4 mt-6">
                <UFormField class="w-full" label="Deine Notiz">
                    <UFieldGroup class="w-full">
                        <UTextarea
                            v-model="currentNote"
                            :rows="2"
                            :ui="{
                                root: 'w-full',
                                base: 'rounded-r-none',
                            }"
                            autoresize
                            highlight
                            placeholder="Notiz eingeben"
                            variant="subtle"
                        />
                        <UButton
                            :disabled="disabled"
                            aria-label="Notiz Hinzufügen"
                            icon="i-lucide-chevron-right"
                            @click="save"
                        />
                    </UFieldGroup>
                </UFormField>
            </div>
        </template>
    </UModal>
</template>
