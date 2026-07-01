import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { VideoProject, VideoGenerationJob } from '@/types/video';
import { videoService } from '@/services/videoService';

export const useVideoStore = defineStore('video', () => {
  // State
  const currentProject = ref<VideoProject | null>(null);
  const currentJob = ref<VideoGenerationJob | null>(null);
  const loading = ref(false);
  const actionLoading = ref(false);

  // Actions

  /** Tạo dự án video AI mới */
  async function createProject(contentVariantId: string, postId?: string): Promise<VideoProject> {
    loading.value = true;
    try {
      const res = await videoService.createProject(contentVariantId, postId);
      currentProject.value = res.data;
      return res.data;
    } finally {
      loading.value = false;
    }
  }

  /** Lấy chi tiết dự án video */
  async function fetchProject(id: string): Promise<VideoProject> {
    loading.value = true;
    try {
      const res = await videoService.getProjectById(id);
      currentProject.value = res.data;
      return res.data;
    } finally {
      loading.value = false;
    }
  }

  /** Cập nhật kịch bản phân cảnh */
  async function updateStoryboard(id: string, scenes: any[]): Promise<VideoProject> {
    actionLoading.value = true;
    try {
      const res = await videoService.updateStoryboard(id, scenes);
      currentProject.value = res.data;
      return res.data;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Bắt đầu sinh các phân cảnh video */
  async function generateVideo(id: string): Promise<VideoGenerationJob> {
    actionLoading.value = true;
    try {
      const res = await videoService.generateVideo(id);
      currentJob.value = res.data;
      // Refresh project to get generating status
      await fetchProject(id);
      return res.data;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Ghép nối video thành phẩm */
  async function renderVideo(id: string): Promise<VideoGenerationJob> {
    actionLoading.value = true;
    try {
      const res = await videoService.renderVideo(id);
      currentJob.value = res.data;
      // Refresh project to get rendering status
      await fetchProject(id);
      return res.data;
    } finally {
      actionLoading.value = false;
    }
  }

  return {
    currentProject,
    currentJob,
    loading,
    actionLoading,
    createProject,
    fetchProject,
    updateStoryboard,
    generateVideo,
    renderVideo
  };
});
