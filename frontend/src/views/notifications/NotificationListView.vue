<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import { Check } from '@element-plus/icons-vue';
import CrmPageHeader from '@/components/crm/common/CrmPageHeader.vue';
import NotificationDetailDrawer from '@/components/crm/notifications/NotificationDetailDrawer.vue';
import NotificationFilters from '@/components/crm/notifications/NotificationFilters.vue';
import NotificationList from '@/components/crm/notifications/NotificationList.vue';
import NotificationSummary from '@/components/crm/notifications/NotificationSummary.vue';
import { useAuthStore } from '@/stores/useAuthStore';
import { useNotificationStore } from '@/stores/useNotificationStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CrmNotification } from '@/types/crm/notification';
import { getNotificationUserId } from '@/utils/notification';

const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const toastStore = useToastStore();
const router = useRouter();
const detailOpen = ref(false);

const notificationUserId = computed(() => getNotificationUserId(authStore.user));
const role = computed(() => authStore.user?.role ?? 'Viewer');
const canWrite = computed(() => role.value !== 'Viewer');

async function boot(userId: string) {
  await notificationStore.initializeForUser(userId);
  await notificationStore.fetchNotifications();
}

onMounted(() => boot(notificationUserId.value));

watch(notificationUserId, (value) => boot(value));

watch(
  () => notificationStore.query,
  () => notificationStore.fetchNotifications(),
  { deep: true }
);

async function openNotification(notification: CrmNotification) {
  await notificationStore.fetchNotificationById(notification.id);
  detailOpen.value = true;
}

async function markRead(notification: CrmNotification) {
  if (!canWrite.value) return;
  try {
    await notificationStore.markAsRead(notification.id);
    toastStore.success('Đã đánh dấu thông báo là đã đọc');
  } catch (error) {
    toastStore.error('Không thể cập nhật thông báo', error instanceof Error ? error.message : undefined);
  }
}

async function markAllRead() {
  if (!canWrite.value) return;
  try {
    await notificationStore.markAllAsRead();
    toastStore.success('Đã đánh dấu tất cả thông báo là đã đọc');
  } catch (error) {
    toastStore.error('Không thể cập nhật thông báo', error instanceof Error ? error.message : undefined);
  }
}

async function deleteNotification(notification: CrmNotification) {
  if (!canWrite.value) return;
  try {
    await ElMessageBox.confirm(`Xóa thông báo "${notification.title}"?`, 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
    });
    await notificationStore.deleteNotification(notification.id);
    detailOpen.value = false;
    toastStore.success('Đã xóa thông báo');
  } catch {
    // User canceled.
  }
}

function openLink(path: string) {
  detailOpen.value = false;
  router.push(path);
}
</script>

<template>
  <div class="page notification-page">
    <CrmPageHeader title="Thông báo" subtitle="Theo dõi cập nhật CRM, phân công, follow-up và hoạt động hệ thống.">
      <template #action>
        <el-button
          v-if="canWrite"
          type="primary"
          :icon="Check"
          :disabled="notificationStore.summary.unreadCount === 0 || notificationStore.submitting"
          @click="markAllRead"
        >
          Đánh dấu tất cả đã đọc
        </el-button>
      </template>
    </CrmPageHeader>

    <NotificationSummary :summary="notificationStore.summary" :loading="notificationStore.loading" />

    <NotificationFilters
      :query="notificationStore.query"
      @change="notificationStore.setQuery"
      @reset="notificationStore.resetFilters"
    />

    <NotificationList
      :notifications="notificationStore.items"
      :loading="notificationStore.loading"
      :can-write="canWrite"
      @open="openNotification"
      @mark-read="markRead"
      @delete="deleteNotification"
    />

    <div class="pagination-row glass-card">
      <el-pagination
        background
        layout="prev, pager, next, sizes, total"
        :current-page="notificationStore.pagination.page"
        :page-size="notificationStore.pagination.pageSize"
        :total="notificationStore.pagination.totalCount"
        :page-sizes="[6, 10, 20, 50]"
        @current-change="(page: number) => notificationStore.setQuery({ page })"
        @size-change="(pageSize: number) => notificationStore.setQuery({ pageSize, page: 1 })"
      />
    </div>

    <NotificationDetailDrawer
      v-model="detailOpen"
      :notification="notificationStore.selectedNotification"
      :loading="notificationStore.detailLoading"
      :can-write="canWrite"
      @mark-read="markRead"
      @delete="deleteNotification"
      @open-link="openLink"
    />
  </div>
</template>

<style scoped>
.notification-page {
  gap: 16px;
}

.pagination-row {
  align-items: center;
  display: flex;
  justify-content: flex-end;
  padding: 12px 14px;
}

@media (max-width: 768px) {
  .pagination-row {
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
