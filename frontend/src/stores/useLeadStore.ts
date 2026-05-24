import { ref } from 'vue';
import { defineStore } from 'pinia';

import type { Lead } from '@/types/lead';

export const useLeadStore = defineStore('lead', () => {
  const items = ref<Lead[]>([]);
  const loading = ref(false);

  const setItems = (leads: Lead[]) => {
    items.value = leads;
  };

  return {
    items,
    loading,
    setItems
  };
});
