<script setup lang="ts">
import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockCrawlSources } from '@/utils/mockData';
import { formatDate } from '@/utils/format';
</script>

<template>
  <section class="page">
    <PageHeader title="Crawler Engine" description="Quản lý nguồn crawl, trạng thái job và chất lượng dữ liệu." />

    <section class="panel">
      <el-table :data="mockCrawlSources">
        <el-table-column prop="name" label="Nguồn" min-width="180" />
        <el-table-column prop="baseUrl" label="URL" min-width="260">
          <template #default="{ row }">
            <span class="mono">{{ row.baseUrl }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Trạng thái" width="140">
          <template #default="{ row }">
            <StatusBadge :label="row.isActive ? 'Active' : 'Paused'" :variant="row.isActive ? 'success' : 'muted'" :pulse="row.isActive" />
          </template>
        </el-table-column>
        <el-table-column label="Tỉ lệ thành công" width="160">
          <template #default="{ row }">
            <span class="numeric">{{ row.successRate }}%</span>
          </template>
        </el-table-column>
        <el-table-column label="Listing hôm nay" width="160">
          <template #default="{ row }">
            <span class="numeric">{{ row.listingsToday }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Lần chạy cuối" width="150">
          <template #default="{ row }">
            {{ formatDate(row.lastRunAt) }}
          </template>
        </el-table-column>
      </el-table>
    </section>
  </section>
</template>
