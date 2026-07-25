<script lang="ts" setup>
import { withQuery } from 'ufo';
import { computed, ref } from 'vue';
import { useUser } from '@/stores/user.ts';
import { useRouter } from 'vue-router';
import { useLogo } from '@/composables/logo.ts';

const user = useUser();
const router = useRouter();
const logo = useLogo();

if (user.loggedIn) router.replace('/');
const isDev = import.meta.env.DEV;

const remember = ref(true);

const loginUrl = computed(() => {
    let redirectUrl = router.currentRoute.value.query['redirectUrl'];
    if (Array.isArray(redirectUrl)) {
        redirectUrl = null;
    } else if (redirectUrl) redirectUrl = decodeURIComponent(redirectUrl);
    return withQuery('/api/oidc/start', {
        staySignedIn: remember.value,
        redirectUrl: !redirectUrl || !sameOrigin(redirectUrl) ? '/' : redirectUrl,
    });
});

function sameOrigin(link: string) {
    try {
        const url = new URL(link, location.origin);
        return url.origin === location.origin;
    } catch (e) {
        return false;
    }
}
</script>

<template>
    <div class="flex justify-center items-center flex-col mt-8">
        <div>
            <div class="flex justify-center">
                <img :src="logo" alt="Logo des Verein der Altafraner" height="200" />
            </div>
            <h1>Willkommen bei der Afra-App</h1>
            <p class="mb-6">Bitte melde dich an, um die Afra-App zu nutzen.</p>
            <UButton
                class="w-full"
                color="neutral"
                external
                icon="i-lucide-arrow-right"
                label="Anmelden"
                :to="loginUrl"
                variant="soft"
            />
            <UCheckbox
                v-model="remember"
                class="mt-4"
                color="secondary"
                label="Angemeldet bleiben"
            />
            <UButton
                v-if="isDev"
                :to="{ name: 'Dev-Login' }"
                class="w-full mt-4"
                color="warning"
                icon="i-lucide-user"
                label="Nutzerauswahl"
                variant="soft"
            />
        </div>
    </div>
</template>

<style scoped></style>
