<script setup lang="ts">
import type { CrmLead, LeadStatus } from '@/types/crm/lead';
import { getLeadStatusLabel } from '@/utils/crm';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import LeadCard from './LeadCard.vue';

defineProps<{
  status: LeadStatus;
  leads: CrmLead[];
}>();

defineEmits<{
  (e: 'open', lead: CrmLead): void;
  (e: 'status', lead: CrmLead, status: LeadStatus): void;
}>();
</script>

<template>
  <section class="kanban-column">
    <header>
      <strong>{{ getLeadStatusLabel(status) }}</strong>
      <span class="numeric">{{ leads.length }}</span>
    </header>
    <div class="kanban-column__body">
      <CrmEmptyState v-if="leads.length === 0" title="Chưa có Lead" description="Cột này đang trống." />
      <LeadCard
        v-for="lead in leads"
        v-else
        :key="lead.id"
        :lead="lead"
        @open="$emit('open', $event)"
        @status="(item, nextStatus) => $emit('status', item, nextStatus)"
      />
    </div>
  </section>
</template>

<style scoped>
.kanban-column {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  min-height: 520px;
  min-width: 280px;
  overflow: hidden;
}

header {
  align-items: center;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  justify-content: space-between;
  padding: 12px;
}

strong {
  color: var(--color-text-primary);
}

span {
  background: var(--color-yellow-muted);
  border-radius: 999px;
  color: var(--color-yellow-hover);
  font-weight: 800;
  padding: 3px 8px;
}

.kanban-column__body {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 10px;
  overflow-y: auto;
  padding: 10px;
}
</style>
