<script lang="ts" setup>
import { computed } from 'vue';
import { isDark } from '@/helpers/isdark.ts';
import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import LoginForm from '@/components/Login.vue';
import { useUser } from '@/stores/user.ts';
import { useRouter } from 'vue-router';

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));
const user = useUser();
const router = useRouter();

// if (user.loggedIn) router.replace('/');
</script>

<template>
    <div class="flex justify-center items-center flex-col mt-8">
        <div>
            <div class="flex justify-center">
                <img :src="logo" alt="Logo des Verein der Altafraner" height="200" />
            </div>
            <h1>Willkommen bei der Afra-App</h1>
            <p>Bitte logge dich ein, um die Afra-App zu nutzen.</p>
            <LoginForm></LoginForm>
        </div>
    </div>
</template>

<style scoped></style>
