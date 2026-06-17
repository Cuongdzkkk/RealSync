<script setup lang="ts">
import { formatDateTime, formatNullableText } from '@/utils/crm';
import type { CrmCustomerDetail } from '@/types/crm/customer';
import CrmSectionCard from '@/components/crm/common/CrmSectionCard.vue';

defineProps<{ customer: CrmCustomerDetail }>();
</script>

<template>
  <CrmSectionCard title="Hồ sơ khách hàng" description="Thông tin liên hệ và bối cảnh chăm sóc.">
    <div class="overview-grid">
      <div class="overview-item">
        <span>Địa chỉ</span>
        <strong>{{ formatNullableText(customer.address) }}</strong>
      </div>
      <div class="overview-item">
        <span>Công ty</span>
        <strong>{{ customer.company || 'Cá nhân' }}</strong>
      </div>
      <div class="overview-item overview-item--wide">
        <span>Ghi chú</span>
        <strong>{{ formatNullableText(customer.notes) }}</strong>
      </div>
      <div class="overview-item">
        <span>Phone</span>
        <strong>{{ formatNullableText(customer.phone) }}</strong>
      </div>
      <div class="overview-item">
        <span>Email</span>
        <strong>{{ formatNullableText(customer.email) }}</strong>
      </div>
      <div class="overview-item">
        <span>Nguồn</span>
        <strong>{{ formatNullableText(customer.source) }}</strong>
      </div>
      <div class="overview-item">
        <span>Người phụ trách</span>
        <strong>{{ formatNullableText(customer.assignedToName) }}</strong>
      </div>
      <div class="overview-item">
        <span>Ngày tạo</span>
        <strong>{{ formatDateTime(customer.createdAt) }}</strong>
      </div>
      <div class="overview-item">
        <span>Cập nhật</span>
        <strong>{{ formatDateTime(customer.updatedAt) }}</strong>
      </div>
    </div>
  </CrmSectionCard>
</template>

<style scoped>
.overview-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.overview-item {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 12px;
}

.overview-item--wide {
  grid-column: 1 / -1;
}

span {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
}

strong {
  color: var(--color-text-primary);
  font-size: 13px;
  line-height: 1.5;
}

@media (max-width: 768px) {
  .overview-grid {
    grid-template-columns: 1fr;
  }
}
</style>
