<script lang="ts" setup>
import { defineAsyncComponent, ref } from 'vue';
import { Button } from 'primevue';

const LegacyLogin = defineAsyncComponent(() => import('@/components/LegacyLogin.vue'));

const legacy = ref(false);
</script>

<template>
    <div v-if="!legacy" class="flex flex-col gap-6 mt-8">
        <Button
            as="a"
            fluid
            href="/api/oidc/start"
            label="Anmelden mit SSO"
            severity="primary"
        />
        <Button
            fluid
            label="Anmelden mit Nutzername und Passwort"
            severity="secondary"
            @click="legacy = true"
        />
    </div>
    <div v-if="legacy" class="flex flex-col gap-6 mt-8">
        <LegacyLogin />
        <Button
            as="a"
            fluid
            href="/api/oidc/start"
            label="Stattdessen Anmelden mit SSO"
            severity="secondary"
        />
    </div>
</template>

<style scoped></style>
