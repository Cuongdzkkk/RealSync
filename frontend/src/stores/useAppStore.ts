import { ref, watch } from 'vue';
import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', () => {
  const mobileSidebarOpen = ref(false);
  const sidebarCollapsed = ref(false);
  
  // Initialize theme from localStorage or default to 'light'
  const savedTheme = localStorage.getItem('realsync.theme') as 'light' | 'dark' | null;
  const theme = ref<'light' | 'dark'>(savedTheme || 'light');

  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value;
  };

  const toggleMobileSidebar = () => {
    mobileSidebarOpen.value = !mobileSidebarOpen.value;
  };

  const toggleTheme = () => {
    theme.value = theme.value === 'light' ? 'dark' : 'light';
  };

  // Sync theme to DOM and localStorage
  watch(
    theme,
    (newTheme) => {
      localStorage.setItem('realsync.theme', newTheme);
      const root = document.documentElement;
      if (newTheme === 'dark') {
        root.classList.add('dark');
        root.setAttribute('data-theme', 'dark');
      } else {
        root.classList.remove('dark');
        root.setAttribute('data-theme', 'light');
      }
    },
    { immediate: true }
  );

  return {
    mobileSidebarOpen,
    sidebarCollapsed,
    theme,
    toggleSidebar,
    toggleMobileSidebar,
    toggleTheme
  };
});

