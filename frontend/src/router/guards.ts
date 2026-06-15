import type { NavigationGuard } from 'vue-router';

import { useAuthStore } from '@/stores/useAuthStore';

export const requireAuth: NavigationGuard = () => {
  const authStore = useAuthStore();

  // DEV: tự động login lấy token JWT thật (bất đồng bộ, không block)
  if (import.meta.env.DEV) {
    authStore.autoLoginDev();
  }

  if (!authStore.isAuthenticated) {
    return { name: 'login' };
  }

  return true;
};
