import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { Property } from '@/types/property';
import { mockProperties } from '@/utils/mockData';
import { propertyService } from '@/services/propertyService';

export const usePropertyStore = defineStore('property', () => {
  const items = ref<Property[]>(mockProperties);
  const loading = ref(false);

  const setItems = (properties: Property[]) => {
    items.value = properties;
  };

  const fetchProperties = async () => {
    loading.value = true;
    try {
      const res = await propertyService.getProperties({ page: 1, pageSize: 100 });
      items.value = res.data ?? [];
    } catch (err) {
      console.error('Failed to fetch properties:', err);
    } finally {
      loading.value = false;
    }
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
    fetchProperties,
    addProperty,
    updateProperty,
    deleteProperty
  };
});
