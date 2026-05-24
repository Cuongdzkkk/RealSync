import { ref } from 'vue';
import * as signalR from '@microsoft/signalr';

export const useSignalR = (hubUrl: string) => {
  const connection = ref<signalR.HubConnection | null>(null);
  const connected = ref(false);

  const start = async () => {
    connection.value = new signalR.HubConnectionBuilder().withUrl(hubUrl).withAutomaticReconnect().build();
    await connection.value.start();
    connected.value = true;
  };

  const stop = async () => {
    await connection.value?.stop();
    connected.value = false;
  };

  return {
    connection,
    connected,
    start,
    stop
  };
};
