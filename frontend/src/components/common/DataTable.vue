<script setup lang="ts">
import EmptyState from './EmptyState.vue';

interface Column {
  prop: string;
  label: string;
  width?: string | number;
  minWidth?: string | number;
  sortable?: boolean;
  align?: 'left' | 'center' | 'right';
  mono?: boolean;
}

interface Props {
  columns: Column[];
  data: unknown[];
  loading?: boolean;
  selectable?: boolean;
  total?: number;
  page?: number;
  pageSize?: number;
}

withDefaults(defineProps<Props>(), {
  loading: false,
  selectable: false,
  total: 0,
  page: 1,
  pageSize: 20
});

const emit = defineEmits<{
  (e: 'selectionChange', selection: unknown[]): void;
  (e: 'pageChange', page: number): void;
  (e: 'sortChange', prop: string, order: 'ascending' | 'descending' | null): void;
}>();

function handleSortChange({ prop, order }: { prop: string; order: 'ascending' | 'descending' | null }) {
  emit('sortChange', prop, order);
}
</script>

<template>
  <div class="data-table-wrap">
    <el-table
      :data="loading ? [] : data"
      :highlight-current-row="false"
      @selection-change="emit('selectionChange', $event)"
      @sort-change="handleSortChange"
      stripe
      style="width: 100%"
    >
      <el-table-column v-if="selectable" type="selection" width="44" />

      <el-table-column
        v-for="col in columns"
        :key="col.prop"
        :prop="col.prop"
        :label="col.label"
        :width="col.width"
        :min-width="col.minWidth"
        :sortable="col.sortable ?? false"
        :align="col.align ?? 'left'"
        :class-name="col.mono ? 'cell-mono' : ''"
      >
        <template #default="{ row }">
          <slot :name="col.prop" :row="row" :value="(row as Record<string, unknown>)[col.prop]">
            <span :class="{ mono: col.mono }">
              {{ (row as Record<string, unknown>)[col.prop] }}
            </span>
          </slot>
        </template>
      </el-table-column>
    </el-table>

    <!-- Skeleton overlay -->
    <div v-if="loading" class="data-table-skeleton" aria-hidden="true">
      <div v-for="r in 5" :key="r" class="data-table-skeleton-row">
        <div v-for="c in columns.length" :key="c" class="data-table-skeleton-cell">
          <div class="skeleton" style="height: 14px; width: 100%" />
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <EmptyState
      v-if="!loading && data.length === 0"
      title="Không có dữ liệu"
      description="Chưa có bản ghi nào trong danh sách này."
    />

    <!-- Pagination -->
    <div v-if="total > 0" class="data-table-pagination">
      <el-pagination
        v-model:current-page="page"
        :page-size="pageSize"
        :total="total"
        layout="prev, pager, next"
        small
        background
        @current-change="emit('pageChange', $event)"
      />
    </div>
  </div>
</template>

<style scoped>
.data-table-wrap {
  position: relative;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  overflow: hidden;
}

.data-table-wrap :deep(.el-table) {
  --el-table-header-bg-color: var(--color-surface);
  --el-table-header-text-color: var(--color-text-secondary);
  --el-table-row-hover-bg-color: var(--color-primary-04);
  --el-table-border-color: var(--color-border);
}

.data-table-wrap :deep(.el-table th.el-table__cell) {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  border-bottom: 2px solid var(--color-border-strong);
  padding: 10px 16px;
}

.data-table-wrap :deep(.el-table .el-table__row) {
  height: 48px;
}

.data-table-wrap :deep(.el-table .el-table__cell) {
  padding: 0 16px;
  font-size: 14px;
  color: var(--color-text-primary);
}

.data-table-wrap :deep(.cell-mono .el-table__cell) {
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--color-text-secondary);
}

/* Skeleton overlay */
.data-table-skeleton {
  position: absolute;
  inset: 0;
  background: var(--color-surface);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.data-table-skeleton-row {
  display: flex;
  gap: 16px;
  padding: 0 16px;
  height: 48px;
  align-items: center;
}

.data-table-skeleton-cell {
  flex: 1;
}

/* Pagination */
.data-table-pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--color-border);
}
</style>
