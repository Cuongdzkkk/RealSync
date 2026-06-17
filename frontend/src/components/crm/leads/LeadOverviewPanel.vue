<script setup lang="ts">
import { formatDateTime, formatNullableText, formatVnd } from '@/utils/crm';
import type { CrmLead } from '@/types/crm/lead';
import CrmSectionCard from '@/components/crm/common/CrmSectionCard.vue';

defineProps<{ lead: CrmLead }>();
</script>

<template>
  <CrmSectionCard title="Tổng quan nhu cầu" description="Thông tin tư vấn và bối cảnh mua bất động sản.">
    <div class="overview-grid">
      <div class="overview-item overview-item--wide">
        <span>Nhu cầu</span>
        <strong>{{ formatNullableText(lead.requirements) }}</strong>
      </div>
      <div class="overview-item">
        <span>Ngân sách</span>
        <strong class="numeric">{{ formatVnd(lead.budget) }}</strong>
      </div>
      <div class="overview-item">
        <span>Khu vực</span>
        <strong>{{ formatNullableText(lead.preferredArea) }}</strong>
      </div>
      <div class="overview-item">
        <span>Loại BĐS</span>
        <strong>{{ formatNullableText(lead.preferredType) }}</strong>
      </div>
      <div class="overview-item">
        <span>Sản phẩm quan tâm</span>
        <strong>{{ formatNullableText(lead.interestedPropertyTitle) }}</strong>
      </div>
      <div class="overview-item">
        <span>Nguồn</span>
        <strong>{{ formatNullableText(lead.sourceChannel) }}</strong>
      </div>
      <div class="overview-item">
        <span>Liên hệ gần nhất</span>
        <strong>{{ formatDateTime(lead.lastContactedAt) }}</strong>
      </div>
      <div class="overview-item">
        <span>Follow-up tiếp theo</span>
        <strong>{{ formatDateTime(lead.nextFollowUpAt) }}</strong>
      </div>
      <div class="overview-item">
        <span>ConvertedAt</span>
        <strong>{{ formatDateTime(lead.convertedAt) }}</strong>
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
  min-width: 0;
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
