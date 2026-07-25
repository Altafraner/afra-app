<script lang="ts" setup>
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';

const loading = ref(false);
const user = useUser();
const toast = useToast();
const calLink = ref(null);

const numSubs = ref(0);

async function fetchNum() {
    loading.value = true;
    const endpoint = mande('/api/calendar/count');
    try {
        numSubs.value = await endpoint.get();
    } catch (e) {
        await user.update();
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Laden der Anzahl aktiver Links aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

async function fetchKey() {
    loading.value = true;
    const endpoint = mande('/api/calendar');
    try {
        calLink.value = await endpoint.get();
    } catch (e) {
        await user.update();
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Laden des Kalender-Links aufgetreten.',
        });
        console.error(e);
    } finally {
        await fetchNum();
        loading.value = false;
    }
}

async function deleteKeys() {
    loading.value = true;
    const endpoint = mande('/api/calendar');
    try {
        await endpoint.delete();
        calLink.value = null;
        toast.add({
            color: 'success',
            title: 'Löschung erfolgreich',
            description: 'Alle deine Kalender-Links wurden erfolgreich gelöscht.',
            duration: 2000,
        });
    } catch (e) {
        await user.update();
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Es ist ein Fehler beim Löschen der Kalender-Links aufgetreten.',
        });
        console.error(e);
    } finally {
        await fetchNum();
        loading.value = false;
    }
}

const copy = async (text: string) => {
    try {
        await navigator.clipboard.writeText(text);
        toast.add({
            color: 'success',
            title: 'Kopiert',
            description: 'Der Link wurde in die Zwischenablage kopiert.',
            duration: 2000,
        });
    } catch {
        toast.add({ color: 'error', title: 'Fehler beim Kopieren' });
    }
};

await fetchNum();

const navItems = [
    {
        label: 'Einstellungen',
    },
];
</script>

<template>
    <NavBreadcrumb :items="navItems" />
    <h1>Einstellungen</h1>

    <h2>Erscheinungsbild</h2>

    <p>Wähle, ob die App im hellen oder dunklen Design angezeigt wird.</p>

    <UColorModeSelect class="w-fit" color="neutral" variant="subtle" />

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
        Generiere einen Link und füge ihn in ein solches Programm als Kalender-Abonement ein.
        Solltest du den Link verlieren oder er aufhören zu funktionieren, kannst du beliebig oft
        einen neuen erstellen.
    </p>

    <span class="inline-flex gap-1 justify-between w-full">
        <UButton :loading="loading" label="Kalender-Link erstellen" @click="fetchKey" />

        <UButton
            v-if="numSubs > 0"
            :label="`Alle erstellten Kalender-Links (${numSubs}) löschen`"
            color="error"
            @click="deleteKeys"
        />
    </span>

    <UCard
        v-if="calLink"
        class="mt-4"
        description="Dieser Link ist wie ein Passwort. Teile ihn nicht mit Dritten."
        title="Dein persönlicher Link"
    >
        <UFieldGroup>
            <UBadge :label="calLink" color="neutral" size="xl" variant="subtle" />
            <UButton
                icon="i-lucide-clipboard"
                label="Kopieren"
                size="xl"
                variant="subtle"
                @click.prevent="copy(calLink)"
            />
        </UFieldGroup>
    </UCard>
</template>

<style scoped></style>
