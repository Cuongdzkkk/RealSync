import type { ApiResponse } from '@/types/common';
import type { PublicationJob, PublicationAttempt, QueuePublicationRequest } from '@/types/publication';
import { api } from './api';

export const publicationService = {
  /** Lấy danh sách jobs xuất bản */
  async getJobs(params?: {
    postId?: string;
    status?: string;
    page?: number;
    pageSize?: number;
  }): Promise<ApiResponse<PublicationJob[]>> {
    const { data } = await api.get('/publications/jobs', { params });
    return data;
  },

  /** Đưa bài đăng vào hàng đợi xuất bản */
  async queueJob(request: QueuePublicationRequest): Promise<PublicationJob> {
    const { data: res } = await api.post('/publications/jobs', request);
    return res.data;
  },

  /** Thử lại một job xuất bản bị lỗi */
  async retryJob(id: string): Promise<PublicationJob> {
    const { data: res } = await api.post(`/publications/jobs/${id}/retry`);
    return res.data;
  },

  /** Hủy một job xuất bản */
  async cancelJob(id: string): Promise<void> {
    await api.post(`/publications/jobs/${id}/cancel`);
  },

  async refreshStatus(id: string): Promise<PublicationJob> {
    const { data: res } = await api.post(`/publications/jobs/${id}/refresh-status`);
    return res.data;
  },

  /** Lấy lịch sử các lượt thử của job */
  async getAttempts(id: string): Promise<PublicationAttempt[]> {
    const { data: res } = await api.get(`/publications/jobs/${id}/attempts`);
    return res.data;
  }
};
