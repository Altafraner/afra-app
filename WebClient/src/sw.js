import { cleanupOutdatedCaches, precacheAndRoute } from 'workbox-precaching';
import { NavigationRoute, registerRoute } from 'workbox-routing';
import { NetworkFirst } from 'workbox-strategies';

precacheAndRoute(self.__WB_MANIFEST);

cleanupOutdatedCaches();

registerRoute(
    new NavigationRoute(new NetworkFirst(), {
        denylist: [/^\/api/],
    }),
);

self.addEventListener('push', (event) => {
    let title = 'Afra-App';
    let body = 'Neue Benachrichtigung';

    if (event.data) {
        try {
            const data = event.data.json();
            title = data.title ?? title;
            body = data.body ?? body;
        } catch {
            body = event.data.text();
        }
    }

    event.waitUntil(
        self.registration.showNotification(title, {
            body,
            icon: '/vdaa/appicon.svg',
            badge: '/vdaa/favicon.svg',
        }),
    );
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        clients
            .matchAll({ type: 'window', includeUncontrolled: true })
            .then((windowClients) => {
                const existing = windowClients.find((c) =>
                    c.url.startsWith(self.location.origin),
                );
                if (existing) return existing.focus();
                return clients.openWindow('/');
            }),
    );
});
