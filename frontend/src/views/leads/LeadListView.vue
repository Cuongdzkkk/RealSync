<script setup lang="ts">
import PageHeader from '@/components/common/PageHeader.vue';
import LeadKanban from '@/components/lead/LeadKanban.vue';
import { mockLeads } from '@/utils/mockData';

const leadMetrics = [
  { label: 'Tổng lead', value: 164, variant: 'info' as const },
  { label: 'Lead nóng', value: 34, variant: 'danger' as const },
  { label: 'Đã xác nhận', value: 89, variant: 'success' as const },
  { label: 'Chờ xử lý', value: 41, variant: 'warning' as const }
];
</script>

<template>
  <section class="page">
    <PageHeader title="Khách hàng tiềm năng" description="Theo dõi nhu cầu, mức độ nóng và trạng thái chăm sóc khách hàng.">
      <template #actions>
        <button class="btn-new">+ Thêm lead</button>
      </template>
    </PageHeader>

    <div class="lead-metrics">
      <div
        v-for="m in leadMetrics"
        :key="m.label"
        class="lead-metric"
        :style="{ background: m.variant === 'danger' ? 'var(--color-pastel-pink)' : m.variant === 'success' ? 'var(--color-pastel-green)' : m.variant === 'warning' ? 'var(--color-pastel-peach)' : 'var(--color-pastel-blue)' }"
      >
        <span class="lead-metric__label">{{ m.label }}</span>
        <strong class="lead-metric__value">{{ m.value }}</strong>
      </div>
    </div>

    <LeadKanban :leads="mockLeads" />
  </section>
</template>

<style scoped>
.lead-metrics {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.lead-metric {
  border-radius: 12px;
  padding: 16px 20px;
}

.lead-metric__label {
  display: block;
  font-size: 10px;
  font-weight: 500;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  margin-bottom: 8px;
}

.lead-metric__value {
  display: block;
  font-size: 28px;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
  line-height: 1;
}

.btn-new {
  background: #F5E642;
  color: #0D0D0D;
  font-size: 13px;
  font-weight: 600;
  border: none;
  border-radius: 20px;
  padding: 8px 18px;
  cursor: pointer;
  transition: background var(--duration-fast) var(--ease-standard);
}

.btn-new:hover {
  background: #EDD800;
}

@media (max-width: 640px) {
  .lead-metrics {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
