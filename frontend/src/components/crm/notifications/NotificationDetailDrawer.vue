<script setup lang="ts">
import { computed } from 'vue';
import { Delete, Link, View } from '@element-plus/icons-vue';
import type { CrmNotification } from '@/types/crm/notification';
import { formatDateTime } from '@/utils/crm';
import {
  formatNotificationRelativeTime,
  getNotificationEventLabel,
  isFollowUpNotification,
  isFollowUpOverdue,
  parseNotificationData,
  resolveNotificationLink
} from '@/utils/notification';
import NotificationTypeBadge from './NotificationTypeBadge.vue';
import NotificationTypeIcon from './NotificationTypeIcon.vue';

const props = defineProps<{
  modelValue: boolean;
  notification: CrmNotification | null;
  canWrite: boolean;
  loading: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'markRead', notification: CrmNotification): void;
  (e: 'delete', notification: CrmNotification): void;
  (e: 'openLink', path: string): void;
}>();

const parsed = computed(() => parseNotificationData(props.notification?.data));
const parsedEntries = computed(() => Object.entries(parsed.value).map(([key, value]) => [key, typeof value === 'object' ? JSON.stringify(value) : String(value)]));
const targetPath = computed(() => resolveNotificationLink(props.notification?.link, parsed.value));
const hasInvalidJson = computed(() => Boolean(props.notification?.data) && Object.keys(parsed.value).length === 0);
</script>

<template>
  <el-drawer :model-value="modelValue" title="Chi tiết thông báo" size="420px" @update:model-value="emit('update:modelValue', $event)">
    <el-skeleton v-if="loading" :rows="8" animated />
    <div v-else-if="notification" class="detail">
      <div class="detail-head">
        <NotificationTypeIcon :type="notification.type" />
        <div>
          <h3>{{ notification.title }}</h3>
          <div class="detail-meta">
            <NotificationTypeBadge :type="notification.type" />
            <span>{{ getNotificationEventLabel(parsed) }}</span>
          </div>
        </div>
      </div>

      <p class="message">{{ notification.message }}</p>

      <div v-if="isFollowUpNotification(parsed)" class="follow-card" :class="{ 'follow-card--overdue': isFollowUpOverdue(parsed) }">
        <strong>{{ isFollowUpOverdue(parsed) ? 'Follow-up đã quá hạn' : 'Follow-up sắp đến hạn' }}</strong>
        <span class="numeric">{{ formatDateTime(String(parsed.scheduledFor)) }}</span>
      </div>

      <dl class="facts">
        <div>
          <dt>Trạng thái</dt>
          <dd>{{ notification.isRead ? 'Đã đọc' : 'Chưa đọc' }}</dd>
        </div>
        <div>
          <dt>Ngày tạo</dt>
          <dd class="numeric">{{ formatDateTime(notification.createdAt) }}</dd>
        </div>
        <div>
          <dt>Thời gian</dt>
          <dd>{{ formatNotificationRelativeTime(notification.createdAt) }}</dd>
        </div>
        <div>
          <dt>Đã đọc lúc</dt>
          <dd class="numeric">{{ formatDateTime(notification.readAt) }}</dd>
        </div>
      </dl>

      <section class="data-card">
        <h4>Dữ liệu liên quan</h4>
        <p v-if="hasInvalidJson" class="invalid-json">{{ notification.data }}</p>
        <dl v-else-if="parsedEntries.length" class="facts facts--compact">
          <div v-for="[key, value] in parsedEntries" :key="key">
            <dt>{{ key }}</dt>
            <dd>{{ value }}</dd>
          </div>
        </dl>
        <span v-else class="muted">Không có dữ liệu mở rộng.</span>
      </section>

      <el-collapse>
        <el-collapse-item title="Technical details" name="technical">
          <pre>{{ notification }}</pre>
        </el-collapse-item>
      </el-collapse>

      <div class="drawer-actions">
        <el-button :icon="Link" type="primary" @click="emit('openLink', targetPath)">Mở liên kết</el-button>
        <el-button v-if="canWrite && !notification.isRead" :icon="View" @click="emit('markRead', notification)">Đánh dấu đã đọc</el-button>
        <el-button v-if="canWrite" :icon="Delete" type="danger" @click="emit('delete', notification)">Xóa</el-button>
      </div>
    </div>
  </el-drawer>
</template>

<style scoped>
.detail {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.detail-head {
  align-items: flex-start;
  display: flex;
  gap: 12px;
}

h3,
h4 {
  color: var(--color-text-primary);
  margin: 0;
}

h3 {
  font-size: 18px;
  line-height: 1.35;
}

h4 {
  font-size: 13px;
}

.detail-meta {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 8px;
}

.detail-meta span,
.muted {
  color: var(--color-text-muted);
  font-size: 12px;
}

.message {
  color: var(--color-text-secondary);
  line-height: 1.65;
  margin: 0;
}

.follow-card,
.data-card {
  background: var(--color-warning-bg);
  border: 1px solid var(--color-warning-border);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
}

.follow-card--overdue {
  background: var(--color-danger-bg);
  border-color: var(--color-danger-border);
}

.data-card {
  background: var(--color-surface-glass);
  border-color: var(--color-border);
}

.facts {
  display: flex;
  flex-direction: column;
  gap: 9px;
  margin: 0;
}

.facts div {
  display: flex;
  justify-content: space-between;
  gap: 14px;
}

.facts--compact div {
  align-items: flex-start;
  border-bottom: 1px solid var(--color-divider);
  padding-bottom: 8px;
}

dt {
  color: var(--color-text-muted);
  font-size: 12px;
}

dd {
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 700;
  margin: 0;
  min-width: 0;
  overflow-wrap: anywhere;
  text-align: right;
}

.invalid-json {
  color: var(--color-danger);
  font-family: var(--font-mono);
  font-size: 12px;
  margin: 0;
  overflow-wrap: anywhere;
}

pre {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  color: var(--color-text-secondary);
  font-size: 11px;
  max-height: 260px;
  overflow: auto;
  padding: 10px;
}

.drawer-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>
