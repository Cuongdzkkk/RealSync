import { computed } from 'vue';

import { useAuthStore } from '@/stores/useAuthStore';
import type { UserProfile } from '@/types/auth';

export const usePermission = () => {
  const authStore = useAuthStore();

  const hasRole = (roles: UserProfile['role'][]) =>
    computed(() => Boolean(authStore.user && roles.includes(authStore.user.role)));

  return {
    hasRole
  };
};
