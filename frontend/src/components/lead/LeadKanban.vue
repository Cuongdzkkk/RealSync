<script setup lang="ts">
import type { Lead, LeadStage } from '@/types/lead';
import { LEAD_STAGES } from '@/utils/constants';
import { formatCurrency, formatDate } from '@/utils/format';
import StatusBadge from '@/components/common/StatusBadge.vue';

defineProps<{
  leads: Lead[];
}>();

const stageLabels: Record<LeadStage, string> = {
  new: 'Mới',
  contacted: 'Đã liên hệ',
  qualified: 'Đủ điều kiện',
  proposal: 'Đang đề xuất',
  won: 'Thành công',
  lost: 'Thất bại'
};
</script>

<template>
  <div class="lead-kanban">
    <section v-for="stage in LEAD_STAGES" :key="stage" class="lead-kanban__column">
      <h2>{{ stageLabels[stage] }}</h2>
      <article v-for="lead in leads.filter((item) => item.stage === stage)" :key="lead.id" class="lead-card">
        <div class="lead-card__top">
          <strong>{{ lead.fullName }}</strong>
          <StatusBadge :label="lead.temperature" :variant="lead.temperature === 'hot' ? 'danger' : lead.temperature === 'warm' ? 'warning' : 'info'" />
        </div>
        <p>{{ lead.demand }}</p>
        <span class="numeric">{{ formatCurrency(lead.budget) }}</span>
        <small>{{ lead.assignedTo }} · {{ formatDate(lead.lastContactAt) }}</small>
      </article>
    </section>
  </div>
</template>

<style scoped>
.lead-kanban {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(6, minmax(220px, 1fr));
  overflow-x: auto;
}

.lead-kanban__column {
  background: #f8f8f7;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  min-height: 480px;
  padding: 12px;
}

.lead-kanban__column h2 {
  font-size: 14px;
  margin: 0 0 12px;
}

.lead-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
  margin-bottom: 10px;
  padding: 12px;
}

.lead-card__top {
  align-items: center;
  display: flex;
  justify-content: space-between;
}

.lead-card p {
  color: var(--color-text-secondary);
  margin: 10px 0;
}

.lead-card span,
.lead-card small {
  display: block;
}

.lead-card small {
  color: var(--color-text-muted);
  margin-top: 8px;
}
</style>
