<script setup lang="ts">
import SimpleBarChart from '@/components/charts/SimpleBarChart.vue';
import MetricCard from '@/components/common/MetricCard.vue';
import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockCrawlSources, mockMarketTrend, mockMetrics, mockProperties } from '@/utils/mockData';
import { formatCurrency } from '@/utils/format';
</script>

<template>
  <section class="page">
    <PageHeader title="Dashboard" description="Tổng quan data engine, listing, crawler và lead pipeline.">
      <template #actions>
        <el-button type="primary">Xuất báo cáo</el-button>
      </template>
    </PageHeader>

    <div class="metric-grid">
      <MetricCard v-for="metric in mockMetrics" :key="metric.label" v-bind="metric" />
    </div>

    <div class="dashboard-grid">
      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Khu vực nổi bật</h2>
        </div>
        <div class="panel__body">
          <SimpleBarChart :data="mockMarketTrend" />
        </div>
      </section>

      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Crawler health</h2>
        </div>
        <div class="panel__body crawler-list">
          <div v-for="source in mockCrawlSources" :key="source.id" class="crawler-list__item">
            <div>
              <strong>{{ source.name }}</strong>
              <span class="mono">{{ source.baseUrl }}</span>
            </div>
            <StatusBadge :label="source.isActive ? 'Active' : 'Paused'" :variant="source.isActive ? 'success' : 'muted'" :pulse="source.isActive" />
          </div>
        </div>
      </section>
    </div>

    <section class="panel">
      <div class="panel__header">
        <h2 class="panel__title">Listing cần xử lý</h2>
      </div>
      <el-table :data="mockProperties">
        <el-table-column prop="title" label="Sản phẩm" min-width="260" />
        <el-table-column prop="area" label="Khu vực" width="140" />
        <el-table-column label="Giá" width="170">
          <template #default="{ row }">
            <span class="numeric">{{ formatCurrency(row.price) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="AI score" width="120">
          <template #default="{ row }">
            <StatusBadge :label="`${row.aiScore}%`" variant="ai" />
          </template>
        </el-table-column>
      </el-table>
    </section>
  </section>
</template>

<style scoped>
.dashboard-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: minmax(0, 1.4fr) minmax(320px, 0.8fr);
}

.crawler-list {
  display: grid;
  gap: 14px;
}

.crawler-list__item {
  align-items: center;
  display: flex;
  justify-content: space-between;
}

.crawler-list__item div {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.crawler-list__item span {
  color: var(--color-text-muted);
  font-size: 12px;
}

@media (max-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}
</style>
