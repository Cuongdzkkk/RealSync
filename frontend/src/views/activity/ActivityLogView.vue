<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { Refresh, Search } from '@element-plus/icons-vue';
import CrmPageHeader from '@/components/crm/common/CrmPageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { useActivityLogStore } from '@/stores/useActivityLogStore';
import type { ActivityAction, ActivityLog } from '@/types/crm/activity';
import { ACTIVITY_ACTIONS } from '@/types/crm/activity';
import type { StatusVariant } from '@/types/common';

const activityStore = useActivityLogStore();

const actionVariant: Record<ActivityAction, StatusVariant> = {
  Login: 'info',
  Create: 'success',
  Update: 'warning',
  Delete: 'danger',
  StatusChange: 'warning',
  Assignment: 'ai',
  View: 'muted',
  Export: 'info'
};

const entityTypes = computed(() => {
  const values = new Set(activityStore.items.map((item) => item.entityType).filter(Boolean));
  return Array.from(values).sort();
});

onMounted(() => activityStore.fetchActivityLogs());

watch(
  () => activityStore.query,
  () => activityStore.fetchActivityLogs(),
  { deep: true }
);

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(value));
}

function shortId(value?: string | null) {
  if (!value) return '-';
  return value.length > 12 ? `${value.slice(0, 8)}...${value.slice(-4)}` : value;
}

function userLabel(row: ActivityLog) {
  return row.userName || row.userEmail || 'System';
}

function hasPayload(row: ActivityLog) {
  return Boolean(row.oldValues || row.newValues || row.userAgent);
}

function resetFilters() {
  activityStore.resetFilters();
}
</script>

<template>
  <div class="page activity-log-page">
    <CrmPageHeader title="Activity Log" subtitle="Audit trail">
      <template #action>
        <el-button :icon="Refresh" :loading="activityStore.loading" @click="activityStore.fetchActivityLogs">
          Làm mới
        </el-button>
      </template>
    </CrmPageHeader>

    <section class="filter-panel glass-card">
      <div class="filter-grid">
        <el-input
          :model-value="activityStore.query.search"
          clearable
          placeholder="Tìm user, email, mô tả"
          :prefix-icon="Search"
          @update:model-value="(search: string) => activityStore.setQuery({ search })"
        />

        <el-select
          :model-value="activityStore.query.action"
          clearable
          placeholder="Action"
          @update:model-value="(action: ActivityAction | null) => activityStore.setQuery({ action })"
        >
          <el-option v-for="action in ACTIVITY_ACTIONS" :key="action" :label="action" :value="action" />
        </el-select>

        <el-select
          :model-value="activityStore.query.entityType"
          clearable
          filterable
          allow-create
          placeholder="Đối tượng"
          @update:model-value="(entityType: string | null) => activityStore.setQuery({ entityType })"
        >
          <el-option v-for="entityType in entityTypes" :key="entityType" :label="entityType" :value="entityType" />
        </el-select>

        <el-input
          :model-value="activityStore.query.userId"
          clearable
          placeholder="User ID"
          @update:model-value="(userId: string) => activityStore.setQuery({ userId })"
        />

        <el-date-picker
          :model-value="activityStore.query.fromDate"
          type="datetime"
          value-format="YYYY-MM-DDTHH:mm:ss"
          placeholder="Từ ngày"
          @update:model-value="(fromDate: string | null) => activityStore.setQuery({ fromDate })"
        />

        <el-date-picker
          :model-value="activityStore.query.toDate"
          type="datetime"
          value-format="YYYY-MM-DDTHH:mm:ss"
          placeholder="Đến ngày"
          @update:model-value="(toDate: string | null) => activityStore.setQuery({ toDate })"
        />

        <el-button @click="resetFilters">Xóa lọc</el-button>
      </div>
    </section>

    <section class="activity-table glass-card">
      <el-table
        v-loading="activityStore.loading"
        :data="activityStore.items"
        row-key="id"
        class="activity-table__inner"
      >
        <el-table-column type="expand" width="42">
          <template #default="{ row }">
            <div v-if="hasPayload(row)" class="payload-grid">
              <div v-if="row.oldValues" class="payload-block">
                <span>Old values</span>
                <pre>{{ row.oldValues }}</pre>
              </div>
              <div v-if="row.newValues" class="payload-block">
                <span>New values</span>
                <pre>{{ row.newValues }}</pre>
              </div>
              <div v-if="row.userAgent" class="payload-block payload-block--wide">
                <span>User agent</span>
                <pre>{{ row.userAgent }}</pre>
              </div>
            </div>
            <div v-else class="payload-empty">Không có payload chi tiết.</div>
          </template>
        </el-table-column>

        <el-table-column label="Thời gian" min-width="150">
          <template #default="{ row }">
            <span class="numeric">{{ formatDateTime(row.createdAt) }}</span>
          </template>
        </el-table-column>

        <el-table-column label="User" min-width="190">
          <template #default="{ row }">
            <div class="user-cell">
              <span>{{ userLabel(row) }}</span>
              <small>{{ row.userRole || row.userEmail || '-' }}</small>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="Action" width="130">
          <template #default="{ row }">
            <StatusBadge :label="row.action" :variant="actionVariant[row.action as ActivityAction] ?? 'muted'" />
          </template>
        </el-table-column>

        <el-table-column label="Đối tượng" min-width="170">
          <template #default="{ row }">
            <div class="entity-cell">
              <span>{{ row.entityType }}</span>
              <small class="mono">{{ shortId(row.entityId) }}</small>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="description" label="Mô tả" min-width="260" show-overflow-tooltip />

        <el-table-column label="IP" width="140">
          <template #default="{ row }">
            <span class="mono">{{ row.ipAddress || '-' }}</span>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-row">
        <el-pagination
          background
          layout="prev, pager, next, sizes, total"
          :current-page="activityStore.pagination.page"
          :page-size="activityStore.pagination.pageSize"
          :total="activityStore.pagination.totalCount"
          :page-sizes="[10, 20, 50, 100]"
          @current-change="(page: number) => activityStore.setQuery({ page })"
          @size-change="(pageSize: number) => activityStore.setQuery({ pageSize, page: 1 })"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.activity-log-page {
  gap: 16px;
}

.filter-panel {
  padding: 14px;
}

.filter-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(220px, 1.4fr) repeat(2, minmax(150px, 1fr)) minmax(210px, 1.2fr) repeat(2, minmax(170px, 1fr)) auto;
}

.activity-table {
  overflow: hidden;
}

.activity-table__inner {
  width: 100%;
}

.user-cell,
.entity-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.user-cell span,
.entity-cell span {
  color: var(--color-text-primary);
  font-weight: 600;
}

.user-cell small,
.entity-cell small {
  color: var(--color-text-muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.payload-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  padding: 12px 18px;
}

.payload-block {
  min-width: 0;
}

.payload-block--wide {
  grid-column: 1 / -1;
}

.payload-block span {
  color: var(--color-text-muted);
  display: block;
  font-size: 11px;
  font-weight: 700;
  margin-bottom: 6px;
  text-transform: uppercase;
}

pre {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  color: var(--color-text-secondary);
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.5;
  margin: 0;
  max-height: 180px;
  overflow: auto;
  padding: 10px;
  white-space: pre-wrap;
  word-break: break-word;
}

.payload-empty {
  color: var(--color-text-muted);
  font-size: 12px;
  padding: 14px 18px;
}

.pagination-row {
  align-items: center;
  border-top: 1px solid var(--color-divider);
  display: flex;
  justify-content: flex-end;
  padding: 12px 14px;
}

@media (max-width: 1180px) {
  .filter-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .filter-grid,
  .payload-grid {
    grid-template-columns: 1fr;
  }

  .pagination-row {
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
