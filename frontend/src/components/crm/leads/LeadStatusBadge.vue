<script setup lang="ts">
import { getLeadStatusLabel } from '@/utils/crm';
import type { LeadStatus } from '@/types/crm/lead';

defineProps<{ status: LeadStatus }>();
</script>

<template>
  <span class="lead-badge" :class="`lead-badge--${status.toLowerCase()}`">
    {{ getLeadStatusLabel(status) }}
  </span>
</template>

<style scoped>
.lead-badge {
  border: 1px solid var(--color-border);
  border-radius: 999px;
  display: inline-flex;
  font-size: 11px;
  font-weight: 700;
  line-height: 1;
  padding: 6px 9px;
  white-space: nowrap;
}

.lead-badge--new { background: var(--color-info-bg); border-color: var(--color-info-border); color: var(--color-info); }
.lead-badge--contacted { background: var(--color-info-bg); border-color: var(--color-info-border); color: var(--color-info); }
.lead-badge--qualified { background: var(--color-ai-bg); border-color: var(--color-ai-border); color: var(--color-ai); }
.lead-badge--proposal { background: var(--color-warning-bg); border-color: var(--color-warning-border); color: var(--color-warning); }
.lead-badge--won { background: var(--color-success-bg); border-color: var(--color-success-border); color: var(--color-success); }
.lead-badge--lost { background: var(--color-danger-bg); border-color: var(--color-danger-border); color: var(--color-danger); }
</style>
