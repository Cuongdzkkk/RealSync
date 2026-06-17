<script setup lang="ts">
import { Delete, View } from '@element-plus/icons-vue';
import type { CrmNotification } from '@/types/crm/notification';
import { formatDateTime } from '@/utils/crm';
import {
  formatNotificationRelativeTime,
  getNotificationEventLabel,
  isFollowUpNotification,
  isFollowUpOverdue,
  parseNotificationData
} from '@/utils/notification';
import NotificationTypeBadge from './NotificationTypeBadge.vue';
import NotificationTypeIcon from './NotificationTypeIcon.vue';

const props = withDefaults(defineProps<{
  notification: CrmNotification;
  mode?: 'compact' | 'full';
  canWrite?: boolean;
}>(), {
  mode: 'full',
  canWrite: true
});

defineEmits<{
  (e: 'open', notification: CrmNotification): void;
  (e: 'markRead', notification: CrmNotification): void;
  (e: 'delete', notification: CrmNotification): void;
}>();

const parsed = parseNotificationData(props.notification.data);
</script>

<template>
  <article
    class="notification-item"
    :class="{
      'notification-item--unread': !notification.isRead,
      'notification-item--compact': mode === 'compact'
    }"
    role="button"
    tabindex="0"
    @click="$emit('open', notification)"
    @keyup.enter="$emit('open', notification)"
  >
    <NotificationTypeIcon :type="notification.type" :size="mode === 'compact' ? 'sm' : 'md'" />

    <div class="notification-item__body">
      <div class="notification-item__topline">
        <strong>{{ notification.title }}</strong>
        <span class="time">{{ formatNotificationRelativeTime(notification.createdAt) }}</span>
      </div>
      <p>{{ notification.message }}</p>
      <div class="notification-item__meta">
        <NotificationTypeBadge v-if="mode === 'full'" :type="notification.type" />
        <span>{{ getNotificationEventLabel(parsed) }}</span>
        <span v-if="isFollowUpNotification(parsed)" class="follow-up" :class="{ 'follow-up--overdue': isFollowUpOverdue(parsed) }">
          {{ isFollowUpOverdue(parsed) ? 'Quá hạn' : 'Đến hạn' }}
        </span>
        <span v-if="mode === 'full'" class="numeric">{{ formatDateTime(notification.createdAt) }}</span>
      </div>
    </div>

    <div v-if="mode === 'full'" class="notification-item__actions" @click.stop>
      <el-tooltip v-if="canWrite && !notification.isRead" content="Đánh dấu đã đọc">
        <el-button circle :icon="View" @click="$emit('markRead', notification)" />
      </el-tooltip>
      <el-tooltip v-if="canWrite" content="Xóa">
        <el-button circle type="danger" :icon="Delete" @click="$emit('delete', notification)" />
      </el-tooltip>
    </div>
  </article>
</template>

<style scoped>
.notification-item {
  align-items: flex-start;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  cursor: pointer;
  display: grid;
  gap: 12px;
  grid-template-columns: auto minmax(0, 1fr) auto;
  padding: 12px;
  transition: border-color var(--duration-fast), background-color var(--duration-fast), transform var(--duration-fast);
}

.notification-item:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-strong);
}

.notification-item--compact {
  border: 0;
  border-bottom: 1px solid var(--color-divider);
  border-radius: 0;
  grid-template-columns: auto minmax(0, 1fr);
  padding: 11px 12px;
}

.notification-item--unread {
  box-shadow: inset 3px 0 0 var(--color-yellow);
}

.notification-item__body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.notification-item__topline {
  align-items: flex-start;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

strong {
  color: var(--color-text-primary);
  font-size: 13px;
  line-height: 1.35;
}

p {
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 1.55;
  margin: 0;
}

.time {
  color: var(--color-text-muted);
  flex-shrink: 0;
  font-size: 10.5px;
  white-space: nowrap;
}

.notification-item__meta {
  align-items: center;
  color: var(--color-text-muted);
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
  font-size: 11px;
}

.follow-up {
  border-radius: 5px;
  background: var(--color-warning-bg);
  color: var(--color-warning);
  font-weight: 800;
  padding: 2px 6px;
}

.follow-up--overdue {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.notification-item__actions {
  align-items: center;
  display: flex;
  gap: 4px;
}

@media (max-width: 640px) {
  .notification-item {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .notification-item__actions {
    grid-column: 2;
  }
}
</style>
