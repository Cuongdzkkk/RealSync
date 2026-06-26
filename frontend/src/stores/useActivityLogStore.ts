import { ref } from 'vue';
import { defineStore } from 'pinia';
import { activityLogService } from '@/services/activityLogService';
import type { ApiMeta } from '@/types/crm/lead';
import type { ActivityLog, ActivityLogQuery } from '@/types/crm/activity';

const defaultQuery: ActivityLogQuery = {
  page: 1,
  pageSize: 20,
  search: '',
  userId: null,
  entityType: null,
  entityId: null,
  action: null,
  fromDate: null,
  toDate: null,
  sortBy: 'createdAt',
  sortDirection: 'desc'
};

export const useActivityLogStore = defineStore('activityLog', () => {
  const items = ref<ActivityLog[]>([]);
  const query = ref<ActivityLogQuery>({ ...defaultQuery });
  const pagination = ref<ApiMeta>({
    page: defaultQuery.page,
    pageSize: defaultQuery.pageSize,
    totalCount: 0,
    totalPages: 1
  });
  const loading = ref(false);
  const error = ref<string | null>(null);

  function captureError(value: unknown) {
    error.value = value instanceof Error ? value.message : 'Không thể tải activity log.';
  }

  async function fetchActivityLogs() {
    loading.value = true;
    error.value = null;
    try {
      const response = await activityLogService.getActivityLogs(query.value);
      items.value = response.data;
      if (response.meta) pagination.value = response.meta;
    } catch (err) {
      captureError(err);
    } finally {
      loading.value = false;
    }
  }

  function setQuery(partial: Partial<ActivityLogQuery>) {
    query.value = {
      ...query.value,
      ...partial,
      page: partial.page ?? 1
    };
  }

  function resetFilters() {
    query.value = { ...defaultQuery };
  }

  return {
    items,
    query,
    pagination,
    loading,
    error,
    fetchActivityLogs,
    setQuery,
    resetFilters
  };
});
