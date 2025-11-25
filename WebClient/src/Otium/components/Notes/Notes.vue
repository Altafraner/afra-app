<script lang="ts" setup>
import { inject, ref } from 'vue';
import NoteElement from './Note.vue';
import { Button, FloatLabel, InputGroup, Message, Textarea, useToast } from 'primevue';
import type { Note, NoteCreationRequest } from '@/Otium/models/note';
import { mande } from 'mande';
import { useUser } from '@/stores/user';

const dialogRef = inject<{
    value: {
        data: {
            notes: Note[];
            myNote: Note;
            blockId: string;
        };
    };
}>('dialogRef');

const emit = defineEmits<{
    update: any;
}>();

const user = useUser();
const toast = useToast();

const currentNote = ref<string>(dialogRef.value.data.myNote?.content ?? '');
const disabled = ref<boolean>(false);

async function save() {
    disabled.value = true;
    const api = mande('/api/otium/notes');
    const request: NoteCreationRequest = {
        content: currentNote.value,
        blockId: dialogRef.value.data.blockId,
        studentId: user.user.id,
    };
    try {
        await api.put(request);
        toast.add({
            severity: 'success',
            summary: 'Notiz gespeichert',
            life: 10000,
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail:
                'Die Notiz konnte nicht gespeichert werden\nFehlercode: ' +
                error.response?.status,
        });
    } finally {
        disabled.value = false;
        emit('update');
    }
}
</script>

<template>
    <div class="flex flex-col gap-4">
        <NoteElement v-for="note in dialogRef.data.notes" :key="note.id" :note="note" />
        <span v-if="dialogRef.data.notes.length === 0" class="text-center"
            >Bisher gibt es keine Notizen von anderen.</span
        >
    </div>
    <div class="flex flex-col gap-4 mt-6">
        <InputGroup>
            <FloatLabel variant="on">
                <Textarea id="note" v-model="currentNote" autoResize fluid rows="2" />
                <label for="note">Deine Notiz</label>
            </FloatLabel>
            <Button
                :disabled="disabled"
                aria-label="Notiz Hinzufügen"
                icon="pi pi-angle-right"
                @click="save"
            />
        </InputGroup>
        <Message class="mx-1 -mt-2" severity="secondary" variant="simple">
            Notizen sind jeweils für die betroffenen Schüler:innen sowie die aufsichtsführenden
            Lehrer:innen sichtbar.
        </Message>
    </div>
</template>
