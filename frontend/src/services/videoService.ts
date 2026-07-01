import type { ApiResponse } from '@/types/common';
import type { VideoProject, VideoGenerationJob, VideoScene } from '@/types/video';
import { api } from './api';

export const videoService = {
  /** Tạo dự án video AI mới từ biến thể hoặc bài viết */
  async createProject(contentVariantId: string, postId?: string): Promise<ApiResponse<VideoProject>> {
    const { data } = await api.post('/videoprojects', { contentVariantId, postId });
    return data;
  },

  /** Lấy chi tiết dự án video */
  async getProjectById(id: string): Promise<ApiResponse<VideoProject>> {
    const { data } = await api.get(`/videoprojects/${id}`);
    return data;
  },

  /** Cập nhật kịch bản phân cảnh (storyboard) */
  async updateStoryboard(id: string, scenes: any[]): Promise<ApiResponse<VideoProject>> {
    const { data } = await api.put(`/videoprojects/${id}/storyboard`, scenes);
    return data;
  },

  /** Bắt đầu sinh các phân cảnh video bằng AI */
  async generateVideo(id: string): Promise<ApiResponse<VideoGenerationJob>> {
    const { data } = await api.post(`/videoprojects/${id}/generate`);
    return data;
  },

  /** Ghép nối video thành phẩm */
  async renderVideo(id: string): Promise<ApiResponse<VideoGenerationJob>> {
    const { data } = await api.post(`/videoprojects/${id}/render`);
    return data;
  }
};
