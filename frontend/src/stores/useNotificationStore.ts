import { ref } from 'vue';
import { defineStore } from 'pinia';
import { crmNotificationMockService } from '@/services/mock/crmNotificationMockService';
import type { ApiMeta } from '@/types/crm/lead';
import type { CrmNotification, NotificationQuery, NotificationSummary } from '@/types/crm/notification';

const defaultQuery: NotificationQuery = {
  page: 1,
  pageSize: 10,
  search: '',
  isRead: null,
  readFilter: 'all',
  type: null,
  fromDate: null,
  toDate: null,
  sortBy: 'createdAt',
  sortDirection: 'desc'
};

const emptySummary: NotificationSummary = {
  totalCount: 0,
  unreadCount: 0,
  readCount: 0
};

export const useNotificationStore = defineStore('notification', () => {
  const items = ref<CrmNotification[]>([]);
  const previewItems = ref<CrmNotification[]>([]);
  const selectedNotification = ref<CrmNotification | null>(null);
  const summary = ref<NotificationSummary>({ ...emptySummary });
  const query = ref<NotificationQuery>({ ...defaultQuery });
  const pagination = ref<ApiMeta>({
    page: defaultQuery.page,
    pageSize: defaultQuery.pageSize,
    totalCount: 0,
    totalPages: 1
  });
  const loading = ref(false);
  const previewLoading = ref(false);
  const detailLoading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);
  const dropdownOpen = ref(false);
  const initializedUserId = ref<string | null>(null);

  function captureError(value: unknown) {
    error.value = value instanceof Error ? value.message : 'Đã có lỗi xảy ra.';
  }

  function currentUserId(): string {
    if (!initializedUserId.value) throw new Error('Chưa xác định người dùng thông báo.');
    return initializedUserId.value;
  }

  async function initializeForUser(userId: string) {
    if (initializedUserId.value !== userId) {
      clearForUserChange();
      initializedUserId.value = userId;
    }
    await Promise.all([fetchSummary(), fetchPreview()]);
  }

  function clearForUserChange() {
    items.value = [];
    previewItems.value = [];
    selectedNotification.value = null;
    summary.value = { ...emptySummary };
    query.value = { ...defaultQuery };
    pagination.value = {
      page: defaultQuery.page,
      pageSize: defaultQuery.pageSize,
      totalCount: 0,
      totalPages: 1
    };
    error.value = null;
    dropdownOpen.value = false;
    initializedUserId.value = null;
  }

  async function fetchNotifications() {
    loading.value = true;
    error.value = null;
    try {
      const response = await crmNotificationMockService.getNotifications(currentUserId(), query.value);
      items.value = response.data;
      if (response.meta) pagination.value = response.meta;
    } catch (err) {
      captureError(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchPreview() {
    previewLoading.value = true;
    try {
      const response = await crmNotificationMockService.getNotifications(currentUserId(), {
        ...defaultQuery,
        pageSize: 6
      });
      previewItems.value = response.data;
    } catch (err) {
      captureError(err);
    } finally {
      previewLoading.value = false;
    }
  }

  async function fetchSummary() {
    try {
      const response = await crmNotificationMockService.getSummary(currentUserId());
      summary.value = response.data;
    } catch (err) {
      captureError(err);
    }
  }

  async function fetchUnreadCount() {
    try {
      const response = await crmNotificationMockService.getUnreadCount(currentUserId());
      summary.value = {
        ...summary.value,
        unreadCount: response.data,
        readCount: Math.max(0, summary.value.totalCount - response.data)
      };
      return response.data;
    } catch (err) {
      captureError(err);
      return 0;
    }
  }

  async function fetchNotificationById(id: string) {
    detailLoading.value = true;
    error.value = null;
    try {
      const response = await crmNotificationMockService.getNotificationById(currentUserId(), id);
      selectedNotification.value = response.data;
      return response.data;
    } catch (err) {
      captureError(err);
      selectedNotification.value = null;
      return null;
    } finally {
      detailLoading.value = false;
    }
  }

  async function refreshLightweight() {
    await Promise.all([fetchSummary(), fetchPreview()]);
    await fetchNotifications();
  }

  async function markAsRead(id: string) {
    submitting.value = true;
    try {
      const response = await crmNotificationMockService.markAsRead(currentUserId(), id);
      const update = (collection: CrmNotification[]) => collection.map((item) => (item.id === id ? response.data : item));
      items.value = update(items.value);
      previewItems.value = update(previewItems.value);
      if (selectedNotification.value?.id === id) selectedNotification.value = response.data;
      await fetchSummary();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function markAllAsRead() {
    submitting.value = true;
    try {
      await crmNotificationMockService.markAllAsRead(currentUserId());
      const now = new Date().toISOString();
      items.value = items.value.map((item) => ({ ...item, isRead: true, readAt: item.readAt ?? now }));
      previewItems.value = previewItems.value.map((item) => ({ ...item, isRead: true, readAt: item.readAt ?? now }));
      if (selectedNotification.value) {
        selectedNotification.value = { ...selectedNotification.value, isRead: true, readAt: selectedNotification.value.readAt ?? now };
      }
      await refreshLightweight();
    } finally {
      submitting.value = false;
    }
  }

  async function deleteNotification(id: string) {
    submitting.value = true;
    try {
      await crmNotificationMockService.deleteNotification(currentUserId(), id);
      items.value = items.value.filter((item) => item.id !== id);
      previewItems.value = previewItems.value.filter((item) => item.id !== id);
      if (selectedNotification.value?.id === id) selectedNotification.value = null;
      await fetchSummary();
      await fetchNotifications();
    } finally {
      submitting.value = false;
    }
  }

  async function openNotification(id: string) {
    const notification = await fetchNotificationById(id);
    if (notification && !notification.isRead) await markAsRead(id);
    return selectedNotification.value ?? notification;
  }

  function setQuery(partial: Partial<NotificationQuery>) {
    query.value = {
      ...query.value,
      ...partial,
      page: partial.page ?? 1
    };
  }

  function resetFilters() {
    query.value = { ...defaultQuery };
  }

  function toggleDropdown() {
    dropdownOpen.value = !dropdownOpen.value;
  }

  function closeDropdown() {
    dropdownOpen.value = false;
  }

  return {
    items,
    previewItems,
    selectedNotification,
    summary,
    query,
    pagination,
    loading,
    previewLoading,
    detailLoading,
    submitting,
    error,
    dropdownOpen,
    initializedUserId,
    initializeForUser,
    clearForUserChange,
    fetchNotifications,
    fetchPreview,
    fetchSummary,
    fetchUnreadCount,
    fetchNotificationById,
    markAsRead,
    markAllAsRead,
    deleteNotification,
    openNotification,
    setQuery,
    resetFilters,
    toggleDropdown,
    closeDropdown
  };
});
