<script setup lang="ts">
import { Link } from '@element-plus/icons-vue';
import { formatDateTime } from '@/utils/crm';
import type { CrmCustomerDetail } from '@/types/crm/customer';
import CrmSectionCard from '@/components/crm/common/CrmSectionCard.vue';

defineProps<{ customer: CrmCustomerDetail }>();
defineEmits<{ (e: 'openLead', leadId: string): void }>();
</script>

<template>
  <CrmSectionCard :title="customer.convertedFromLeadId ? 'Chuyển đổi từ Lead' : 'Khách hàng trực tiếp'">
    <div v-if="customer.convertedFromLeadId" class="source-card source-card--converted">
      <span class="badge">Success</span>
      <strong>{{ customer.convertedFromLeadName }}</strong>
      <p>Ngày tạo Customer: {{ formatDateTime(customer.createdAt) }}</p>
      <el-button :icon="Link" @click="$emit('openLead', customer.convertedFromLeadId)">Mở Lead gốc</el-button>
    </div>
    <div v-else class="source-card">
      <span class="badge badge--muted">Direct</span>
      <strong>Khách hàng trực tiếp</strong>
      <p>Nguồn: {{ customer.source || 'Other' }}</p>
    </div>
  </CrmSectionCard>
</template>

<style scoped>
.source-card {
  background: var(--color-info-bg);
  border: 1px solid var(--color-info-border);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 14px;
}

.source-card--converted {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
}

.badge {
  align-self: flex-start;
  border: 1px solid var(--color-success-border);
  border-radius: 999px;
  color: var(--color-success);
  font-size: 10px;
  font-weight: 800;
  padding: 4px 8px;
  text-transform: uppercase;
}

.badge--muted {
  border-color: var(--color-border);
  color: var(--color-text-muted);
}

strong {
  color: var(--color-text-primary);
}

p {
  color: var(--color-text-secondary);
  margin: 0;
}
</style>
