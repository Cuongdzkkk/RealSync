import axios from 'axios';
import { ElMessage } from 'element-plus';

import { API_BASE_URL } from '@/utils/constants';

export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Track trạng thái đang login để tránh retry nhiều lần
let isRefreshing = false;
let pendingRequests: Array<(token: string) => void> = [];

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('realsync.accessToken');

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Nếu 401 và chưa retry lần nào → thử auto-login
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (!isRefreshing) {
        isRefreshing = true;

        try {
          // Gọi API login với tài khoản admin
          const { data } = await axios.post(`${API_BASE_URL}/auth/login`, {
            email: 'admin@realsync.vn',
            password: 'Admin@123'
          });

          const newToken = data.data.accessToken;
          localStorage.setItem('realsync.accessToken', newToken);

          // Retry tất cả requests đang chờ
          pendingRequests.forEach(cb => cb(newToken));
          pendingRequests = [];

          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        } catch {
          pendingRequests = [];
          ElMessage.error('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
          return Promise.reject(error);
        } finally {
          isRefreshing = false;
        }
      } else {
        // Đang có login khác chạy — đợi nó xong rồi retry
        return new Promise((resolve) => {
          pendingRequests.push((token: string) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            resolve(api(originalRequest));
          });
        });
      }
    }

    const message = error.response?.data?.message ?? 'Có lỗi xảy ra. Vui lòng thử lại.';
    ElMessage.error(message);
    return Promise.reject(error);
  }
);
