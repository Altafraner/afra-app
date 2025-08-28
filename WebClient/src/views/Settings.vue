<script setup>
import { Button, useToast } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user.js';

const loading = ref(false);
const user = useUser();
const toast = useToast();
const calLink = ref(null);

const baseUrl = import.meta.env.BASE_URL;

async function fetchKey() {
    loading.value = true;
    const dataGetter = mande('/api/calendar');
    try {
        const response = await dataGetter.get();
        calLink.value = response;
    } catch (e) {
        await user.update();
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden des Kalender-Links aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

async function deleteKeys() {
    loading.value = true;
    const dataGetter = mande('/api/calendar');
    try {
        const response = await dataGetter.delete();
        calLink.value = null;
        toast.add({
            severity: 'success',
            summary: 'Löschung erfolgreich',
            detail: 'Alle deine Kalender-Links wurden erfolgreich gelöscht.',
            life: 2000,
        });
    } catch (e) {
        await user.update();
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Löschen der Kalender-Links aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

const copy = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        toast.add({
            severity: 'success',
            summary: 'Kopiert',
            detail: 'Der Link wurde in die Zwischenablage kopiert.',
            life: 2000,
        });
    } catch {
        toast.add({ severity: 'error', summary: 'Fehler beim Kopieren', life: 2000 });
    }
};
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Einstellungen</h1>

    <h2>Kalender abonnieren</h2>

    <p v-if="user.isStudent">
        Hier kannst du deine Otia-Einschreibungen in einem externen Kalender-Programm anzeigen
        lassen.
    </p>
    <p v-else>
        Hier kannst du von dir betreute Otia-Termine in einem externen Kalender-Programm
        anzeigen lassen.
    </p>

    <p>
        Generiere einen Link und füge ihn in ein solches Programm als Kalender-Abbonement ein.
        Solltest du den Link verlieren oder er aufhören zu funktionieren, kannst du beliebig oft
        einen neuen erstellen.
    </p>

    <span class="inline-flex gap-1">
        <Button
            label="Kalender-Link erstellen"
            :loading="loading"
            @click="fetchKey"
            class="p-button-primary"
        />

        <Button
            label="Alle erstellten Kalender-Links löschen"
            severity="danger"
            :loading="loading"
            @click="deleteKeys"
            class="p-button-primary"
        />
    </span>

    <div v-if="calLink" class="key-display">
        <h3>Dein persönlicher Link:</h3>

        <p>Klicke auf den Link, um ihn zu kopieren.</p>

        <p>Dieser Link ist ein Passwort. Teile ihn nicht mit Dritten.</p>

        <Button
            :label="`${baseUrl}api/calendar/${calLink}.ics`"
            variant="text"
            @click.prevent="copy(`${baseUrl}api/calendar/${calLink}.ics`)"
        />
    </div>
</template>

<style scoped>
.key-display {
    margin-top: 1rem;
    padding: 1rem;
    background: #f4f4f4;
    border-radius: 6px;
}
</style>
