import { ref } from 'vue';
import { defineStore } from 'pinia';

import type { Property } from '@/types/property';

export const usePropertyStore = defineStore('property', () => {
  const items = ref<Property[]>([]);
  const loading = ref(false);

  const setItems = (properties: Property[]) => {
    items.value = properties;
  };

  return {
    items,
    loading,
    setItems
  };
});
