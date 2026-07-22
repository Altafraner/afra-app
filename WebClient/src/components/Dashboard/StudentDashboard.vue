<script lang="ts" setup>
import { useUser } from '@/stores/user.ts';
import StudentEventOverview from '@/components/Dashboard/StudentEventOverview.vue';

const user = useUser();
</script>

<template>
    <div class="flex flex-col gap-4 mt-2">
        <h1>Hallo, {{ user.user?.vorname ?? 'Nutzer' }}!</h1>
        <StudentEventOverview scope="self" />
    </div>
</template>

<style scoped></style>
