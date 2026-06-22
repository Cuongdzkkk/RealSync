<script setup lang="ts">
import { ArrowLeft, Edit } from '@element-plus/icons-vue';
import { formatDateTime, getInitials } from '@/utils/crm';
import type { CrmLead } from '@/types/crm/lead';
import LeadFollowUpBadge from './LeadFollowUpBadge.vue';
import LeadPriorityBadge from './LeadPriorityBadge.vue';
import LeadScoreBadge from './LeadScoreBadge.vue';
import LeadStatusBadge from './LeadStatusBadge.vue';

defineProps<{ lead: CrmLead }>();
defineEmits<{
  (e: 'back'): void;
  (e: 'edit'): void;
}>();
</script>

<template>
  <section class="profile glass-card">
    <button class="icon-btn" aria-label="Quay lại danh sách Lead" @click="$emit('back')">
      <el-icon><ArrowLeft /></el-icon>
    </button>
    <div class="avatar">{{ getInitials(lead.fullName) }}</div>
    <div class="profile__main">
      <div class="profile__title">
        <h1>{{ lead.fullName }}</h1>
        <LeadStatusBadge :status="lead.status" />
        <LeadPriorityBadge :priority="lead.priority" />
        <LeadScoreBadge :score="lead.score" />
      </div>
      <div class="profile__meta">
        <span>{{ lead.phone || 'Chưa có SĐT' }}</span>
        <span>{{ lead.email || 'Chưa có email' }}</span>
        <span>{{ lead.assignedToName || 'Chưa phân công' }}</span>
        <span>Tạo {{ formatDateTime(lead.createdAt) }}</span>
      </div>
      <LeadFollowUpBadge :date="lead.nextFollowUpAt" :status="lead.status" />
    </div>
    <el-button :icon="Edit" @click="$emit('edit')">Chỉnh sửa</el-button>
  </section>
</template>

<style scoped>
.profile {
  align-items: center;
  display: flex;
  gap: 14px;
  padding: 16px;
}

.icon-btn {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  color: var(--color-text-secondary);
  cursor: pointer;
  display: flex;
  height: 34px;
  justify-content: center;
  width: 34px;
}

.avatar {
  align-items: center;
  background: var(--color-yellow);
  border-radius: 12px;
  color: var(--color-yellow-text);
  display: flex;
  flex-shrink: 0;
  font-size: 16px;
  font-weight: 800;
  height: 50px;
  justify-content: center;
  width: 50px;
}

.profile__main {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 9px;
  min-width: 0;
}

.profile__title,
.profile__meta {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

h1 {
  color: var(--color-text-primary);
  font-size: 20px;
  margin: 0 8px 0 0;
}

.profile__meta {
  color: var(--color-text-secondary);
  font-size: 12px;
}

@media (max-width: 768px) {
  .profile {
    align-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
