<script setup lang="ts">
import { mockProperties, mockMetrics, mockMarketTrend } from '@/utils/mockData';
import PropertyListItem from '@/components/ui/PropertyListItem.vue';
import MetricCard from '@/components/common/MetricCard.vue';
import SimpleBarChart from '@/components/charts/SimpleBarChart.vue';

const metricColors = [
  'var(--color-pastel-pink)',
  'var(--color-pastel-blue)',
  'var(--color-pastel-green)',
  'var(--color-pastel-peach)'
];
</script>

<template>
  <div class="col-left">
    <div class="list-header">
      <h2 class="section-title">Danh sách sản phẩm</h2>
      <div class="filter-chips">
        <button class="chip chip--active">Tất cả</button>
        <button class="chip">Đang bán</button>
        <button class="chip">Cho thuê</button>
      </div>
    </div>
    
    <div class="property-list">
      <PropertyListItem 
        v-for="prop in mockProperties" 
        :key="prop.id" 
        :property="prop" 
      />
    </div>
  </div>

  <div class="col-center">
    <h2 class="section-title">Tổng quan thị trường</h2>
    
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
        <h3 class="chart-title">Lượt xem theo khu vực</h3>
        <div class="chart-hero-number numeric">3,842</div>
      </div>
      <SimpleBarChart :data="mockMarketTrend" />
    </div>
  </div>
</template>

<style scoped>
.section-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 16px 0;
}

.list-header {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.filter-chips {
  display: flex;
  gap: 8px;
}

.chip {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 20px;
  padding: 5px 14px;
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-primary);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
}

.chip:hover {
  background: var(--color-surface-hover);
}

.chip--active {
  background: #0D0D0D;
  color: #FFFFFF;
  border-color: #0D0D0D;
}

.chip--active:hover {
  background: #0D0D0D;
}

.property-list {
  display: flex;
  flex-direction: column;
}

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

@media (max-width: 1024px) {
  .metric-grid {
    grid-template-columns: 1fr;
  }
}
</style>
