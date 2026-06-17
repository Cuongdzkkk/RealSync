<script setup lang="ts">
import type { CrmNotification } from '@/types/crm/notification';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import NotificationItem from './NotificationItem.vue';

defineProps<{
  notifications: CrmNotification[];
  loading: boolean;
  canWrite: boolean;
}>();

defineEmits<{
  (e: 'open', notification: CrmNotification): void;
  (e: 'markRead', notification: CrmNotification): void;
  (e: 'delete', notification: CrmNotification): void;
}>();
</script>

<template>
  <section class="notification-list glass-card">
    <el-skeleton v-if="loading" :rows="8" animated />
    <CrmEmptyState v-else-if="notifications.length === 0" title="Không có thông báo phù hợp" description="Thử đổi bộ lọc hoặc reset danh sách tìm kiếm." />
    <div v-else class="notification-list__items">
      <NotificationItem
        v-for="notification in notifications"
        :key="notification.id"
        :notification="notification"
        :can-write="canWrite"
        @open="$emit('open', notification)"
        @mark-read="$emit('markRead', notification)"
        @delete="$emit('delete', notification)"
      />
    </div>
  </section>
</template>

<style scoped>
.notification-list {
  padding: 12px;
}

.notification-list__items {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
</style>
