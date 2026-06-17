<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { Filter, Refresh } from '@element-plus/icons-vue';
import { NOTIFICATION_TYPES, type NotificationQuery, type NotificationReadFilter } from '@/types/crm/notification';
import { getNotificationTypeLabel } from '@/utils/notification';

const props = defineProps<{ query: NotificationQuery }>();
const emit = defineEmits<{
  (e: 'change', query: Partial<NotificationQuery>): void;
  (e: 'reset'): void;
}>();

const drawerOpen = ref(false);
const local = reactive({
  search: props.query.search,
  readFilter: props.query.readFilter ?? 'all' as NotificationReadFilter,
  type: props.query.type,
  dateRange: props.query.fromDate && props.query.toDate ? [props.query.fromDate, props.query.toDate] : [],
  sortBy: props.query.sortBy ?? 'createdAt',
  sortDirection: props.query.sortDirection ?? 'desc'
});

let searchTimer: number | undefined;
watch(
  () => local.search,
  (value) => {
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(() => emit('change', { search: value, page: 1 }), 300);
  }
);

watch(
  () => [local.readFilter, local.type, local.dateRange, local.sortBy, local.sortDirection],
  () => {
    emit('change', {
      readFilter: local.readFilter,
      type: local.type,
      fromDate: local.dateRange[0] ?? null,
      toDate: local.dateRange[1] ?? null,
      sortBy: local.sortBy,
      sortDirection: local.sortDirection,
      page: 1
    });
  },
  { deep: true }
);

watch(
  () => props.query,
  (query) => {
    local.search = query.search;
    local.readFilter = query.readFilter ?? 'all';
    local.type = query.type;
    local.dateRange = query.fromDate && query.toDate ? [query.fromDate, query.toDate] : [];
    local.sortBy = query.sortBy ?? 'createdAt';
    local.sortDirection = query.sortDirection ?? 'desc';
  },
  { deep: true }
);
</script>

<template>
  <section class="notification-filters glass-card">
    <div class="filters-desktop">
      <el-input v-model="local.search" clearable placeholder="Tìm tiêu đề, nội dung, dữ liệu" />
      <el-segmented
        v-model="local.readFilter"
        :options="[
          { label: 'Tất cả', value: 'all' },
          { label: 'Chưa đọc', value: 'unread' },
          { label: 'Đã đọc', value: 'read' }
        ]"
      />
      <el-select v-model="local.type" clearable placeholder="Loại">
        <el-option v-for="type in NOTIFICATION_TYPES" :key="type" :label="getNotificationTypeLabel(type)" :value="type" />
      </el-select>
      <el-date-picker v-model="local.dateRange" type="daterange" range-separator="-" start-placeholder="Từ ngày" end-placeholder="Đến ngày" value-format="YYYY-MM-DDTHH:mm:ss" />
      <el-select v-model="local.sortBy" placeholder="Sort">
        <el-option label="Ngày tạo" value="createdAt" />
        <el-option label="Ngày đọc" value="readAt" />
        <el-option label="Loại" value="type" />
        <el-option label="Tiêu đề" value="title" />
      </el-select>
      <el-select v-model="local.sortDirection" placeholder="Thứ tự">
        <el-option label="Giảm dần" value="desc" />
        <el-option label="Tăng dần" value="asc" />
      </el-select>
      <el-button :icon="Refresh" @click="emit('reset')">Reset</el-button>
    </div>

    <div class="filters-mobile">
      <el-input v-model="local.search" clearable placeholder="Tìm thông báo" />
      <el-button :icon="Filter" @click="drawerOpen = true">Bộ lọc</el-button>
    </div>

    <el-drawer v-model="drawerOpen" title="Bộ lọc thông báo" size="100%">
      <div class="drawer-filters">
        <el-segmented
          v-model="local.readFilter"
          :options="[
            { label: 'Tất cả', value: 'all' },
            { label: 'Chưa đọc', value: 'unread' },
            { label: 'Đã đọc', value: 'read' }
          ]"
        />
        <el-select v-model="local.type" clearable placeholder="Loại">
          <el-option v-for="type in NOTIFICATION_TYPES" :key="type" :label="getNotificationTypeLabel(type)" :value="type" />
        </el-select>
        <el-date-picker v-model="local.dateRange" type="daterange" value-format="YYYY-MM-DDTHH:mm:ss" />
        <el-select v-model="local.sortBy" placeholder="Sort">
          <el-option label="Ngày tạo" value="createdAt" />
          <el-option label="Ngày đọc" value="readAt" />
          <el-option label="Loại" value="type" />
          <el-option label="Tiêu đề" value="title" />
        </el-select>
        <el-select v-model="local.sortDirection" placeholder="Thứ tự">
          <el-option label="Giảm dần" value="desc" />
          <el-option label="Tăng dần" value="asc" />
        </el-select>
        <el-button :icon="Refresh" @click="emit('reset')">Reset</el-button>
      </div>
    </el-drawer>
  </section>
</template>

<style scoped>
.notification-filters {
  padding: 12px;
}

.filters-desktop {
  align-items: center;
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(220px, 1.1fr) minmax(230px, auto) minmax(130px, 0.6fr) minmax(260px, 1fr) minmax(120px, 0.55fr) minmax(110px, 0.5fr) auto;
}

.filters-mobile {
  display: none;
  gap: 10px;
  grid-template-columns: 1fr auto;
}

.drawer-filters {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

@media (max-width: 1240px) {
  .filters-desktop {
    display: none;
  }

  .filters-mobile {
    display: grid;
  }
}
</style>
