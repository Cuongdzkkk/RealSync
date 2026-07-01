import axios from 'axios';
import { ElMessage } from 'element-plus';

import { API_BASE_URL } from '@/utils/constants';

export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json'
  }
});

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
    // Nếu 401 → xóa token và redirect đến login
    if (error.response?.status === 401) {
      // Tránh redirect loop nếu đang ở trang login
      const isLoginPage = window.location.pathname.includes('/login') ||
        window.location.pathname.includes('/auth');

      if (!isLoginPage) {
        localStorage.removeItem('realsync.accessToken');
        localStorage.removeItem('realsync.user');
        ElMessage.error('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
        // Redirect sau 1 giây để message hiển thị
        setTimeout(() => {
          window.location.href = '/login';
        }, 1000);
      }
      return Promise.reject(error);
    }

    // Không hiển thị lỗi nếu request bị hủy
    if (axios.isCancel(error)) {
      return Promise.reject(error);
    }

    let message = error.response?.data?.message;
    if (!message) {
      if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
        message = 'Yêu cầu quá hạn. AI hoặc máy chủ không phản hồi kịp.';
      } else if (error.code === 'ERR_NETWORK') {
        message = 'Không thể kết nối đến máy chủ. Kiểm tra lại kết nối mạng.';
      } else {
        message = 'Có lỗi xảy ra. Vui lòng thử lại.';
      }
    }
    ElMessage.error(message);
    return Promise.reject(error);
  }
);
