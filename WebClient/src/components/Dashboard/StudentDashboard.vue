<script lang="ts" setup>
import { useUser } from '@/stores/user.ts';
import StudentEventOverview from '@/components/Dashboard/StudentEventOverview.vue';
import StudentProfundaOverview from '@/Profundum/components/StudentProfundaOverview.vue';

const user = useUser();
</script>

<template>
    <div class="flex flex-col gap-4 mt-2">
        <h1>Hallo, {{ user.user?.vorname ?? 'Nutzer' }}!</h1>
        <StudentEventOverview scope="self" />
        <StudentProfundaOverview v-if="user.isMittelstufe" />
    </div>
</template>

<style scoped></style>
