<script setup lang="ts">
import { Calendar, Delete, Edit, UserFilled, View } from '@element-plus/icons-vue';
import { formatNullableText, formatVnd, getInitials } from '@/utils/crm';
import type { CrmLead } from '@/types/crm/lead';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import LeadFollowUpBadge from './LeadFollowUpBadge.vue';
import LeadPriorityBadge from './LeadPriorityBadge.vue';
import LeadScoreBadge from './LeadScoreBadge.vue';
import LeadStatusBadge from './LeadStatusBadge.vue';

defineProps<{
  leads: CrmLead[];
  loading: boolean;
}>();

defineEmits<{
  (e: 'open', lead: CrmLead): void;
  (e: 'edit', lead: CrmLead): void;
  (e: 'assign', lead: CrmLead): void;
  (e: 'followUp', lead: CrmLead): void;
  (e: 'delete', lead: CrmLead): void;
}>();
</script>

<template>
  <div class="lead-table glass-card">
    <el-table
      v-loading="loading"
      :data="leads"
      row-key="id"
      class="crm-el-table"
      @row-click="(row: CrmLead) => $emit('open', row)"
    >
      <el-table-column label="Lead" min-width="220">
        <template #default="{ row }">
          <div class="lead-cell">
            <div class="avatar">{{ getInitials(row.fullName) }}</div>
            <div>
              <strong>{{ row.fullName }}</strong>
              <span>{{ row.phone || row.email || 'Chưa có liên hệ' }}</span>
            </div>
          </div>
        </template>
      </el-table-column>
      <el-table-column label="Trạng thái" width="140">
        <template #default="{ row }"><LeadStatusBadge :status="row.status" /></template>
      </el-table-column>
      <el-table-column label="Ưu tiên" width="130">
        <template #default="{ row }"><LeadPriorityBadge :priority="row.priority" /></template>
      </el-table-column>
      <el-table-column label="Score" width="130">
        <template #default="{ row }"><LeadScoreBadge :score="row.score" /></template>
      </el-table-column>
      <el-table-column label="Nhu cầu" min-width="230">
        <template #default="{ row }">
          <div class="stack">
            <strong>{{ formatNullableText(row.requirements) }}</strong>
            <span>{{ formatNullableText(row.preferredArea) }} · {{ formatNullableText(row.preferredType) }}</span>
          </div>
        </template>
      </el-table-column>
      <el-table-column label="Ngân sách" width="150">
        <template #default="{ row }"><span class="numeric">{{ formatVnd(row.budget) }}</span></template>
      </el-table-column>
      <el-table-column label="Nguồn" width="110" prop="sourceChannel" />
      <el-table-column label="Người phụ trách" min-width="160">
        <template #default="{ row }">
          <div class="assignee">
            <span class="mini-avatar">{{ getInitials(row.assignedToName) }}</span>
            <span>{{ row.assignedToName || 'Chưa phân công' }}</span>
          </div>
        </template>
      </el-table-column>
      <el-table-column label="Follow-up" min-width="180">
        <template #default="{ row }"><LeadFollowUpBadge :date="row.nextFollowUpAt" :status="row.status" /></template>
      </el-table-column>
      <el-table-column label="Thao tác" fixed="right" width="170">
        <template #default="{ row }">
          <div class="actions" @click.stop>
            <el-tooltip content="Xem chi tiết">
              <el-button circle :icon="View" @click="$emit('open', row)" />
            </el-tooltip>
            <el-tooltip content="Sửa">
              <el-button circle :icon="Edit" @click="$emit('edit', row)" />
            </el-tooltip>
            <el-tooltip content="Phân công">
              <el-button circle :icon="UserFilled" @click="$emit('assign', row)" />
            </el-tooltip>
            <el-tooltip content="Đặt follow-up">
              <el-button circle :icon="Calendar" @click="$emit('followUp', row)" />
            </el-tooltip>
            <el-tooltip content="Xóa">
              <el-button circle type="danger" :icon="Delete" @click="$emit('delete', row)" />
            </el-tooltip>
          </div>
        </template>
      </el-table-column>
      <template #empty>
        <CrmEmptyState title="Không có Lead phù hợp" description="Thử đổi bộ lọc hoặc thêm Lead mới để bắt đầu." />
      </template>
    </el-table>
  </div>
</template>

<style scoped>
.lead-table {
  overflow: hidden;
}

.lead-cell,
.assignee,
.actions {
  align-items: center;
  display: flex;
  gap: 9px;
}

.avatar,
.mini-avatar {
  align-items: center;
  background: var(--color-yellow);
  border-radius: 10px;
  color: var(--color-yellow-text);
  display: flex;
  flex-shrink: 0;
  font-weight: 800;
  height: 34px;
  justify-content: center;
  width: 34px;
}

.mini-avatar {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  height: 26px;
  width: 26px;
}

.lead-cell > div,
.stack {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

strong {
  color: var(--color-text-primary);
  font-size: 12px;
}

span {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.actions {
  gap: 4px;
}
</style>
