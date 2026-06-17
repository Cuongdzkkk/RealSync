<script setup lang="ts">
import { getLeadPriorityLabel } from '@/utils/crm';
import type { LeadPriority } from '@/types/crm/lead';

defineProps<{ priority: LeadPriority }>();
</script>

<template>
  <span class="priority" :class="`priority--${priority.toLowerCase()}`">
    {{ getLeadPriorityLabel(priority) }}
  </span>
</template>

<style scoped>
.priority {
  border: 1px solid var(--color-border);
  border-radius: 7px;
  display: inline-flex;
  font-size: 11px;
  font-weight: 700;
  line-height: 1;
  padding: 6px 8px;
}

.priority--low { background: var(--color-surface-glass); color: var(--color-text-muted); }
.priority--normal { background: var(--color-info-bg); border-color: var(--color-info-border); color: var(--color-info); }
.priority--high { background: var(--color-warning-bg); border-color: var(--color-warning-border); color: var(--color-warning); }
.priority--urgent { background: var(--color-danger-bg); border-color: var(--color-danger-border); color: var(--color-danger); }
</style>
