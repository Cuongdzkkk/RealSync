import { ref } from 'vue';
import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', () => {
  const mobileSidebarOpen = ref(false);
  const sidebarCollapsed = ref(false);

  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value;
  };

  const toggleMobileSidebar = () => {
    mobileSidebarOpen.value = !mobileSidebarOpen.value;
  };

  return {
    mobileSidebarOpen,
    sidebarCollapsed,
    toggleSidebar,
    toggleMobileSidebar
  };
});
