<script setup>
import { Button, useToast } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user.js';

const loading = ref(false);
const user = useUser();
const toast = useToast();
const calLink = ref(null);

async function fetchKey() {
    loading.value = true;
    const dataGetter = mande('/api/calendar/subscribe');
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
    <h1>Kalender abonnieren</h1>

    <p v-if="user.isStudent">
        Hier kannst du deine Otia-Einschreibungen in einem externen Kalender-Programm anzeigen
        lassen.
    </p>
    <p v-else>
        Hier kannst du von dir betreute Otia-Termine in einem externen Kalender-Programm
        anzeigen lassen.
    </p>

    <p>Generiere einen Link und füge ihn in solchem als Kalender-Abbonement ein.</p>

    <Button
        label="Kalender-Link erstellen"
        :loading="loading"
        @click="fetchKey"
        class="p-button-primary"
    />

    <div v-if="calLink" class="key-display">
        <h3>Dein persönlicher Link (anklicken um ihn zu kopieren):</h3>
        <p>Dieser Link ist ein Passwort. Teile ihn nicht mit Dritten.</p>
        <Button
            :label="`https://afra.altafraner.de/api/calendar/${calLink}.ics`"
            variant="text"
            @click.prevent="copy(`https://afra.altafraner.de/api/calendar/${calLink}.ics`)"
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
