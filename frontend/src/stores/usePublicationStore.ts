import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { PublicationJob, PublicationAttempt, QueuePublicationRequest } from '@/types/publication';
import { publicationService } from '@/services/publicationService';

export const usePublicationStore = defineStore('publication', () => {
  // State
  const jobs = ref<PublicationJob[]>([]);
  const attempts = ref<PublicationAttempt[]>([]);
  const totalJobs = ref(0);
  const loading = ref(false);
  const actionLoading = ref(false);

  // Actions

  /** Lấy danh sách jobs xuất bản */
  async function fetchJobs(params?: {
    postId?: string;
    status?: string;
    page?: number;
    pageSize?: number;
  }) {
    loading.value = true;
    try {
      const res = await publicationService.getJobs(params);
      jobs.value = res.data ?? [];
      totalJobs.value = res.meta?.totalCount ?? 0;
    } finally {
      loading.value = false;
    }
  }

  /** Gửi yêu cầu xuất bản bài viết */
  async function queueJob(request: QueuePublicationRequest): Promise<PublicationJob> {
    actionLoading.value = true;
    try {
      const newJob = await publicationService.queueJob(request);
      jobs.value.unshift(newJob);
      return newJob;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Kích hoạt thử lại job */
  async function retryJob(id: string): Promise<PublicationJob> {
    actionLoading.value = true;
    try {
      const updatedJob = await publicationService.retryJob(id);
      const idx = jobs.value.findIndex(j => j.id === id);
      if (idx !== -1) {
        jobs.value[idx] = updatedJob;
      }
      return updatedJob;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Hủy job xuất bản */
  async function cancelJob(id: string): Promise<void> {
    actionLoading.value = true;
    try {
      await publicationService.cancelJob(id);
      const idx = jobs.value.findIndex(j => j.id === id);
      if (idx !== -1) {
        jobs.value[idx].status = 'Cancelled';
      }
    } finally {
      actionLoading.value = false;
    }
  }

  /** Lấy danh sách attempts của job */
  async function refreshStatus(id: string): Promise<PublicationJob> {
    actionLoading.value = true;
    try {
      const updatedJob = await publicationService.refreshStatus(id);
      const idx = jobs.value.findIndex(j => j.id === id);
      if (idx !== -1) {
        jobs.value[idx] = updatedJob;
      }
      return updatedJob;
    } finally {
      actionLoading.value = false;
    }
  }

  async function fetchAttempts(id: string) {
    loading.value = true;
    try {
      attempts.value = await publicationService.getAttempts(id);
    } finally {
      loading.value = false;
    }
  }

  return {
    jobs,
    attempts,
    totalJobs,
    loading,
    actionLoading,
    fetchJobs,
    queueJob,
    retryJob,
    cancelJob,
    refreshStatus,
    fetchAttempts
  };
});
