<script setup lang="ts">
import { ChatDotRound, Calendar, Message, Phone, Refresh, User, Finished } from '@element-plus/icons-vue';
import { formatRelativeDate, getActivityTypeLabel } from '@/utils/crm';
import type { LeadActivity, LeadActivityType } from '@/types/crm/lead';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';

defineProps<{ activities: LeadActivity[] }>();

const iconMap: Record<LeadActivityType, typeof Phone> = {
  Call: Phone,
  Email: Message,
  Meeting: Calendar,
  Note: ChatDotRound,
  StatusChange: Refresh,
  Assigned: User,
  FollowUp: Calendar,
  Converted: Finished
};
</script>

<template>
  <section class="timeline glass-card">
    <div class="timeline__header">
      <h2>Hoạt động Lead</h2>
      <span>{{ activities.length }} ghi nhận</span>
    </div>

    <CrmEmptyState v-if="activities.length === 0" title="Chưa có hoạt động" description="Các cuộc gọi, email, ghi chú và thay đổi hệ thống sẽ xuất hiện tại đây." />

    <ol v-else class="timeline__list">
      <li
        v-for="activity in activities"
        :key="activity.id"
        class="timeline__item"
        :class="`timeline__item--${activity.activityType.toLowerCase()}`"
      >
        <div class="timeline__icon">
          <el-icon><component :is="iconMap[activity.activityType]" /></el-icon>
        </div>
        <div class="timeline__content">
          <div class="timeline__row">
            <strong>{{ getActivityTypeLabel(activity.activityType) }}</strong>
            <time>{{ formatRelativeDate(activity.createdAt) }}</time>
          </div>
          <p v-if="activity.description">{{ activity.description }}</p>
          <p v-if="activity.oldValue || activity.newValue" class="timeline__change">
            {{ activity.oldValue || 'Trống' }} → {{ activity.newValue || 'Trống' }}
          </p>
          <small>{{ activity.performedByName || 'Hệ thống' }}</small>
        </div>
      </li>
    </ol>
  </section>
</template>

<style scoped>
.timeline {
  overflow: hidden;
}

.timeline__header {
  align-items: center;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  justify-content: space-between;
  padding: 16px 18px;
}

h2 {
  color: var(--color-text-primary);
  font-size: 14px;
  margin: 0;
}

.timeline__header span,
small,
time {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 600;
}

.timeline__list {
  display: flex;
  flex-direction: column;
  gap: 0;
  list-style: none;
  margin: 0;
  padding: 8px 18px 18px;
}

.timeline__item {
  display: grid;
  gap: 12px;
  grid-template-columns: 30px 1fr;
  padding: 12px 0;
}

.timeline__icon {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 9px;
  color: var(--color-text-secondary);
  display: flex;
  height: 30px;
  justify-content: center;
  margin-top: 2px;
  width: 30px;
}

.timeline__content {
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  flex-direction: column;
  gap: 5px;
  min-width: 0;
  padding-bottom: 12px;
}

.timeline__row {
  align-items: center;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

strong {
  color: var(--color-text-primary);
}

p {
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
}

.timeline__change {
  color: var(--color-ai);
  font-family: var(--font-mono);
  font-size: 11px;
}

.timeline__item--call .timeline__icon,
.timeline__item--email .timeline__icon { color: var(--color-info); background: var(--color-info-bg); border-color: var(--color-info-border); }
.timeline__item--meeting .timeline__icon,
.timeline__item--followup .timeline__icon { color: var(--color-warning); background: var(--color-warning-bg); border-color: var(--color-warning-border); }
.timeline__item--assigned .timeline__icon { color: var(--color-ai); background: var(--color-ai-bg); border-color: var(--color-ai-border); }
.timeline__item--converted .timeline__icon { color: var(--color-success); background: var(--color-success-bg); border-color: var(--color-success-border); }
</style>
