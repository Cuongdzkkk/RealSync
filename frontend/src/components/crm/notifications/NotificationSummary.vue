<script setup lang="ts">
import type { NotificationSummary as Summary } from '@/types/crm/notification';

defineProps<{
  summary: Summary;
  loading: boolean;
}>();
</script>

<template>
  <section class="notification-summary">
    <div class="summary-card glass-card">
      <span>Tổng thông báo</span>
      <strong class="numeric">{{ loading ? '--' : summary.totalCount }}</strong>
    </div>
    <div class="summary-card glass-card summary-card--unread">
      <span>Chưa đọc</span>
      <strong class="numeric">{{ loading ? '--' : summary.unreadCount }}</strong>
    </div>
    <div class="summary-card glass-card">
      <span>Đã đọc</span>
      <strong class="numeric">{{ loading ? '--' : summary.readCount }}</strong>
    </div>
  </section>
</template>

<style scoped>
.notification-summary {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.summary-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 14px;
}

.summary-card span {
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 700;
}

.summary-card strong {
  color: var(--color-text-primary);
  font-size: 26px;
  line-height: 1;
}

.summary-card--unread {
  box-shadow: inset 3px 0 0 var(--color-yellow);
}

@media (max-width: 680px) {
  .notification-summary {
    grid-template-columns: 1fr;
  }
}
</style>
