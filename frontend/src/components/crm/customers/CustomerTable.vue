<script setup lang="ts">
import { Delete, Edit, UserFilled, View } from '@element-plus/icons-vue';
import { formatDateTime, getInitials } from '@/utils/crm';
import type { CrmCustomerDetail } from '@/types/crm/customer';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import CustomerSourceBadge from './CustomerSourceBadge.vue';

defineProps<{
  customers: CrmCustomerDetail[];
  loading: boolean;
  canWrite: boolean;
  canDelete: boolean;
}>();

defineEmits<{
  (e: 'open', customer: CrmCustomerDetail): void;
  (e: 'edit', customer: CrmCustomerDetail): void;
  (e: 'assign', customer: CrmCustomerDetail): void;
  (e: 'delete', customer: CrmCustomerDetail): void;
  (e: 'openLead', leadId: string): void;
}>();
</script>

<template>
  <div class="customer-table glass-card">
    <el-table
      v-loading="loading"
      :data="customers"
      row-key="id"
      class="crm-el-table"
      @row-click="(row: CrmCustomerDetail) => $emit('open', row)"
    >
      <el-table-column label="Khách hàng" min-width="230">
        <template #default="{ row }">
          <div class="identity">
            <span class="avatar">{{ getInitials(row.fullName) }}</span>
            <div>
              <strong>{{ row.fullName }}</strong>
              <small>{{ row.phone || row.email || 'Chưa có liên hệ' }}</small>
            </div>
          </div>
        </template>
      </el-table-column>
      <el-table-column label="Công ty" min-width="150">
        <template #default="{ row }">{{ row.company || 'Cá nhân' }}</template>
      </el-table-column>
      <el-table-column label="Nguồn" width="150">
        <template #default="{ row }"><CustomerSourceBadge :source="row.source" /></template>
      </el-table-column>
      <el-table-column label="Người phụ trách" min-width="170">
        <template #default="{ row }">
          <div class="assignee">
            <span class="mini-avatar">{{ getInitials(row.assignedToName) }}</span>
            <span>{{ row.assignedToName || 'Chưa phân công' }}</span>
          </div>
        </template>
      </el-table-column>
      <el-table-column label="Nguồn chuyển đổi" min-width="190">
        <template #default="{ row }">
          <button v-if="row.convertedFromLeadId" class="lead-link" @click.stop="$emit('openLead', row.convertedFromLeadId)">
            {{ row.convertedFromLeadName }}
          </button>
          <span v-else class="muted">Khách hàng trực tiếp</span>
        </template>
      </el-table-column>
      <el-table-column label="Ngày tạo" width="160">
        <template #default="{ row }"><span class="numeric">{{ formatDateTime(row.createdAt) }}</span></template>
      </el-table-column>
      <el-table-column label="Thao tác" fixed="right" width="150">
        <template #default="{ row }">
          <div class="actions" @click.stop>
            <el-tooltip content="Xem chi tiết">
              <el-button circle :icon="View" @click="$emit('open', row)" />
            </el-tooltip>
            <el-tooltip v-if="canWrite" content="Sửa">
              <el-button circle :icon="Edit" @click="$emit('edit', row)" />
            </el-tooltip>
            <el-tooltip v-if="canWrite" content="Phân công">
              <el-button circle :icon="UserFilled" @click="$emit('assign', row)" />
            </el-tooltip>
            <el-tooltip v-if="canDelete" content="Xóa">
              <el-button circle type="danger" :icon="Delete" @click="$emit('delete', row)" />
            </el-tooltip>
          </div>
        </template>
      </el-table-column>
      <template #empty>
        <CrmEmptyState title="Không có khách hàng phù hợp" description="Thử đổi bộ lọc hoặc thêm khách hàng mới." />
      </template>
    </el-table>
  </div>
</template>

<style scoped>
.customer-table {
  overflow: hidden;
}

.identity,
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

.identity > div {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

strong {
  color: var(--color-text-primary);
  font-size: 12px;
}

small,
.muted,
.assignee span {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.lead-link {
  background: transparent;
  border: 0;
  color: var(--color-ai);
  cursor: pointer;
  font-weight: 700;
  padding: 0;
  text-align: left;
}

.actions {
  gap: 4px;
}
</style>
