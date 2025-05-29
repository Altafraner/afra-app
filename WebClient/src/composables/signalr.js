import {ref} from "vue";
import {useToast} from "primevue";
import * as signalR from "@microsoft/signalr";

/**
 * Establishes a SignalR connection to the specified URL and provides methods to send messages and register handlers.
 * @param {string} url The URL of the SignalR hub.
 * @param {boolean} reconnect Whether to enable automatic reconnection.
 */
export function useSignalR(url, reconnect = true) {
  const connection = ref(null);
  const {resolve, reject, promise} = createDeferred();

  let conBuilder = new signalR.HubConnectionBuilder()
    .withUrl(url)
    .configureLogging(signalR.LogLevel.Information)

  if (reconnect)
    conBuilder = conBuilder.withAutomaticReconnect();

  const con = conBuilder.build();

  connection.value = con;

  con.onclose(error => {
    console.log("Connection closed: ", error);
    if (error) {
      const toast = useToast();
      toast.add({
        severity: "error",
        summary: "Verbindung getrennt",
        detail: "Die Verbindung zum Server wurde getrennt.",
      })
    }
  });

  con.start()
    .then(() => resolve())
    .catch(err => {
      const toast = useToast();
      reject(err)
      console.error("Error starting SignalR connection: ", err)
      toast.add(
        {
          severity: "error",
          summary: "Verbindungsfehler",
          detail: "Die Verbindung zum Server konnte nicht hergestellt werden.",
        }
      )
    });

  function registerReconnectHandler(handler) {
    con.onreconnected(handler);
  }

  function registerCloseHandler(handler) {
    con.onclose(handler);
  }

  function registerMessageHandler(eventName, handler) {
    con.on(eventName, handler);
  }

  function sendMessage(eventName, ...args) {
    return con.invoke(eventName, ...args)
      .catch(err => {
        const toast = useToast();
        console.error(`Error sending message '${eventName}': `, err)
        toast.add({
          severity: "error",
          summary: "Nachrichtenfehler",
          detail: `Fehler beim Senden einer Nachricht an den Server.`,
        });
      });
  }

  return {
    sendMessage,
    registerReconnectHandler,
    registerCloseHandler,
    registerMessageHandler,
    connectionPromise: promise
  };
}

function createDeferred() {
  let resolve, reject;
  const promise = new Promise((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return {promise, resolve, reject};
}
