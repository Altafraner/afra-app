<script lang="ts" setup>
import { useFeedback } from '@/Profundum/composables/feedback';
import { shallowRef } from 'vue';
import type { StudentFeedbackHierarchie } from '@/Profundum/models/feedback';
import FeedbackDisplay from '@/Profundum/components/FeedbackDisplay.vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import type { NavBreadcrumbElement } from '@/models/navBreadcrumb';

const feedbackService = useFeedback();

const data = shallowRef<StudentFeedbackHierarchie>(await feedbackService.getSelfDisclosure());
const navItems: NavBreadcrumbElement[] = [
    {
        label: 'Profundum',
    },
    {
        label: 'Feedback',
    },
];
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Feedback Selbsteinsicht</h1>

    <FeedbackDisplay :value="data" />
</template>

<style scoped></style>
