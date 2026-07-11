import { ref } from 'vue';
import * as signalR from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr';
import { Toast } from '@nuxt/ui/composables';

/**
 * Establishes a SignalR connection to the specified URL and provides methods to send messages and register handlers.
 * @param url The URL of the SignalR hub.
 * @param reconnect Whether to enable automatic reconnection.
 * @param toastService Optional service for displaying toast notifications.
 */
export function useSignalR(
    url: string,
    reconnect: boolean = true,
    toastService: { add: (message: Partial<Toast>) => void } = {
        add: () => undefined,
    },
) {
    const connection = ref<HubConnection | null>(null);
    const { resolve, reject, promise } = createDeferred();

    let conBuilder = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .configureLogging(signalR.LogLevel.Information);

    if (reconnect) conBuilder = conBuilder.withAutomaticReconnect();

    const con = conBuilder.build();

    connection.value = con;

    con.onclose((error: Error | undefined): void => {
        console.log('Connection closed: ', error);
        if (error) {
            toastService.add({
                color: 'error',
                title: 'Verbindung getrennt',
                description: 'Die Verbindung zum Server wurde getrennt.',
            });
        }
    });

    con.start()
        .then(() => resolve(undefined))
        .catch((err) => {
            reject(err);
            console.error('Error starting SignalR connection: ', err);
            toastService.add({
                color: 'error',
                title: 'Verbindungsfehler',
                description: 'Die Verbindung zum Server konnte nicht hergestellt werden.',
            });
        });

    function registerReconnectHandler(handler: (connectionId?: string) => void): void {
        con.onreconnected(handler);
    }

    function registerCloseHandler(handler: (error: Error | undefined) => void): void {
        con.onclose(handler);
    }

    function registerMessageHandler(eventName: string, handler: (...args: any[]) => any): void {
        con.on(eventName, handler);
    }

    async function sendMessage(eventName: string, ...args: any[]): Promise<any> {
        try {
            return await con.invoke(eventName, ...args);
        } catch (err) {
            console.error(`Error sending message '${eventName}': `, err);
            toastService.add({
                color: 'error',
                title: 'Nachrichtenfehler',
                description: 'Fehler beim Senden einer Nachricht an den Server.',
            });
        }
    }

    async function closeConnection(): Promise<void> {
        try {
            await con.stop();
            console.log('SignalR connection closed successfully.');
        } catch (error) {
            console.error('Error closing SignalR connection: ', error);
        }
    }

    return {
        sendMessage,
        registerReconnectHandler,
        registerCloseHandler,
        registerMessageHandler,
        closeConnection,
        connectionPromise: promise,
    };
}

function createDeferred(): {
    promise: Promise<unknown>;
    resolve: (value: unknown) => void;
    reject: (reason?: unknown) => void;
} {
    let resolve: (value: unknown) => void = () => {},
        reject: (reason?: unknown) => void = () => {};
    const promise = new Promise((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
}
