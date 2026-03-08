<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { Button, Popover, Badge, ProgressSpinner } from 'primevue';
import { useNotifications } from '@/stores/notifications';

const store = useNotifications();
const popover = ref(null);
const anchor = ref(null);

const unread = computed(() => store.unreadCount);

function toggle(event) {
    popover.value?.toggle(event);
}

function formatDate(iso) {
    return new Date(iso).toLocaleString('de-DE', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    });
}

async function markRead(n) {
    if (!n.isRead) await store.markAsRead(n.id);
}

async function dismiss(n) {
    await store.dismiss(n.id);
}

async function markAllRead() {
    await store.markAllRead();
}

onMounted(async () => {
    await store.fetchNotifications();
    store.connectHub();
});

onUnmounted(() => {
    store.disconnectHub();
});
</script>

<template>
    <span class="relative inline-flex items-center">
        <Button
            ref="anchor"
            icon="pi pi-bell"
            variant="text"
            severity="secondary"
            aria-label="Benachrichtigungen"
            @click="toggle"
        />
        <Badge
            v-if="unread > 0"
            :value="unread > 99 ? '99+' : unread"
            severity="danger"
            class="absolute -top-1 -right-1 pointer-events-none"
        />
    </span>

    <Popover ref="popover" class="w-96 max-w-[95vw]">
        <div class="flex items-center justify-between mb-3">
            <h3 class="text-base font-semibold m-0">Benachrichtigungen</h3>
            <Button
                v-if="unread > 0"
                label="Alle als gelesen markieren"
                variant="text"
                size="small"
                @click="markAllRead"
            />
        </div>

        <ProgressSpinner v-if="store.loading" class="w-8 h-8" />

        <p v-else-if="store.notifications.length === 0" class="text-sm text-surface-500 m-0">
            Keine Benachrichtigungen
        </p>

        <ul v-else class="list-none m-0 p-0 flex flex-col gap-2 max-h-[70vh] overflow-y-auto">
            <li
                v-for="n in store.notifications"
                :key="n.id"
                class="p-3 rounded-md cursor-pointer transition-colors"
                :class="
                    n.isRead
                        ? 'bg-surface-100 dark:bg-surface-800'
                        : 'bg-primary-50 dark:bg-primary-950'
                "
                @click="markRead(n)"
            >
                <div class="flex justify-between items-start gap-2">
                    <span
                        class="font-medium text-sm leading-tight"
                        :class="n.isRead ? '' : 'text-primary-700 dark:text-primary-300'"
                    >
                        {{ n.subject }}
                    </span>
                    <Button
                        icon="pi pi-times"
                        variant="text"
                        severity="secondary"
                        size="small"
                        class="flex-shrink-0 -mt-1 -mr-1"
                        aria-label="Schließen"
                        @click.stop="dismiss(n)"
                    />
                </div>
                <p
                    class="text-sm text-surface-600 dark:text-surface-400 mt-1 mb-0 whitespace-pre-wrap"
                >
                    {{ n.body }}
                </p>
                <span class="text-xs text-surface-400 mt-1 block">{{
                    formatDate(n.createdAt)
                }}</span>
            </li>
        </ul>
    </Popover>
</template>

<style scoped></style>
