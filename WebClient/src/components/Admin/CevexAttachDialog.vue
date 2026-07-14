<script lang="ts" setup>
import { ref } from 'vue';
import type { CevexEntity } from '@/models/admin/cevex';
import type { UserInfoMinimal } from '@/models/user/user';
import { formatStudent } from '@/helpers/formatters';

const props = defineProps<{ options: CevexEntity[]; student: UserInfoMinimal }>();
defineEmits<{ close: [CevexEntity | undefined] }>();
const selectedId = ref<string | undefined>();
const mappedOptions = props.options.map((option) => ({
    value: option.id,
    label: `${option.firstName} ${option.lastName}`,
}));
</script>

<template>
    <UModal title="Cevex-Schüler:in zuweisen">
        <template #description>
            Bitte wählen Sie die Cevex-Schüler:in aus, die der Nutzer:in
            <strong class="inline-block">{{ formatStudent(student) }}</strong>
            entspricht.
        </template>
        <template #body>
            <UFormField label="Cevex-Schüler:in">
                <USelectMenu
                    v-model="selectedId"
                    :items="mappedOptions"
                    class="w-full"
                    placeholder="Cevex Schüler:in auswählen"
                    value-key="value"
                />
            </UFormField>
        </template>
        <template #footer>
            <div class="flex flex-row gap-4 w-full">
                <UButton
                    color="neutral"
                    label="Abbrechen"
                    variant="soft"
                    @click="$emit('close', undefined)"
                />
                <UButton
                    :disabled="!selectedId"
                    class="w-full"
                    color="primary"
                    icon="i-lucide-arrow-right"
                    label="Zuweisen"
                    @click="
                        $emit('close', options.find((o) => o.id == selectedId) ?? undefined)
                    "
                />
            </div>
        </template>
    </UModal>
</template>

<style scoped></style>
