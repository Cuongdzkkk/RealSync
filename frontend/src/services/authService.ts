import type { AuthSession, LoginRequest } from '@/types/auth';

import { api } from './api';

export const authService = {
  async login(payload: LoginRequest): Promise<AuthSession> {
    const { data } = await api.post('/auth/login', payload);
    return data.data;
  },

  async getProfile() {
    const { data } = await api.get('/auth/me');
    return data.data;
  }
};
