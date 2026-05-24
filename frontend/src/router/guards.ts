import type { NavigationGuard } from 'vue-router';

import { useAuthStore } from '@/stores/useAuthStore';

export const requireAuth: NavigationGuard = () => {
  const authStore = useAuthStore();

  if (!authStore.isAuthenticated) {
    return { name: 'login' };
  }

  return true;
};
