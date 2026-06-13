import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { LoginRequest, UserProfile } from '@/types/auth';
import { authService } from '@/services/authService';

const storageKey = 'realsync.accessToken';

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(localStorage.getItem(storageKey));
  const user = ref<UserProfile | null>(accessToken.value
    ? {
        id: 'local-admin',
        fullName: 'RealSync Admin',
        email: 'admin@realsync.vn',
        role: 'Admin'
      }
    : null);
  const loading = ref(false);

  const isAuthenticated = computed(() => Boolean(accessToken.value));

  const login = async (payload: LoginRequest) => {
    loading.value = true;
    try {
      const session = await authService.login(payload);
      accessToken.value = session.accessToken;
      user.value = session.user;
      localStorage.setItem(storageKey, session.accessToken);
      localStorage.setItem('realsync.refreshToken', session.refreshToken);
    } finally {
      loading.value = false;
    }
  };

  const logout = () => {
    accessToken.value = null;
    user.value = null;
    localStorage.removeItem(storageKey);
    localStorage.removeItem('realsync.refreshToken');
  };

  return {
    accessToken,
    user,
    loading,
    isAuthenticated,
    login,
    logout
  };
});
