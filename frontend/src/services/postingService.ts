import type { ApiResponse, PaginationQuery } from '@/types/common';
import type { AIContentGeneration, AIContentGenerateRequest, Post, PostCreateRequest } from '@/types/posting';

import { api } from './api';

export const postingService = {
  /** Tạo bài đăng mới */
  async createPost(data: PostCreateRequest): Promise<Post> {
    const { data: res } = await api.post('/posts', data);
    return res.data;
  },

  /** Lấy danh sách bài đăng */
  async getAllPosts(query?: Partial<PaginationQuery>): Promise<ApiResponse<Post[]>> {
    const { data } = await api.get('/posts', { params: query });
    return data;
  },

  /** Lấy chi tiết bài đăng */
  async getPostById(id: string): Promise<Post> {
    const { data } = await api.get(`/posts/${id}`);
    return data.data;
  },

  /** Sinh nội dung AI cho bài đăng */
  async generateContent(postId: string, request: AIContentGenerateRequest): Promise<AIContentGeneration> {
    const { data } = await api.post(`/posts/${postId}/ai-content/generate`, request, {
      timeout: 35000
    });
    return data.data;
  },

  /** Lấy lịch sử nội dung AI của bài đăng */
  async getContentHistory(postId: string): Promise<AIContentGeneration[]> {
    const { data } = await api.get(`/posts/${postId}/ai-content`);
    return data.data;
  },

  /** Cập nhật bài đăng */
  async updatePost(id: string, payload: Partial<PostCreateRequest>): Promise<Post> {
    const { data } = await api.put(`/posts/${id}`, payload);
    return data.data;
  },

  /** Xoá bài đăng (soft delete) */
  async deletePost(id: string): Promise<void> {
    await api.delete(`/posts/${id}`);
  },

  /** Tạo kênh đăng bài cho post */
  async createChannel(postId: string, channel: string): Promise<any> {
    const { data: res } = await api.post(`/posts/${postId}/channels`, { channel });
    return res.data;
  },

  /** Lấy danh sách các kênh đăng bài của post */
  async getChannels(postId: string): Promise<any[]> {
    const { data: res } = await api.get(`/posts/${postId}/channels`);
    return res.data;
  },

  /** Thực thi đăng bài lên kênh */
  async publishChannel(postId: string, channelId: string): Promise<any> {
    const { data: res } = await api.post(`/posts/${postId}/channels/${channelId}/publish`);
    return res.data;
  },

  /** Áp dụng bản nháp AI đã chỉnh sửa */
  async applyAiContent(postId: string, content: string, summary?: string): Promise<void> {
    await api.post(`/posts/${postId}/ai-content/apply`, { content, summary });
  },
};
