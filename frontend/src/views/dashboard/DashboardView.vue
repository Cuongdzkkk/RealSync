<script setup lang="ts">
import SimpleBarChart from '@/components/charts/SimpleBarChart.vue';
import MetricCard from '@/components/common/MetricCard.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockCrawlSources, mockMarketTrend, mockMetrics, mockProperties } from '@/utils/mockData';
import { formatCurrency } from '@/utils/format';

const metricColors = [
  'var(--color-pastel-pink)',
  'var(--color-pastel-blue)',
  'var(--color-pastel-green)',
  'var(--color-pastel-peach)'
];
</script>

<template>
  <div class="col-left">
    <section class="panel">
      <div class="panel__header">
        <h2 class="panel__title">Listing cần xử lý</h2>
      </div>
      <el-table :data="mockProperties">
        <el-table-column prop="title" label="Sản phẩm" min-width="200" />
        <el-table-column prop="area" label="Khu vực" width="110" />
        <el-table-column label="Giá" width="140">
          <template #default="{ row }">
            <span class="numeric">{{ formatCurrency(row.price) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="AI" width="80">
          <template #default="{ row }">
            <StatusBadge :label="`${row.aiScore}%`" variant="ai" />
          </template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel" style="margin-top: 20px;">
      <div class="panel__header">
        <h2 class="panel__title">Sức khỏe Crawler</h2>
      </div>
      <div class="panel__body crawler-list">
        <div v-for="source in mockCrawlSources" :key="source.id" class="crawler-list__item">
          <div>
            <strong>{{ source.name }}</strong>
            <span class="mono">{{ source.baseUrl }}</span>
          </div>
          <StatusBadge
            :label="source.isActive ? 'Hoạt động' : 'Tạm dừng'"
            :variant="source.isActive ? 'success' : 'muted'"
            :pulse="source.isActive"
          />
        </div>
      </div>
    </section>

    <section class="panel activity-panel">
      <div class="panel__header">
        <h2 class="panel__title">Hoạt động gần đây</h2>
      </div>
      <div class="panel__body">
        <div class="activity-item">
          <div class="activity-dot activity-dot--success" />
          <div class="activity-content">
            <strong>Crawler Batdongsan</strong>
            <p>342 tin mới, 18 tin trùng đã loại bỏ</p>
          </div>
          <span class="activity-time">2 giờ trước</span>
        </div>
        <div class="activity-item">
          <div class="activity-dot activity-dot--warning" />
          <div class="activity-content">
            <strong>AI confidence thấp</strong>
            <p>3 tin cần kiểm duyệt thủ công</p>
          </div>
          <span class="activity-time">3 giờ trước</span>
        </div>
        <div class="activity-item">
          <div class="activity-dot activity-dot--danger" />
          <div class="activity-content">
            <strong>Khách hàng nóng chưa gán</strong>
            <p>Nguyễn Minh Anh cần gọi lại trong 24h</p>
          </div>
          <span class="activity-time">5 giờ trước</span>
        </div>
        <div class="activity-item">
          <div class="activity-dot activity-dot--info" />
          <div class="activity-content">
            <strong>Listing mới từ CallAgent</strong>
            <p>15 tin đã qua AI classification, sẵn sàng xuất bản</p>
          </div>
          <span class="activity-time">6 giờ trước</span>
        </div>
      </div>
    </section>
  </div>

  <div class="col-center">
    <div class="metric-grid">
      <MetricCard
        v-for="(metric, i) in mockMetrics"
        :key="metric.label"
        v-bind="metric"
        :bg-color="metricColors[i % metricColors.length]"
      />
    </div>

    <div class="chart-container">
      <div class="chart-header">
        <h3 class="chart-title">Khu vực nổi bật</h3>
        <div class="chart-hero-number numeric">3,842</div>
      </div>
      <SimpleBarChart :data="mockMarketTrend" />
    </div>
  </div>
</template>

<style scoped>
.metric-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.chart-container {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 20px 24px;
}

.chart-header {
  margin-bottom: 20px;
}

.chart-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 8px 0;
}

.chart-hero-number {
  font-size: 36px;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
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

.activity-panel {
  margin-top: 20px;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px 0;
  border-bottom: 1px solid var(--color-divider);
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 5px;
  flex-shrink: 0;
}

.activity-dot--success { background: var(--color-success); }
.activity-dot--warning { background: var(--color-warning); }
.activity-dot--danger { background: var(--color-danger); }
.activity-dot--info { background: var(--color-info); }

.activity-content {
  flex: 1;
  min-width: 0;
}

.activity-content strong {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: 2px;
}

.activity-content p {
  margin: 0;
  font-size: 12px;
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.activity-time {
  font-size: 11px;
  color: var(--color-text-muted);
  white-space: nowrap;
  flex-shrink: 0;
}

@media (max-width: 1024px) {
  .metric-grid {
    grid-template-columns: 1fr;
  }
}
</style>
