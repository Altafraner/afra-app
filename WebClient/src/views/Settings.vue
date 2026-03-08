<script setup>
import { Button, useToast, ToggleSwitch } from 'primevue';
import { ref, onMounted } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useNotifications } from '@/stores/notifications';
import { usePushNotifications } from '@/composables/pushNotifications';

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
    const endpoint = mande('/api/calendar');
    try {
        calLink.value = await endpoint.get();
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
    const endpoint = mande('/api/calendar');
    try {
        await endpoint.delete();
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

const notifStore = useNotifications();
const pushHelper = usePushNotifications();

const receiveEmailNotifications = ref(true);
const notifSettingsLoading = ref(false);
const pushSubscribed = ref(false);
const pushLoading = ref(false);

async function loadNotifSettings() {
    try {
        const settings = await notifStore.fetchSettings();
        receiveEmailNotifications.value = settings.receiveEmailNotifications;
    } catch (e) {
        console.error('Failed to load notification settings', e);
    }
}

async function saveNotifSettings() {
    notifSettingsLoading.value = true;
    try {
        await notifStore.saveSettings({
            receiveEmailNotifications: receiveEmailNotifications.value,
        });
        toast.add({
            severity: 'success',
            summary: 'Gespeichert',
            detail: 'Benachrichtigungseinstellungen wurden gespeichert.',
            life: 2000,
        });
    } catch (e) {
        console.error('Failed to save notification settings', e);
        toast.add({ severity: 'error', summary: 'Fehler beim Speichern' });
    } finally {
        notifSettingsLoading.value = false;
    }
}

async function checkPushSubscription() {
    if (!pushHelper.isSupported()) return;
    const reg = await navigator.serviceWorker.ready;
    const sub = await reg.pushManager.getSubscription();
    pushSubscribed.value = !!sub;
}

async function togglePush() {
    pushLoading.value = true;
    try {
        if (pushSubscribed.value) {
            await pushHelper.unsubscribe();
            pushSubscribed.value = false;
            toast.add({
                severity: 'info',
                summary: 'Push-Benachrichtigungen deaktiviert',
                life: 2000,
            });
        } else {
            const sub = await pushHelper.subscribe();
            if (sub) {
                pushSubscribed.value = true;
                toast.add({
                    severity: 'success',
                    summary: 'Push-Benachrichtigungen aktiviert',
                    life: 2000,
                });
            } else if (pushHelper.getPermission() === 'denied') {
                toast.add({
                    severity: 'warn',
                    summary: 'Berechtigung verweigert',
                    detail: 'Bitte erlauben Sie Benachrichtigungen in den Browser-Einstellungen.',
                });
            }
        }
    } catch (e) {
        console.error(e);
        toast.add({ severity: 'error', summary: 'Fehler beim Ändern der Push-Einstellung' });
    } finally {
        pushLoading.value = false;
    }
}

const testLoading = ref(false);
async function sendTestNotification() {
    testLoading.value = true;
    try {
        await notifStore.sendTestNotification();
        toast.add({
            severity: 'success',
            summary: 'Testbenachrichtigung gesendet',
            detail: 'Schau in das Benachrichtigungsglöckchen oben rechts.',
            life: 3000,
        });
    } catch (e) {
        console.error(e);
        toast.add({
            severity: 'error',
            summary: 'Fehler beim Senden der Testbenachrichtigung',
        });
    } finally {
        testLoading.value = false;
    }
}

await fetchNum();
onMounted(async () => {
    await loadNotifSettings();
    await checkPushSubscription();
});

const navItems = [
    {
        label: 'Einstellungen',
    },
];
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
        Generiere einen Link und füge ihn in ein solches Programm als Kalender-Abonement ein.
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
            :label="calLink"
            variant="text"
            @click.prevent="copy(calLink)"
        />
    </div>

    <h2>Benachrichtigungen</h2>

    <div class="flex flex-col gap-4">
        <div class="flex items-center justify-between gap-4">
            <div>
                <p class="font-medium m-0">E-Mail-Benachrichtigungen</p>
                <p class="text-sm text-surface-500 m-0 mt-1">
                    Benachrichtigungen werden zusätzlich per E-Mail zugestellt.
                </p>
            </div>
            <ToggleSwitch v-model="receiveEmailNotifications" @change="saveNotifSettings" />
        </div>

        <div v-if="pushHelper.isSupported()" class="flex items-center justify-between gap-4">
            <div>
                <p class="font-medium m-0">Push-Benachrichtigungen</p>
                <p class="text-sm text-surface-500 m-0 mt-1">
                    Benachrichtigungen auch dann anzeigen, wenn die App nicht geöffnet ist.
                </p>
            </div>
            <Button
                :label="pushSubscribed ? 'Deaktivieren' : 'Aktivieren'"
                :severity="pushSubscribed ? 'danger' : 'primary'"
                :loading="pushLoading"
                size="small"
                @click="togglePush"
            />
        </div>

        <div class="flex items-center justify-between gap-4">
            <div>
                <p class="font-medium m-0">Testbenachrichtigung</p>
                <p class="text-sm text-surface-500 m-0 mt-1">
                    Sendet eine Testbenachrichtigung, um das System zu prüfen.
                </p>
            </div>
            <Button
                label="Senden"
                icon="pi pi-send"
                severity="secondary"
                size="small"
                :loading="testLoading"
                @click="sendTestNotification"
            />
        </div>
    </div>
</template>

<style scoped></style>
