<script setup>
import { Form } from '@primevue/forms';
import { Button, Checkbox, FloatLabel, InputText, Password, useToast } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';

const loading = ref(false);
const user = useUser();
const toast = useToast();

const submit = async (evt) => {
    if (loading.value) return;
    const username = evt.states['username'].value;
    const password = evt.states['password'].value;
    const remember = !!evt.states['remember'].value;
    if (!(username && password)) return;
    loading.value = true;
    try {
        const loginRequest = await mande('/api/user/login').post(
            {
                username: username,
                password: password,
                rememberMe: remember,
            },
            { responseAs: 'response' },
        );
        await user.update();
    } catch (error) {
        if (error.response.status === 401) {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Fehlerhafte Anmeldedaten',
                life: 5000,
            });
        } else if (error.response.status === 429) {
            toast.add({
                severity: 'error',
                summary: 'Zu viele Anmeldeversuche',
                detail: 'Bitte warten Sie 5 Minuten, bevor Sie es erneut versuchen.',
                life: 5000,
            });
        } else {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Ein unbekannter Fehler ist aufgetreten',
                life: 5000,
            });
        }
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <Form class="flex flex-col gap-6" @submit="submit">
        <FloatLabel variant="on">
            <InputText id="username" fluid name="username" type="text" />
            <label for="username">Nutzername</label>
        </FloatLabel>
        <FloatLabel variant="on">
            <Password :feedback="false" fluid input-id="password" name="password" toggle-mask />
            <label for="password">Passwort</label>
        </FloatLabel>
        <div class="flex items-center gap-2">
            <Checkbox binary input-id="remember" name="remember" />
            <label class="cursor-pointer font-medium text-surface-500" for="remember"
                >Angemeldet bleiben</label
            >
        </div>
        <Button :loading="loading" fluid label="Einloggen" severity="primary" type="submit" />
    </Form>
</template>

<style scoped></style>
