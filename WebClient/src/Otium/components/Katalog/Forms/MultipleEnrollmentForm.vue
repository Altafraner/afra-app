<script lang="ts" setup>
import { ref } from 'vue';
import { formatDate } from '@/helpers/formatters';
import ATransferList from '@/components/Form/ATransferList.vue';
import type { ListboxItem } from '@nuxt/ui';

const props = defineProps<{
    options: string[];
}>();

const emit = defineEmits<{
    close: [string[] | undefined];
}>();

const mappedOptions = props.options.map((option) => ({
    label: formatDate(new Date(option), true),
    value: option,
}));

const selection = ref<ListboxItem[]>([]);

function cancel() {
    emit('close', undefined);
}

function submit() {
    emit(
        'close',
        (selection.value as { value: string }[]).map((e) => e.value),
    );
}
</script>

<template>
    <UModal
        description="Gleichzeitiges Einschreiben in mehrere Termine desselben Otiums"
        title="Mehrfach einschreiben"
    >
        <template #body>
            <div class="flex flex-col gap-4">
                <UFormField
                    help="Bitte wähle alle zusätzlichen Termine, zu denen du kommen möchtest."
                    label="Termine"
                >
                    <ATransferList v-model="selection" :items="mappedOptions" />
                </UFormField>
                <div
                    class="grid grid-cols-[1fr] items-stretch justify-stretch sm:flex flex-wrap gap-4"
                >
                    <UButton
                        color="neutral"
                        label="Abbrechen"
                        size="lg"
                        variant="soft"
                        @click="cancel"
                    />
                    <UButton
                        class="flex-1"
                        color="primary"
                        icon="i-lucide-plus"
                        label="Einschreiben"
                        size="lg"
                        @click="submit"
                    />
                </div>
            </div>
        </template>
        <template #footer>
            <p class="text-sm text-muted">
                Ist das Einschreiben in einen ausgewählten Termin für dich nicht erlaubt, wirst
                du in diesen nicht eingeschrieben.
            </p>
        </template>
    </UModal>
</template>

<style scoped></style>
