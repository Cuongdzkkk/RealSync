<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { Filter, Refresh } from '@element-plus/icons-vue';
import { CUSTOMER_SOURCES, type CustomerQuery } from '@/types/crm/customer';
import { mockCrmUsers } from '@/mocks/crm/users';

const props = defineProps<{ query: CustomerQuery }>();
const emit = defineEmits<{
  (e: 'change', query: Partial<CustomerQuery>): void;
  (e: 'reset'): void;
}>();

const drawerOpen = ref(false);
const activeUsers = mockCrmUsers.filter((user) => user.isActive);
const local = reactive({
  search: props.query.search,
  source: props.query.source,
  assignedToId: props.query.assignedToId,
  origin: props.query.origin ?? 'all',
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
  () => [local.source, local.assignedToId, local.origin, local.dateRange, local.sortBy, local.sortDirection],
  () => {
    emit('change', {
      source: local.source,
      assignedToId: local.assignedToId,
      origin: local.origin,
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
    local.source = query.source;
    local.assignedToId = query.assignedToId;
    local.origin = query.origin ?? 'all';
    local.dateRange = query.fromDate && query.toDate ? [query.fromDate, query.toDate] : [];
    local.sortBy = query.sortBy ?? 'createdAt';
    local.sortDirection = query.sortDirection ?? 'desc';
  },
  { deep: true }
);
</script>

<template>
  <section class="customer-filters glass-card">
    <div class="filters-desktop">
      <el-input v-model="local.search" clearable placeholder="Tìm tên, phone, email, công ty, nguồn" />
      <el-select v-model="local.source" clearable placeholder="Nguồn">
        <el-option v-for="source in CUSTOMER_SOURCES" :key="source" :label="source" :value="source" />
      </el-select>
      <el-select v-model="local.assignedToId" clearable placeholder="Phụ trách">
        <el-option v-for="user in activeUsers" :key="user.id" :label="user.fullName" :value="user.id" />
      </el-select>
      <el-select v-model="local.origin" placeholder="Loại nguồn">
        <el-option label="Tất cả" value="all" />
        <el-option label="Chuyển đổi từ Lead" value="converted" />
        <el-option label="Khách hàng trực tiếp" value="direct" />
      </el-select>
      <el-date-picker v-model="local.dateRange" type="daterange" range-separator="-" start-placeholder="Từ ngày" end-placeholder="Đến ngày" value-format="YYYY-MM-DDTHH:mm:ss" />
      <el-select v-model="local.sortBy" placeholder="Sort">
        <el-option label="Ngày tạo" value="createdAt" />
        <el-option label="Ngày cập nhật" value="updatedAt" />
        <el-option label="Tên" value="fullName" />
        <el-option label="Công ty" value="company" />
      </el-select>
      <el-select v-model="local.sortDirection" placeholder="Thứ tự">
        <el-option label="Giảm dần" value="desc" />
        <el-option label="Tăng dần" value="asc" />
      </el-select>
      <el-button :icon="Refresh" @click="emit('reset')">Reset</el-button>
    </div>

    <div class="filters-mobile">
      <el-input v-model="local.search" clearable placeholder="Tìm khách hàng" />
      <el-button :icon="Filter" @click="drawerOpen = true">Bộ lọc</el-button>
    </div>

    <el-drawer v-model="drawerOpen" title="Bộ lọc khách hàng" size="100%">
      <div class="drawer-filters">
        <el-select v-model="local.source" clearable placeholder="Nguồn">
          <el-option v-for="source in CUSTOMER_SOURCES" :key="source" :label="source" :value="source" />
        </el-select>
        <el-select v-model="local.assignedToId" clearable placeholder="Phụ trách">
          <el-option v-for="user in activeUsers" :key="user.id" :label="user.fullName" :value="user.id" />
        </el-select>
        <el-select v-model="local.origin" placeholder="Loại nguồn">
          <el-option label="Tất cả" value="all" />
          <el-option label="Chuyển đổi từ Lead" value="converted" />
          <el-option label="Khách hàng trực tiếp" value="direct" />
        </el-select>
        <el-date-picker v-model="local.dateRange" type="daterange" value-format="YYYY-MM-DDTHH:mm:ss" />
        <el-select v-model="local.sortBy" placeholder="Sort">
          <el-option label="Ngày tạo" value="createdAt" />
          <el-option label="Ngày cập nhật" value="updatedAt" />
          <el-option label="Tên" value="fullName" />
          <el-option label="Công ty" value="company" />
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
.customer-filters {
  padding: 12px;
}

.filters-desktop {
  align-items: center;
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(220px, 1.2fr) repeat(3, minmax(130px, 0.75fr)) minmax(260px, 1fr) minmax(120px, 0.6fr) minmax(110px, 0.55fr) auto;
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
