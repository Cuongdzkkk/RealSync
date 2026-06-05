import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { Lead } from '@/types/lead';
import { mockLeads } from '@/utils/mockData';

export const useLeadStore = defineStore('lead', () => {
  const items = ref<Lead[]>(mockLeads);
  const loading = ref(false);

  const setItems = (leads: Lead[]) => {
    items.value = leads;
  };

  const addLead = (lead: Lead) => {
    items.value.unshift(lead);
  };

  const updateLead = (updatedLead: Lead) => {
    const idx = items.value.findIndex(l => l.id === updatedLead.id);
    if (idx !== -1) {
      items.value[idx] = updatedLead;
    }
  };

  const deleteLead = (id: string) => {
    items.value = items.value.filter(l => l.id !== id);
  };

  return {
    items,
    loading,
    setItems,
    addLead,
    updateLead,
    deleteLead
  };
});
