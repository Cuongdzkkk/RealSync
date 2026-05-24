<script setup lang="ts">
import { computed, ref } from 'vue';
import { Plus } from '@element-plus/icons-vue';

import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import PropertyFilterBar from '@/components/property/PropertyFilterBar.vue';
import { mockProperties } from '@/utils/mockData';
import { formatCurrency, formatDate } from '@/utils/format';

const keyword = ref('');
const status = ref('');

const filteredProperties = computed(() =>
  mockProperties.filter((property) => {
    const matchesKeyword = property.title.toLowerCase().includes(keyword.value.toLowerCase()) || property.address.toLowerCase().includes(keyword.value.toLowerCase());
    const matchesStatus = status.value ? property.status === status.value : true;
    return matchesKeyword && matchesStatus;
  })
);
</script>

<template>
  <section class="page">
    <PageHeader title="Sản phẩm" description="Quản lý listing, chuẩn hóa dữ liệu và điểm AI.">
      <template #actions>
        <el-button :icon="Plus" type="primary">Thêm sản phẩm</el-button>
      </template>
    </PageHeader>

    <section class="panel">
      <div class="panel__body">
        <PropertyFilterBar v-model:keyword="keyword" v-model:status="status" />
      </div>
      <el-table :data="filteredProperties">
        <el-table-column prop="title" label="Tiêu đề" min-width="280" />
        <el-table-column prop="source" label="Nguồn" width="150">
          <template #default="{ row }">
            <span class="mono">{{ row.source }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="area" label="Khu vực" width="140" />
        <el-table-column label="Giá" width="170">
          <template #default="{ row }">
            <span class="numeric">{{ formatCurrency(row.price) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Trạng thái" width="140">
          <template #default="{ row }">
            <StatusBadge :label="row.status" :variant="row.status === 'verified' || row.status === 'published' ? 'success' : row.status === 'expired' ? 'muted' : 'warning'" />
          </template>
        </el-table-column>
        <el-table-column label="Ngày tạo" width="130">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
      </el-table>
    </section>
  </section>
</template>
