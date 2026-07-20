<script lang="ts" setup>
import { ref, shallowRef } from 'vue';
import { mande, MandeError } from 'mande';
import { useUser } from '@/stores/user';

const loading = ref(false);
const user = useUser();
const toast = useToast();
const username = shallowRef<string>();
const password = shallowRef<string>();
const remember = shallowRef<boolean>(false);

const submit = async () => {
    if (loading.value) return;
    if (!(username.value && password.value)) return;
    loading.value = true;
    try {
        const loginRequest = await mande('/api/user/login').post(
            {
                username: username.value,
                password: password.value,
                rememberMe: remember.value,
            },
            { responseAs: 'response' },
        );
        await user.update();
    } catch (error) {
        const mandeError = error as MandeError;
        if (mandeError.response.status === 401) {
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: 'Fehlerhafte Anmeldedaten',
                duration: 5000,
            });
        } else if (mandeError.response.status === 429) {
            toast.add({
                color: 'error',
                title: 'Zu viele Anmeldeversuche',
                description: 'Bitte warten Sie 5 Minuten, bevor Sie es erneut versuchen.',
                duration: 5000,
            });
        } else {
            toast.add({
                color: 'error',
                title: 'Fehler',
                description: 'Ein unbekannter Fehler ist aufgetreten',
                duration: 5000,
            });
        }
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <UForm class="flex flex-col gap-6 mt-8" @submit="submit">
        <UFormField class="w-full" label="Nutzername" name="username" required>
            <UInput v-model="username" class="w-full" type="text" />
        </UFormField>
        <UFormField class="w-full" label="Passwort" name="password" required>
            <UInput v-model="password" class="w-full" type="password" />
        </UFormField>
        <UCheckbox
            v-model="remember"
            color="neutral"
            label="Angemeldet bleiben"
            name="remember"
        />
        <UButton
            :loading="loading"
            color="neutral"
            severity="secondary"
            size="xl"
            type="submit"
            variant="soft"
            >Anmelden</UButton
        >
    </UForm>
</template>

<style scoped></style>
