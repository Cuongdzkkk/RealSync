<script setup lang="ts">
import SimpleBarChart from '@/components/charts/SimpleBarChart.vue';
import MetricCard from '@/components/common/MetricCard.vue';
import ModuleShell from '@/components/common/ModuleShell.vue';
import NotificationFeed from '@/components/common/NotificationFeed.vue';
import { mockMarketTrend, mockNotifications } from '@/utils/mockData';
</script>

<template>
  <ModuleShell title="Dashboard & Insight" description="Theo dõi hiệu suất dữ liệu, thị trường, lead funnel và tín hiệu cần hành động.">
    <div class="metric-grid">
      <MetricCard label="Tăng trưởng listing" :value="18" suffix="%" trend="+5%" variant="success" />
      <MetricCard label="Lead conversion" :value="12" suffix="%" trend="+2%" variant="info" />
      <MetricCard label="Duplicate rate" :value="7" suffix="%" trend="-3%" variant="success" />
      <MetricCard label="Review queue" :value="21" trend="open" variant="warning" />
    </div>

    <div class="insight-grid">
      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Nhu cầu theo khu vực</h2>
        </div>
        <div class="panel__body">
          <SimpleBarChart :data="mockMarketTrend" />
        </div>
      </section>

      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Tín hiệu mới</h2>
        </div>
        <div class="panel__body">
          <NotificationFeed :items="mockNotifications" />
        </div>
      </section>
    </div>
  </ModuleShell>
</template>

<style scoped>
.insight-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: minmax(0, 1fr) 420px;
}

@media (max-width: 1024px) {
  .insight-grid {
    grid-template-columns: 1fr;
  }
}
</style>
