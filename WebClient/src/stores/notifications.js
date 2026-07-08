import { defineStore } from 'pinia';
import { mande } from 'mande';
import * as signalR from '@microsoft/signalr';

export const useNotifications = defineStore('notifications', {
    state: () => ({
        /** @type {{ id: string, subject: string, body: string, createdAt: string, isRead: boolean }[]} */
        notifications: [],
        loading: false,
        /** @type {signalR.HubConnection|null} */
        _connection: null,
    }),

    getters: {
        unreadCount: (state) => state.notifications.filter((n) => !n.isRead).length,
    },

    actions: {
        async fetchNotifications() {
            this.loading = true;
            try {
                const api = mande('/api/notifications');
                this.notifications = await api.get('');
            } finally {
                this.loading = false;
            }
        },

        async markAsRead(id) {
            const api = mande(`/api/notifications/${id}/read`);
            await api.put('');
            const n = this.notifications.find((n) => n.id === id);
            if (n) n.isRead = true;
        },

        async dismiss(id) {
            const api = mande(`/api/notifications/${id}`);
            await api.delete('');
            this.notifications = this.notifications.filter((n) => n.id !== id);
        },

        async markAllRead() {
            const unread = this.notifications.filter((n) => !n.isRead);
            await Promise.all(unread.map((n) => this.markAsRead(n.id)));
        },

        connectHub() {
            if (this._connection) return;

            const connection = new signalR.HubConnectionBuilder()
                .withUrl('/api/notifications/hub')
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            connection.on('ReceiveNotification', (notification) => {
                this.notifications.unshift({
                    id: notification.id,
                    subject: notification.subject,
                    body: notification.body,
                    createdAt: notification.createdAt,
                    isRead: false,
                });
            });

            connection.start().catch((err) => console.error('Notification hub error:', err));
            this._connection = connection;
        },

        disconnectHub() {
            if (this._connection) {
                this._connection.stop();
                this._connection = null;
            }
        },

        async sendTestNotification() {
            const api = mande('/api/notifications/test');
            await api.post('');
        },

        async fetchSettings() {
            const api = mande('/api/notifications/settings');
            return await api.get('');
        },

        async saveSettings(settings) {
            const api = mande('/api/notifications/settings');
            await api.put('', settings);
        },

        async getVapidPublicKey() {
            try {
                const api = mande('/api/notifications/vapid-public-key');
                const result = await api.get('');
                return result.publicKey ?? null;
            } catch {
                return null;
            }
        },

        async savePushSubscription(subscription) {
            const api = mande('/api/notifications/push-subscription');
            const json = subscription.toJSON();
            await api.post('', {
                endpoint: json.endpoint,
                p256dh: json.keys.p256dh,
                auth: json.keys.auth,
            });
        },

        async removePushSubscription(subscription) {
            const api = mande('/api/notifications/push-subscription');
            const json = subscription.toJSON();
            await api.post('unsubscribe', {
                endpoint: json.endpoint,
                p256dh: json.keys.p256dh,
                auth: json.keys.auth,
            });
        },
    },
});
