<script lang="ts" setup>
import '@/assets/main.css';
import 'primeicons/primeicons.css';

import DynamicDialog from 'primevue/dynamicdialog';
import AfraNav from '@/components/AfraNav.vue';
import { useUser } from '@/stores/user';
import { computed } from 'vue';
import wappenLight from '/vdaa/favicon.svg?url';
import wappenDark from '/vdaa/favicon-dark.svg?url';
import { ConfirmPopup } from 'primevue';
import Login from '@/components/Login.vue';
import { isDark } from '@/helpers/isdark';
import ReloadPrompt from '@/components/ReloadPrompt.vue';
import type { ToasterProps } from '@nuxt/ui/components/Toaster.d.vue.ts';
import { de } from '@nuxt/ui/locale';

const user = useUser();
const toast = useToast();
user.update().catch(() => {
    toast.add({
        color: 'error',
        title: 'Fehler',
        description: 'Ein unerwarteter Fehler ist beim Laden der Nutzerdaten aufgetreten',
    });
});

const logo = computed(() => (isDark().value ? wappenDark : wappenLight));

const toastProps: ToasterProps = {
    position: 'top-right',
    progress: false,
    portal: true,
};
</script>

<template>
    <ConfirmPopup />
    <DynamicDialog />
    <div v-if="user.isImpersonating" aria-hidden="true" class="impersonation-tag hidden"></div>
    <UApp :locale="de" :toaster="toastProps">
        <ReloadPrompt />
        <template v-if="!user.loading">
            <afra-nav v-if="user.loggedIn" />
            <main class="flex justify-center min-h-[90vh] mt-4">
                <UContainer v-if="user.loggedIn">
                    <RouterView v-slot="{ Component }">
                        <template v-if="Component">
                            <Suspense>
                                <component :is="Component" />
                                <template #fallback>
                                    <div>
                                        <USkeleton class="h-12 w-[60%]" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-[80%] my-2" />
                                        <USkeleton class="h-8 w-[65%] mb-4 mt-12" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-[30%] my-2" />
                                        <USkeleton class="h-4 w-full mb-2 mt-4" />
                                        <USkeleton class="h-4 w-full my-2" />
                                        <USkeleton class="h-4 w-[40%] my-2" />
                                    </div>
                                </template>
                            </Suspense>
                        </template>
                    </RouterView>
                </UContainer>
                <div v-else class="min-container">
                    <div class="flex justify-center">
                        <img
                            :src="logo"
                            alt="Logo des Verein der Altafraner"
                            height="200"
                        ></img>
                    </div>
                    <h1>Willkommen bei der Afra-App</h1>
                    <p>Bitte logge dich ein, um die Afra-App zu nutzen.</p>
                    <Login></Login>
                </div>
            </main>
        </template>
        <template v-else>
            <USkeleton class="w-full h-[4rem]" />
            <main class="flex justify-center min-h-[90vh] mt-4">
                <UContainer>
                    <h1>
                        <USkeleton class="w-[60%] h-[3rem]" />
                    </h1>
                    <p class="flex gap-2 flex-col">
                        <USkeleton class="w-full h-[1rem]" />
                        <USkeleton class="w-full h-[1rem]" />
                        <USkeleton class="w-[60%] h-[1rem]" />
                    </p>
                    <p class="flex gap-2 flex-col mt-2">
                        <USkeleton class="w-full h-[1rem]" />
                        <USkeleton class="w-full h-[1rem]" />
                        <USkeleton class="w-full h-[1rem]" />
                        <USkeleton class="w-[30%] h-[1rem]" />
                    </p>
                </UContainer>
            </main>
        </template>
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
            <span class="text-right flex justify-end">
                <a
                    aria-label="GitHub"
                    class="w-auto hover:text-highlighted"
                    href="https://github.com/Altafraner/afra-app"
                    target="_blank"
                    ><svg
                        height="1em"
                        role="img"
                        viewBox="0 0 24 24"
                        xmlns="http://www.w3.org/2000/svg"
                    >
                        <title>GitHub</title>
                        <path
                            d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"
                            fill="currentColor"
                        /></svg
                ></a>
            </span>
        </footer>
    </UApp>
</template>

<style scoped>
.min-container {
    max-width: min(95%, 50rem);
    margin-top: 5rem;
}
</style>
