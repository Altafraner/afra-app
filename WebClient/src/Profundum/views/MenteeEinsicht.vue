<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { computed, shallowRef } from 'vue';
import type { MenteeFeedback } from '@/Profundum/models/feedback';
import { formatStudent } from '@/helpers/formatters';
import FeedbackDisplay from '@/Profundum/components/FeedbackDisplay.vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import type { NavBreadcrumbElement } from '@/models/navBreadcrumb';

const props = defineProps<{
    studentId: string;
}>();

const feedback = useFeedback();

const data = shallowRef<MenteeFeedback>(await feedback.getDisclosureFor(props.studentId));

const navItems = computed<NavBreadcrumbElement[]>(() => [
    {
        label: 'Mentees',
    },
    {
        label: formatStudent(data.value.person),
        route: {
            name: 'Mentee',
            props: {
                studentId: props.studentId,
            },
        },
    },
    {
        label: 'Feedback',
    },
]);
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Feedback für {{ formatStudent(data.person) }}</h1>
    <FeedbackDisplay :value="data.feedback" />
</template>

<style scoped></style>
