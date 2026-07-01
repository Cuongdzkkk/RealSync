import { ref } from 'vue';
import { defineStore } from 'pinia';

import type { AIContentGeneration, Post, PostCreateRequest } from '@/types/posting';
import { postingService } from '@/services/postingService';

export const usePostStore = defineStore('post', () => {
  // State
  const posts = ref<Post[]>([]);
  const currentPost = ref<Post | null>(null);
  const aiHistories = ref<AIContentGeneration[]>([]);
  const loading = ref(false);
  const generating = ref(false);

  // Actions

  /** Tạo bài đăng mới */
  async function createPost(data: PostCreateRequest): Promise<Post> {
    loading.value = true;
    try {
      const post = await postingService.createPost(data);
      posts.value.unshift(post);
      currentPost.value = post;
      return post;
    } finally {
      loading.value = false;
    }
  }

  /** Lấy danh sách bài đăng */
  async function fetchPosts(page = 1, pageSize = 20) {
    loading.value = true;
    try {
      const res = await postingService.getAllPosts({ page, pageSize });
      posts.value = res.data ?? [];
    } finally {
      loading.value = false;
    }
  }

  /** Kiểm tra chuỗi có phải GUID hợp lệ không */
  function isValidGuid(value?: string): boolean {
    if (!value) return false;
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
  }

  /** Sinh nội dung AI — tự động tạo Post nếu chưa có */
  async function generateContent(
    title: string,
    prompt: string,
    propertyId?: string,
    channel?: string,
  ): Promise<AIContentGeneration> {
    generating.value = true;
    try {
      // Chỉ gửi propertyId nếu là GUID thật (mock data dùng 'p-001' không phải GUID)
      const validPropertyId = isValidGuid(propertyId) ? propertyId : undefined;

      // 1. Tạo post mới
      const post = await postingService.createPost({
        title: `${title} - ${channel ?? 'content'}`,
        summary: `Kênh: ${channel ?? 'N/A'}`,
        propertyId: validPropertyId,
      });

      // 2. Gọi AI generate
      const result = await postingService.generateContent(post.id, {
        prompt,
        propertyId: validPropertyId,
      });

      // 3. Cập nhật nội dung bài đăng vào trường Content của Post
      const updatedPost = await postingService.updatePost(post.id, {
        title: post.title,
        content: result.generatedContent,
        propertyId: validPropertyId,
      });

      // 4. Cập nhật state
      currentPost.value = updatedPost;
      posts.value.unshift(updatedPost);

      return result;
    } finally {
      generating.value = false;
    }
  }

  /** Lấy lịch sử AI content cho post */
  async function fetchAiHistory(postId: string) {
    try {
      aiHistories.value = await postingService.getContentHistory(postId);
    } catch {
      aiHistories.value = [];
    }
  }

  /** Lấy lịch sử tất cả posts + AI content gần nhất */
  async function fetchAllHistory() {
    loading.value = true;
    try {
      const res = await postingService.getAllPosts({ page: 1, pageSize: 50 });
      posts.value = res.data ?? [];
    } finally {
      loading.value = false;
    }
  }

  /** Áp dụng bản nháp AI đã chỉnh sửa */
  async function applyAiContent(postId: string, content: string, summary?: string) {
    loading.value = true;
    try {
      await postingService.applyAiContent(postId, content, summary);
      if (currentPost.value && currentPost.value.id === postId) {
        currentPost.value.content = content;
        if (summary) currentPost.value.summary = summary;
      }
      const idx = posts.value.findIndex(p => p.id === postId);
      if (idx !== -1) {
        posts.value[idx].content = content;
        if (summary) posts.value[idx].summary = summary;
      }
    } finally {
      loading.value = false;
    }
  }

  return {
    posts,
    currentPost,
    aiHistories,
    loading,
    generating,
    createPost,
    fetchPosts,
    generateContent,
    fetchAiHistory,
    fetchAllHistory,
    applyAiContent,
  };
});
