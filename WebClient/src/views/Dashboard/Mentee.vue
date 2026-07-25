<script setup>
import { computed, ref } from 'vue';
import { formatStudent } from '@/helpers/formatters';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import StudentEventOverview from '@/components/Dashboard/StudentEventOverview.vue';
import { usePeople } from '@/stores/people.ts';

const props = defineProps({
    studentId: String,
});

const people = usePeople();
await people.updatePersonen();
const mentee = computed(() => {
    return people.personen.find((s) => s.id === props.studentId);
});

const username = computed(() => {
    if (mentee.value) {
        return formatStudent(mentee.value);
    } else {
        return 'Mentee';
    }
});

const navItems = ref([
    {
        label: 'Mentees',
    },
    {
        label: username,
    },
]);
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <div class="flex flex-col gap-4">
        <h1>{{ username }}</h1>
        <StudentEventOverview :studentId="props.studentId" scope="student" />
        <UCard title="Profundum">
            <template #footer>
                <UButton
                    :to="{
                        name: 'Profundum-Feedback-Einsicht-Student',
                        props: { studentId: props.studentId },
                    }"
                    color="neutral"
                    label="Feedback Einsehen"
                    variant="subtle"
                ></UButton>
            </template>
        </UCard>
    </div>
</template>

<style scoped></style>
