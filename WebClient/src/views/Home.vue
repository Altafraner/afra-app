<script setup>
import { useUser } from '@/stores/user';
import { defineAsyncComponent } from 'vue';
import { useOtiumStore } from '@/Otium/stores/otium.js';

const Student = defineAsyncComponent(() => import('@/Otium/views/Dashboard/Student.vue'));
const TutorDashboard = defineAsyncComponent(
    () => import('@/components/Dashboard/TutorDashboard.vue'),
);

const user = useUser();
const otiumStore = useOtiumStore();
await otiumStore.updateKategorien();
</script>

<template>
    <Student v-if="user.isStudent" />
    <TutorDashboard v-if="user.isTeacher" />
</template>

<style scoped>
p a {
    color: var(--p-primary-500);
}

p a:visited {
    color: var(--p-primary-700);
}
</style>
