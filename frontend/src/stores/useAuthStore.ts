import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { LoginRequest, UserProfile } from '@/types/auth';
import { authService } from '@/services/authService';

const storageKey = 'realsync.accessToken';
const userStorageKey = 'realsync.user';

function loadUser(): UserProfile | null {
  const raw = localStorage.getItem(userStorageKey);
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return null; }
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(localStorage.getItem(storageKey));
  const user = ref<UserProfile | null>(loadUser());
  const loading = ref(false);

  // DEV mode luôn cho qua — còn gọi API thật sẽ có interceptor xử lý 401
  const isAuthenticated = computed(() => Boolean(accessToken.value) || import.meta.env.DEV);

  /** Tự động login bằng tài khoản admin ở môi trường DEV (gọi API lấy token thật) */
  async function autoLoginDev() {
    if (accessToken.value) return;
    try {
      loading.value = true;
      const session = await authService.login({ email: 'admin@realsync.vn', password: 'Admin@123' });
      accessToken.value = session.accessToken;
      user.value = session.user;
      localStorage.setItem(storageKey, session.accessToken);
      localStorage.setItem('realsync.refreshToken', session.refreshToken);
    } catch {
      // fail silently — vẫn dùng user mock, API sẽ tự login lại nếu 401
    } finally {
      loading.value = false;
    }
  }

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
    localStorage.removeItem(userStorageKey);
  };

  return {
    accessToken,
    user,
    loading,
    isAuthenticated,
    autoLoginDev,
    login,
    logout
  };
});
