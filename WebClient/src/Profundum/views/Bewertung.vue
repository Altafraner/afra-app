<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { useManagement } from '@/Profundum/composables/verwaltung';
import { computed, ref, watch } from 'vue';
import { Button, Card, FloatLabel, Message, Select, useToast } from 'primevue';
import { formatSlot, formatStudent } from '@/helpers/formatters';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import type { AnkerOverview } from '../models/feedback';
import { convertMarkdownToHtml } from '@/composables/markdown';

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
const students = computed<UserInfoMinimal[]>(() => {
    if (!quartale || !quartal.value || !profundum.value) return [];
    return profunda.value.find((p) => p.id == profundum.value)?.students ?? [];
});

async function selectStudent() {
    selectedStudent.value = students.value.find((s) => s.id === student.value);
    currentBewertung.value = await feedback.getBewertung(student.value!, profundum.value!);
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
watch(profundum, cleanup);
watch(quartal, cleanup);

const warn = computed<boolean>(() => {
    const usedAnker: string[] = [];
    for (const ankerId in currentBewertung.value) {
        if (currentBewertung.value[ankerId] != null) usedAnker.push(ankerId);
    }
    const categories = new Set();
    for (const catId in anker.value.ankerByKategorie) {
        if (anker.value.ankerByKategorie[catId].some((a) => usedAnker.includes(a.id))) {
            categories.add(catId);
        }
    }
    return categories.size < 3;
});

async function save() {
    if (!currentBewertung.value || !anker.value || !selectedStudent.value || !profundum.value)
        return;
    await feedback.bewertungAbgeben(
        selectedStudent.value.id,
        profundum.value,
        currentBewertung.value,
    );
    toast.add({
        summary: 'Gespeichert',
        detail: 'Das Feedback wurde erfolgreich gespeichert.',
        life: 1000 * 3,
        severity: 'success',
    });
    selectedStudent.value = undefined;
    currentBewertung.value = undefined;
}
</script>

<template>
    <h1>Profundums-Feedback</h1>
    <div class="flex flex-col gap-4">
        <FloatLabel variant="on">
            <Select
                id="quartal"
                v-model="quartal"
                :options="quartale ?? undefined"
                fluid
                option-value="slot.id"
            >
                <template #option="{ option }">
                    {{ formatSlot(option.slot) }}
                </template>
                <template #value="{ value }"
                    ><template v-if="value">{{
                        formatSlot(quartale.find((slot) => slot.slot.id == value).slot)
                    }}</template></template
                >
            </Select>
            <label for="quartal">Quartal</label>
        </FloatLabel>
        <FloatLabel variant="on">
            <Select
                id="profundum"
                v-model="profundum"
                :options="profunda"
                fluid
                option-label="label"
                option-value="id"
            />
            <label for="profundum">Profundum</label>
        </FloatLabel>
        <FloatLabel variant="on">
            <Select
                id="student"
                v-model="student"
                :option-label="formatStudent"
                :options="students"
                fluid
                option-value="id"
            />
            <label for="student">Student</label>
        </FloatLabel>
        <Button fluid label="Laden" @click="selectStudent" />
    </div>
    <template v-if="currentBewertung && anker && selectedStudent">
        <h2 class="mt-8">{{ formatStudent(selectedStudent) }}</h2>
        <div class="flex gap-4 flex-col">
            <Card v-for="kategorie in anker.kategorien">
                <template #title>
                    <div class="grid grid-cols-[1fr_repeat(5,4rem)] align-baseline gap-x-1">
                        <span
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
                <template #content>
                    <div
                        class="grid grid-cols-[1fr_repeat(5,4rem)] align-baseline gap-y-2 gap-x-1"
                    >
                        <template v-for="currentAnker in anker.ankerByKategorie[kategorie.id]">
                            <span v-html="convertMarkdownToHtml(currentAnker.label, true)" />
                            <Button
                                v-for="i in [1, 2, 3, 4]"
                                :variant="
                                    currentBewertung[currentAnker.id] == i
                                        ? undefined
                                        : 'outlined'
                                "
                                severity="success"
                                @click="() => (currentBewertung[currentAnker.id] = i)"
                            />
                            <Button
                                :variant="
                                    currentBewertung[currentAnker.id] == null
                                        ? undefined
                                        : 'outlined'
                                "
                                label="N/A"
                                severity="secondary"
                                @click="() => (currentBewertung[currentAnker.id] = null)"
                            />
                        </template>
                    </div>
                </template>
            </Card>
        </div>
        <Message v-if="warn" class="mt-8" severity="warn"
            >Bitte nutzen Sie Anker aus mindestens <strong>drei Kategorien.</strong></Message
        >
        <Button
            :disabled="warn"
            class="mt-8"
            fluid
            label="Feedback speichern"
            variant="success"
            @click="save"
        />
    </template>
</template>

<style scoped></style>
