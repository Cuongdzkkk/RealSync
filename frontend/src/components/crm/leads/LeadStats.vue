<script setup lang="ts">
import { Aim, Clock, TrendCharts, User } from '@element-plus/icons-vue';
import CrmMetricCard from '@/components/crm/common/CrmMetricCard.vue';

defineProps<{
  total: number;
  hot: number;
  overdue: number;
  successRate: number;
  loading?: boolean;
}>();
</script>

<template>
  <div class="lead-stats">
    <CrmMetricCard label="Tổng Lead" :value="total" hint="Tất cả lead mock" variant="info" :loading="loading">
      <template #icon><el-icon><User /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Lead nóng" :value="hot" hint="Score từ 70 trở lên" variant="danger" :loading="loading">
      <template #icon><el-icon><Aim /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Follow-up quá hạn" :value="overdue" hint="Cần chăm sóc ngay" variant="warning" :loading="loading">
      <template #icon><el-icon><Clock /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Tỷ lệ thành công" :value="`${successRate}%`" hint="Won / tổng lead" variant="success" :loading="loading">
      <template #icon><el-icon><TrendCharts /></el-icon></template>
    </CrmMetricCard>
  </div>
</template>

<style scoped>
.lead-stats {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

@media (max-width: 1024px) {
  .lead-stats {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 520px) {
  .lead-stats {
    grid-template-columns: 1fr;
  }
}
</style>
