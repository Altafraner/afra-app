<script lang="ts" setup>
import Login from '@/components/Login.vue';
import { Image } from 'primevue';
import { computed } from 'vue';
import { isDark } from '@/helpers/isdark';
import wappenDark from '/vdaa/favicon-dark.svg';
import wappenLight from '/vdaa/favicon.svg';

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
</script>

<template>
    <div class="mt-20 flex items-center justify-center">
        <div>
            <div class="flex justify-center">
                <Image :src="logo" alt="Logo des Verein der Altafraner" height="200"></Image>
            </div>
            <h1>Willkommen bei der Afra-App</h1>
            <p>Bitte logge dich ein, um die Afra-App zu nutzen.</p>
            <Login></Login>
        </div>
    </div>
</template>

<style scoped></style>
