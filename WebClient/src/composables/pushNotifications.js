import { useNotifications } from '@/stores/notifications';

export function usePushNotifications() {
    const store = useNotifications();

    function isSupported() {
        return 'serviceWorker' in navigator && 'PushManager' in window;
    }

    function getPermission() {
        return Notification.permission;
    }

    async function subscribe() {
        if (!isSupported()) return null;

        const publicKey = await store.getVapidPublicKey();
        if (!publicKey) return null;

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') return null;

        const registration = await getServiceWorkerRegistration();
        const existing = await registration.pushManager.getSubscription();
        if (existing) {
            await store.savePushSubscription(existing);
            return existing;
        }

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(publicKey),
        });

        await store.savePushSubscription(subscription);
        return subscription;
    }

    async function unsubscribe() {
        if (!isSupported()) return;

        const registration = await getServiceWorkerRegistration();
        const subscription = await registration.pushManager.getSubscription();
        if (!subscription) return;

        await store.removePushSubscription(subscription);
        await subscription.unsubscribe();
    }

    return { isSupported, getPermission, subscribe, unsubscribe };
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    return Uint8Array.from([...rawData].map((c) => c.charCodeAt(0)));
}

function getServiceWorkerRegistration(timeoutMs = 5000) {
    return Promise.race([
        navigator.serviceWorker.ready,
        new Promise((_, reject) =>
            setTimeout(
                () => reject(new Error('Service worker not ready within timeout')),
                timeoutMs,
            ),
        ),
    ]);
}
