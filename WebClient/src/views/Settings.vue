<script setup>
import { Button, useToast, RadioButton } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user.js';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const loading = ref(false);
const user = useUser();
const toast = useToast();
const calLink = ref(null);

const numSubs = ref(null);

const greeting = ref(user.user.greeting);

const baseUrl = import.meta.env.BASE_URL;

async function updateGreeting(newGreeting) {
    const dataPoster = mande('/api/user/greeting');
    try {
        await dataPoster.patch({ value: newGreeting });
        await user.update();
        toast.add({
            severity: 'success',
            summary: 'Gespeichert',
            detail: `Deine Begrüßung wurde auf "${newGreeting}" aktualisiert.`,
            life: 2000,
        });
    } catch (e) {
        console.error(e);
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Begrüßung konnte nicht gespeichert werden.',
            life: 2000,
        });
        greeting.value = user.user.greeting;
    }
}

function onGreetingChange(val) {
    updateGreeting(val);
}

async function fetchNum() {
    loading.value = true;
    const dataGetter = mande('/api/calendar/count');
    try {
        numSubs.value = await dataGetter.get();
    } catch (e) {
        await user.update();
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden der Anzahl aktiver Links aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

async function fetchKey() {
    loading.value = true;
    const dataGetter = mande('/api/calendar');
    try {
        calLink.value = await dataGetter.get();
    } catch (e) {
        await user.update();
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden des Kalender-Links aufgetreten.',
        });
        console.error(e);
    } finally {
        await fetchNum();
        loading.value = false;
    }
}

async function deleteKeys() {
    loading.value = true;
    const dataGetter = mande('/api/calendar');
    try {
        await dataGetter.delete();
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
        await fetchNum();
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

await fetchNum();

const navItems = [{ label: 'Einstellungen' }];
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Einstellungen</h1>

    <h2>Begrüßung</h2>

    <div class="flex flex-wrap gap-4">
        <div class="flex items-center gap-2">
            <RadioButton
                v-model="greeting"
                inputId="servus"
                name="greeting"
                value="Servus"
                @update:modelValue="onGreetingChange"
            />
            <label for="servus">Servus</label>
        </div>
        <div class="flex items-center gap-2">
            <RadioButton
                v-model="greeting"
                inputId="moin"
                name="greeting"
                value="Moin"
                @update:modelValue="onGreetingChange"
            />
            <label for="moin">Moin</label>
        </div>
        <div class="flex items-center gap-2">
            <RadioButton
                v-model="greeting"
                inputId="hallo"
                name="greeting"
                value="Hallo"
                @update:modelValue="onGreetingChange"
            />
            <label for="hallo">Hallo</label>
        </div>
        <div class="flex items-center gap-2">
            <RadioButton
                v-model="greeting"
                inputId="gutenTag"
                name="greeting"
                value="Guten Tag"
                @update:modelValue="onGreetingChange"
            />
            <label for="gutenTag">Guten Tag</label>
        </div>
    </div>

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

    <span class="inline-flex gap-1 justify-between w-full">
        <Button
            label="Kalender-Link erstellen"
            :loading="loading"
            @click="fetchKey"
            class="p-button-primary"
        />

        <Button
            v-if="numSubs > 0"
            :label="`Alle erstellten Kalender-Links (${numSubs}) löschen`"
            severity="danger"
            @click="deleteKeys"
            class="p-button-primary"
        />
    </span>

    <div v-if="calLink" class="mt-4 p-4 rounded-[6px] bg-gray-200 dark:bg-gray-800">
        <h3>Dein persönlicher Link</h3>

        <p>Dieser Link ist wie ein Passwort. Teile ihn nicht mit Dritten.</p>

        <Button
            icon="pi pi-clipboard"
            :label="`${baseUrl}api/calendar/${calLink}.ics`"
            variant="text"
            @click.prevent="copy(`${baseUrl}api/calendar/${calLink}.ics`)"
        />
    </div>
</template>

<style scoped></style>
