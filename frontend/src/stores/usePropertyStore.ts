import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { Property } from '@/types/property';
import { mockProperties } from '@/utils/mockData';

export const usePropertyStore = defineStore('property', () => {
  const items = ref<Property[]>(mockProperties);
  const loading = ref(false);

  const setItems = (properties: Property[]) => {
    items.value = properties;
  };

  const addProperty = (property: Property) => {
    items.value.unshift(property);
  };

  const updateProperty = (updatedProperty: Property) => {
    const idx = items.value.findIndex(p => p.id === updatedProperty.id);
    if (idx !== -1) {
      items.value[idx] = updatedProperty;
    }
  };

  const deleteProperty = (id: string) => {
    items.value = items.value.filter(p => p.id !== id);
  };

  return {
    items,
    loading,
    setItems,
    addProperty,
    updateProperty,
    deleteProperty
  };
});
