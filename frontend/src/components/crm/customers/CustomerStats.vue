<script setup lang="ts">
import { Connection, User, UserFilled, Warning } from '@element-plus/icons-vue';
import CrmMetricCard from '@/components/crm/common/CrmMetricCard.vue';

defineProps<{
  total: number;
  converted: number;
  direct: number;
  unassigned: number;
  loading?: boolean;
}>();
</script>

<template>
  <div class="customer-stats">
    <CrmMetricCard label="Tổng khách hàng" :value="total" variant="info" :loading="loading">
      <template #icon><el-icon><User /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Chuyển đổi từ Lead" :value="converted" variant="success" :loading="loading">
      <template #icon><el-icon><Connection /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Khách hàng trực tiếp" :value="direct" variant="ai" :loading="loading">
      <template #icon><el-icon><UserFilled /></el-icon></template>
    </CrmMetricCard>
    <CrmMetricCard label="Chưa phân công" :value="unassigned" variant="warning" :loading="loading">
      <template #icon><el-icon><Warning /></el-icon></template>
    </CrmMetricCard>
  </div>
</template>

<style scoped>
.customer-stats {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

@media (max-width: 1024px) {
  .customer-stats {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 520px) {
  .customer-stats {
    grid-template-columns: 1fr;
  }
}
</style>
