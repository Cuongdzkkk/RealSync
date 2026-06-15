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

export const requireRole: NavigationGuard = (to) => {
  const allowedRoles = to.meta.roles;
  if (!allowedRoles || allowedRoles.length === 0) return true;

  const authStore = useAuthStore();
  if (!authStore.user || !allowedRoles.includes(authStore.user.role)) {
    return { name: 'unauthorized' };
  }

  return true;
};
