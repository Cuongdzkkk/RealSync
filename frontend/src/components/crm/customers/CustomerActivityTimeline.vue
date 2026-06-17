<script setup lang="ts">
import { Download, Edit, Hide, Plus, Refresh, User, View } from '@element-plus/icons-vue';
import { formatRelativeDate } from '@/utils/crm';
import type { CustomerActivityAction, CustomerActivityLog } from '@/types/crm/customer';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';

defineProps<{ activities: CustomerActivityLog[] }>();

const iconMap: Record<CustomerActivityAction, typeof Plus> = {
  Create: Plus,
  Update: Edit,
  Delete: Hide,
  Assignment: User,
  StatusChange: Refresh,
  View,
  Export: Download
};

function safeFormat(value?: string | null): string {
  if (!value) return '';
  try {
    const parsed = JSON.parse(value) as Record<string, unknown>;
    return Object.entries(parsed)
      .map(([key, entry]) => `${key}: ${String(entry)}`)
      .join(', ');
  } catch {
    return value;
  }
}
</script>

<template>
  <section class="timeline glass-card">
    <div class="timeline-header">
      <h2>Lịch sử hoạt động</h2>
      <span>{{ activities.length }} hoạt động</span>
    </div>
    <CrmEmptyState v-if="activities.length === 0" title="Chưa có lịch sử" description="Customer activity history là read-only trong phase này." />
    <ol v-else>
      <li v-for="activity in activities" :key="activity.id" :class="`action-${activity.action.toLowerCase()}`">
        <div class="activity-icon">
          <el-icon><component :is="iconMap[activity.action]" /></el-icon>
        </div>
        <div class="activity-body">
          <div class="activity-row">
            <strong>{{ activity.action }}</strong>
            <time>{{ formatRelativeDate(activity.createdAt) }}</time>
          </div>
          <p v-if="activity.description">{{ activity.description }}</p>
          <p v-if="activity.oldValues" class="activity-values">Old: {{ safeFormat(activity.oldValues) }}</p>
          <p v-if="activity.newValues" class="activity-values">New: {{ safeFormat(activity.newValues) }}</p>
          <small>{{ activity.userName || 'Hệ thống' }}</small>
        </div>
      </li>
    </ol>
  </section>
</template>

<style scoped>
.timeline {
  overflow: hidden;
}

.timeline-header {
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

.timeline-header span,
time,
small {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 600;
}

ol {
  list-style: none;
  margin: 0;
  padding: 8px 18px 18px;
}

li {
  display: grid;
  gap: 12px;
  grid-template-columns: 30px 1fr;
  padding: 12px 0;
}

.activity-icon {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 9px;
  color: var(--color-text-secondary);
  display: flex;
  height: 30px;
  justify-content: center;
  width: 30px;
}

.activity-body {
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  flex-direction: column;
  gap: 5px;
  padding-bottom: 12px;
}

.activity-row {
  align-items: center;
  display: flex;
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

.activity-values {
  color: var(--color-ai);
  font-family: var(--font-mono);
  font-size: 11px;
}

.action-create .activity-icon { color: var(--color-success); background: var(--color-success-bg); border-color: var(--color-success-border); }
.action-update .activity-icon,
.action-statuschange .activity-icon,
.action-view .activity-icon { color: var(--color-info); background: var(--color-info-bg); border-color: var(--color-info-border); }
.action-assignment .activity-icon { color: var(--color-ai); background: var(--color-ai-bg); border-color: var(--color-ai-border); }
.action-delete .activity-icon { color: var(--color-danger); background: var(--color-danger-bg); border-color: var(--color-danger-border); }
.action-export .activity-icon { color: var(--color-warning); background: var(--color-warning-bg); border-color: var(--color-warning-border); }
</style>
