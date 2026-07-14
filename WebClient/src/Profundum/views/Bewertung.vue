<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { useManagement } from '@/Profundum/composables/verwaltung';
import { computed, ref, watch } from 'vue';
import { formatSlot, formatStudent } from '@/helpers/formatters';
import type { UserInfoMinimal } from '@/models/user/user';
import type { AnkerOverview } from '../models/feedback';
import { convertMarkdownToHtml } from '@/composables/markdown';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
        to: {
            name: 'Profundum-Feedback-Abgeben',
        },
    },
];

const verwaltung = useManagement();
const feedback = useFeedback();
const toast = useToast();

const quartal = ref<string>();
const profundum = ref<string>();
const student = ref<string>();
const selectedStudent = ref<UserInfoMinimal | undefined>();
const anker = ref<AnkerOverview | undefined>();

const currentBewertung = ref<Record<string, number | null>>();

const quartale = await verwaltung.getAllQuartaleWithEnrollments();
console.log(quartale);
const profunda = computed(() => {
    if (!quartale || !quartal.value) return [];
    return quartale.find((q) => q.slot.id == quartal.value)?.profunda ?? [];
});
const students = computed(() => {
    if (!quartale || !quartal.value || !profundum.value) return [];
    return (
        profunda.value
            .find((p) => p.id == profundum.value)
            ?.students.map((s) => ({
                label: formatStudent(s),
                original: s,
                value: s.id,
            })) ?? []
    );
});

async function selectStudent() {
    selectedStudent.value = students.value.find((s) => s.value === student.value)?.original;
    currentBewertung.value = await feedback.getBewertung(
        student.value!,
        profundum.value!,
        quartal.value!,
    );
}

watch(profundum, async (value) => {
    if (!value) return;
    anker.value = await feedback.getAnkerForProfundum(value);
});

function cleanup() {
    selectedStudent.value = undefined;
    currentBewertung.value = undefined;
}

watch(student, cleanup);
watch(profundum, () => {
    student.value = undefined;
    cleanup();
});
watch(quartal, () => {
    profundum.value = undefined;
    student.value = undefined;
    cleanup();
});

const warn = computed<boolean>(() => {
    const usedAnker: string[] = [];
    for (const ankerId in currentBewertung.value) {
        if (currentBewertung.value[ankerId] != null) usedAnker.push(ankerId);
    }
    const categories = new Set();
    for (const catId in anker.value?.ankerByKategorie ?? []) {
        if (anker.value?.ankerByKategorie[catId].some((a) => usedAnker.includes(a.id))) {
            categories.add(catId);
        }
    }
    return categories.size < 3;
});

async function save() {
    if (
        !currentBewertung.value ||
        !anker.value ||
        !selectedStudent.value ||
        !profundum.value ||
        !quartal.value
    )
        return;
    await feedback.bewertungAbgeben(
        selectedStudent.value.id,
        profundum.value,
        quartal.value,
        currentBewertung.value,
    );
    toast.add({
        title: 'Gespeichert',
        description: 'Das Feedback wurde erfolgreich gespeichert.',
        color: 'success',
    });
    selectedStudent.value = undefined;
    currentBewertung.value = undefined;
}

const quartaleSelect = computed(() => {
    if (quartale == null) return [];
    return quartale.map((q) => ({
        label: formatSlot(q.slot),
        value: q.slot.id,
    }));
});
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Profundums-Feedback</h1>
    <div class="flex flex-col gap-4">
        <UFormField label="Quartal" required>
            <USelect
                :items="quartaleSelect"
                v-model="quartal"
                class="w-full"
                label-key="label"
                placeholder="Quartal wählen"
                value-key="value"
            />
        </UFormField>
        <UFormField label="Profundum" required>
            <USelect
                :items="profunda"
                v-model="profundum"
                class="w-full"
                label-key="label"
                placeholder="Profundum auswählen"
                value-key="id"
            />
        </UFormField>
        <UFormField label="Schüler:in" required>
            <USelect
                :items="students"
                v-model="student"
                class="w-full"
                label-key="label"
                placeholder="Schüler:in wählen"
                value-key="value"
            />
        </UFormField>
        <UButton
            :ui="{
                base: 'justify-center',
            }"
            class="mt-3"
            label="Laden"
            size="lg"
            @click="selectStudent"
        />
    </div>
    <template v-if="currentBewertung && anker && selectedStudent">
        <h2 class="mt-8">{{ formatStudent(selectedStudent) }}</h2>
        <div class="flex gap-4 flex-col">
            <UCard v-for="kategorie in anker.kategorien" variant="subtle">
                <template #header>
                    <div class="grid grid-cols-[1fr_repeat(5,4rem)] align-baseline gap-x-1">
                        <span class="text-highlighted font-semibold"
                            ><template v-if="kategorie.isFachlich"
                                >Fachliche Kompetenz – </template
                            >{{ kategorie.label }}</span
                        >
                        <span class="text-xs text-center font-thin">nicht ausgeprägt</span>
                        <span class="text-xs text-center font-thin">wenig ausgeprägt</span>
                        <span class="text-xs text-center font-thin">deutlich ausgeprägt</span>
                        <span class="text-xs text-center font-thin"
                            >hervorragend ausgeprägt</span
                        >
                    </div>
                </template>
                <template #default>
                    <div class="grid grid-cols-[1fr_repeat(5,4rem)] gap-y-2 gap-x-1">
                        <template v-for="currentAnker in anker.ankerByKategorie[kategorie.id]">
                            <span class="flex items-center">
                                <span
                                    class=""
                                    v-html="convertMarkdownToHtml(currentAnker.label, true)"
                                />
                            </span>
                            <UButton
                                v-for="i in [1, 2, 3, 4]"
                                :variant="
                                    currentBewertung[currentAnker.id] == i ? 'solid' : 'outline'
                                "
                                class="min-h-10"
                                color="success"
                                @click="
                                    () => {
                                        if (currentBewertung)
                                            currentBewertung[currentAnker.id] = i;
                                    }
                                "
                            />
                            <UButton
                                :variant="
                                    currentBewertung[currentAnker.id] == null
                                        ? 'soft'
                                        : 'outline'
                                "
                                label="N/A"
                                color="neutral"
                                @click="
                                    () => {
                                        if (currentBewertung)
                                            currentBewertung[currentAnker.id] = null;
                                    }
                                "
                            />
                        </template>
                    </div>
                </template>
            </UCard>
        </div>
        <UAlert v-if="warn" class="mt-8" color="warning" title="Nicht genügend Kategorien!">
            <template #description
                >Bitte nutzen Sie Anker aus mindestens
                <span class="font-medium">drei Kategorien.</span></template
            >
        </UAlert>
        <UButton
            :disabled="warn"
            :active="!warn"
            active-variant="solid"
            label="Feedback speichern"
            class="mt-8 w-full"
            color="primary"
            size="xl"
            variant="subtle"
            @click="save"
        />
    </template>
</template>

<style scoped></style>
