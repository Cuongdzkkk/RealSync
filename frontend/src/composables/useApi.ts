import { ref } from 'vue';

export const useApi = <T>() => {
  const data = ref<T | null>(null);
  const loading = ref(false);
  const error = ref<unknown>(null);

  const execute = async (request: () => Promise<T>) => {
    loading.value = true;
    error.value = null;
    try {
      data.value = await request();
      return data.value;
    } catch (requestError) {
      error.value = requestError;
      throw requestError;
    } finally {
      loading.value = false;
    }
  };

  return {
    data,
    loading,
    error,
    execute
  };
};
