<script setup lang="ts">
import { MoreFilled } from '@element-plus/icons-vue';
import { formatRelativeDate, formatVnd, getInitials, getLeadStatusLabel } from '@/utils/crm';
import type { CrmLead, LeadStatus } from '@/types/crm/lead';
import { LEAD_STATUSES } from '@/types/crm/lead';
import LeadFollowUpBadge from './LeadFollowUpBadge.vue';
import LeadPriorityBadge from './LeadPriorityBadge.vue';
import LeadScoreBadge from './LeadScoreBadge.vue';

defineProps<{ lead: CrmLead }>();
defineEmits<{
  (e: 'open', lead: CrmLead): void;
  (e: 'status', lead: CrmLead, status: LeadStatus): void;
}>();
</script>

<template>
  <article class="lead-card" role="button" tabindex="0" @click="$emit('open', lead)" @keydown.enter="$emit('open', lead)">
    <div class="lead-card__top">
      <div class="avatar">{{ getInitials(lead.fullName) }}</div>
      <div class="lead-card__identity">
        <strong>{{ lead.fullName }}</strong>
        <span>{{ lead.phone || lead.email || 'Chưa có liên hệ' }}</span>
      </div>
      <el-dropdown trigger="click" @click.stop>
        <button class="icon-btn" aria-label="Đổi trạng thái Lead">
          <el-icon><MoreFilled /></el-icon>
        </button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item
              v-for="status in LEAD_STATUSES"
              :key="status"
              :disabled="status === lead.status"
              @click="$emit('status', lead, status)"
            >
              {{ getLeadStatusLabel(status) }}
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
    <p>{{ lead.requirements || 'Chưa cập nhật nhu cầu' }}</p>
    <div class="lead-card__chips">
      <LeadScoreBadge :score="lead.score" />
      <LeadPriorityBadge :priority="lead.priority" />
    </div>
    <div class="lead-card__meta">
      <span class="numeric">{{ formatVnd(lead.budget) }}</span>
      <span>{{ lead.assignedToName || 'Chưa phân công' }}</span>
      <span>{{ lead.sourceChannel || 'Không rõ nguồn' }}</span>
      <span>{{ formatRelativeDate(lead.createdAt) }}</span>
    </div>
    <LeadFollowUpBadge :date="lead.nextFollowUpAt" :status="lead.status" />
  </article>
</template>

<style scoped>
.lead-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 12px;
  transition: all var(--duration-fast);
}

.lead-card:hover {
  border-color: var(--color-border-strong);
  box-shadow: var(--elevation-raised);
  transform: translateY(-1px);
}

.lead-card__top {
  align-items: center;
  display: flex;
  gap: 10px;
}

.avatar {
  align-items: center;
  background: var(--color-yellow);
  border-radius: 9px;
  color: var(--color-yellow-text);
  display: flex;
  flex-shrink: 0;
  font-weight: 800;
  height: 34px;
  justify-content: center;
  width: 34px;
}

.lead-card__identity {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

strong {
  color: var(--color-text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

span,
p {
  color: var(--color-text-secondary);
  font-size: 12px;
}

p {
  line-height: 1.5;
  margin: 0;
}

.lead-card__chips,
.lead-card__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
}

.lead-card__meta span {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 7px;
  padding: 5px 7px;
}

.icon-btn {
  align-items: center;
  background: transparent;
  border: 1px solid transparent;
  border-radius: 7px;
  color: var(--color-text-muted);
  cursor: pointer;
  display: flex;
  height: 28px;
  justify-content: center;
  width: 28px;
}
</style>
