<script lang="ts" setup>
import { computed, ref } from 'vue';
import type {
    FeedbackAnkerGroup,
    FeedbackEnrollmentInfo,
    FeedbackKategorieGroup,
    ratingBySlot,
    StudentFeedbackHierarchie,
} from '@/Profundum/models/feedback';
import { convertMarkdownToHtml } from '@/composables/markdown';
import FeedbackRatingDisplay from '@/Profundum/components/FeedbackRatingDisplay.vue';

const props = defineProps<{
    value: StudentFeedbackHierarchie;
}>();

const enrollmentsAvailable = computed(() => {
    const result: FeedbackEnrollmentInfo[] = [];
    for (const slotId in props.value.enrollments) {
        result.push(props.value.enrollments[slotId]);
    }
    result.sort((s1, s2) => {
        if (s1.slot.jahr < s2.slot.jahr) return -1;
        if (s2.slot.jahr < s1.slot.jahr) return 1;
        if (s1.slot.quartal < s2.slot.quartal) return -1;
        if (s2.slot.quartal < s1.slot.quartal) return 1;
        if (s1.profundum < s2.profundum) return -1;
        if (s2.profundum < s1.profundum) return 1;
        return 0;
    });
    return result.map((r) => ({
        label: generateEnrollmentLabel(r),
        value: r.slot.id,
    }));
});

const selectedElements = ref<string[]>([]);

const dataSelected = computed(() => {
    const slotsAvailable = new Set(selectedElements.value);
    if (selectedElements.value.length == 0) {
        return props.value.kategorien;
    }
    const kategorienResults: FeedbackKategorieGroup[] = [];
    for (const kategorie of props.value.kategorien) {
        const ankerResults: FeedbackAnkerGroup[] = [];
        for (const anker of kategorie.anker) {
            const ratingResults: ratingBySlot = {};
            for (const slot in anker.ratingsBySlot) {
                if (slotsAvailable.has(slot)) {
                    ratingResults[slot] = anker.ratingsBySlot[slot];
                }
            }
            if (Object.keys(ratingResults).length > 0)
                ankerResults.push(Object.assign({}, anker, { ratingBySlot: ratingResults }));
        }
        if (ankerResults.length > 0)
            kategorienResults.push(Object.assign({}, kategorie, { anker: ankerResults }));
    }
    return kategorienResults;
});

function generateSlotLabel(slotId: string): string {
    const enrollment = props.value.enrollments[slotId];
    return `(${enrollment.slot.jahr} / ${enrollment.slot.jahr + 1} - ${enrollment.slot.quartal})`;
}

function generateEnrollmentLabel(enrollment: FeedbackEnrollmentInfo): string {
    return `${enrollment.profundum} (${enrollment.slot.jahr} / ${enrollment.slot.jahr + 1} - ${enrollment.slot.quartal})`;
}
</script>

<template>
    <UFormField class="mb-8" label="Filter" size="lg">
        <UFieldGroup class="w-full">
            <USelectMenu
                v-model="selectedElements"
                :items="enrollmentsAvailable"
                class="w-full"
                label-key="label"
                multiple
                placeholder="Profunda wählen"
                value-key="value"
            />
            <UTooltip text="Zurücksetzen">
                <UButton
                    :disabled="!selectedElements || selectedElements.length == 0"
                    aria-label="Zurücksetzen"
                    color="neutral"
                    icon="i-lucide-x"
                    variant="outline"
                    @click="selectedElements = []"
                />
            </UTooltip>
        </UFieldGroup>
    </UFormField>

    <div class="flex flex-col gap-8">
        <UCard v-for="kategorie in dataSelected" :key="kategorie.id">
            <template #title>
                <template v-if="kategorie.isFachlich">Fachliche Kompetenzen – </template
                >{{ kategorie.label }}
            </template>
            <template #default>
                <div class="flex flex-col gap-4">
                    <UCard
                        v-for="anker in kategorie.anker"
                        :key="anker.id"
                        :ui="{
                            body: 'p-2 sm:p-4',
                            header: 'p-2 sm:p-4',
                        }"
                        variant="subtle"
                    >
                        <template #title>
                            <span v-html="convertMarkdownToHtml(anker.label, true)" />
                        </template>
                        <div class="grid grid-cols-[1fr_auto] gap-y-2 gap-x-4 items-center">
                            <template v-for="(rating, slot) in anker.ratingsBySlot">
                                <span>
                                    {{ value.enrollments[slot].profundum }}
                                    <span class="inline-block">
                                        {{ generateSlotLabel(slot.toString()) }}
                                    </span>
                                </span>
                                <FeedbackRatingDisplay :value="rating" />
                            </template>
                        </div>
                    </UCard>
                </div>
            </template>
        </UCard>
        <div v-if="value.kategorien.length == 0" class="text-center w-full">
            Kein Feedback gefunden.
        </div>
    </div>
</template>

<style scoped></style>
