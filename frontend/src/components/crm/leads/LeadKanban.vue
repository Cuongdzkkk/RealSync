<script setup lang="ts">
import type { CrmLead, LeadStatus } from '@/types/crm/lead';
import { LEAD_STATUSES } from '@/types/crm/lead';
import LeadKanbanColumn from './LeadKanbanColumn.vue';

defineProps<{
  groups: Record<LeadStatus, CrmLead[]>;
}>();

defineEmits<{
  (e: 'open', lead: CrmLead): void;
  (e: 'status', lead: CrmLead, status: LeadStatus): void;
}>();
</script>

<template>
  <div class="kanban glass-card">
    <LeadKanbanColumn
      v-for="status in LEAD_STATUSES"
      :key="status"
      :status="status"
      :leads="groups[status]"
      @open="$emit('open', $event)"
      @status="(lead, nextStatus) => $emit('status', lead, nextStatus)"
    />
  </div>
</template>

<style scoped>
.kanban {
  display: grid;
  gap: 12px;
  grid-auto-flow: column;
  grid-auto-columns: minmax(280px, 1fr);
  overflow-x: auto;
  padding: 12px;
}
</style>
