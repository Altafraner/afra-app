<script lang="ts" setup>
const props = withDefaults(
    defineProps<{
        modelValue?: string | null;
        editable?: boolean;
        placeholder?: string;
        maxlength?: number;
    }>(),
    { modelValue: '', editable: true, placeholder: 'Beschreibung eingeben...' },
);
const emit = defineEmits<{ 'update:modelValue': [value: string] }>();

function onUpdate(value: string | null) {
    const next = value ?? '';
    emit('update:modelValue', props.maxlength ? next.slice(0, props.maxlength) : next);
}

const toolbarItems = [
    [
        { kind: 'mark', mark: 'bold', icon: 'i-lucide-bold', tooltip: { text: 'Fett' } },
        { kind: 'mark', mark: 'italic', icon: 'i-lucide-italic', tooltip: { text: 'Kursiv' } },
    ],
    [
        { kind: 'heading', level: 2, icon: 'i-lucide-heading-2', tooltip: { text: 'Ü2' } },
        { kind: 'heading', level: 3, icon: 'i-lucide-heading-3', tooltip: { text: 'Ü3' } },
        { kind: 'heading', level: 4, icon: 'i-lucide-heading-3', tooltip: { text: 'Ü4' } },
    ],
    [
        { kind: 'bulletList', icon: 'i-lucide-list', tooltip: { text: 'Liste' } },
        { kind: 'orderedList', icon: 'i-lucide-list-ordered', tooltip: { text: 'Aufzählung' } },
        { kind: 'blockquote', icon: 'i-lucide-text-quote', tooltip: { text: 'Zitat' } },
    ],
    [
        { kind: 'undo', icon: 'i-lucide-undo', tooltip: { text: 'Rückgängig' } },
        { kind: 'redo', icon: 'i-lucide-redo', tooltip: { text: 'Wiederholen' } },
    ],
];
</script>

<template>
    <template v-if="!editable">
        <UEditor
            v-if="modelValue"
            :model-value="modelValue"
            content-type="markdown"
            :editable="false"
            class="w-full"
        />
        <span v-else>–</span>
    </template>
    <UEditor
        v-else
        v-slot="{ editor }"
        :model-value="modelValue"
        content-type="markdown"
        :placeholder="placeholder"
        class="w-full border border-muted rounded-md min-h-48 overflow-hidden"
        @update:model-value="onUpdate"
    >
        <UEditorToolbar
            :editor="editor"
            :items="toolbarItems"
            class="border-b border-muted py-1.5 px-3 bg-muted/30"
        />
        <UEditorDragHandle :editor="editor" />
        <div v-if="maxlength" class="text-xs text-muted text-right px-2 py-1">
            {{ (modelValue ?? '').length }} / {{ maxlength }}
        </div>
    </UEditor>
</template>
