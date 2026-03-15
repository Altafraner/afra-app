<script setup>
import '@/assets/main.css';
import 'primeicons/primeicons.css';

import DynamicDialog from 'primevue/dynamicdialog';
import AfraNav from '@/components/AfraNav.vue';
import { useUser } from '@/stores/user';
import { useUserManagement } from '@/composables/userManagement.ts';
import { ConfirmPopup, Toast, useToast } from 'primevue';
import ReloadPrompt from '@/components/ReloadPrompt.vue';
import { useRoute } from 'vue-router';
import SkeletonView from '@/components/SkeletonView.vue';

const userManagement = useUserManagement();
const route = useRoute();
const user = useUser();
const toast = useToast();
userManagement.updateUser().catch(() => {
    toast.add({
        severity: 'error',
        summary: 'Fehler',
        detail: 'Ein unerwarteter Fehler ist beim Laden der Nutzerdaten aufgetreten',
    });
});
</script>

<template>
    <Toast />
    <ConfirmPopup />
    <DynamicDialog />
    <ReloadPrompt />
    <div v-if="user.isImpersonating" aria-hidden="true" class="impersonation-tag hidden"></div>
    <template v-if="!user.loading">
        <afra-nav v-if="user.loggedIn" />
        <main class="flex justify-center min-h-[90vh] mt-4">
            <div :class="(route.meta.fullWidth ?? false) ? 'w-19/20' : 'container'">
                <RouterView v-slot="{ Component }">
                    <template v-if="Component">
                        <Suspense>
                            <component :is="Component" />
                            <template #fallback>
                                <SkeletonView />
                            </template>
                        </Suspense>
                    </template>
                </RouterView>
            </div>
        </main>
    </template>
    <SkeletonView v-else />
    <footer
        class="bg-primary dark:bg-blue-950 w-full py-6 px-8 mt-[1rem] text-center text-primary-contrast sm:grid sm:grid-cols-[1fr_auto_1fr] items-center gap-3 flex flex-wrap justify-between"
    >
        <span></span>
        <p class="min-h-[1.2em]">
            In Kooperation mit dem
            <a
                class="font-bold inline-block text-primary-contrast underline decoration-primary hover:decoration-primary-contrast transition-all"
                href="https://verein-der-altafraner.de"
                target="_blank"
                >Verein der Altafraner</a
            >
        </p>
        <span class="text-right">
            <a aria-label="GitHub" href="https://github.com/Altafraner/afra-app" target="_blank"
                ><i class="pi pi-github"
            /></a>
        </span>
    </footer>
</template>

<style scoped>
.container {
    width: 50rem;
}

@media screen and (width < 55rem) {
    .container {
        width: 95%;
    }
}
</style>
