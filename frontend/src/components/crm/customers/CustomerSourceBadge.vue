<script setup lang="ts">
import type { CustomerSource } from '@/types/crm/customer';

defineProps<{
  source?: CustomerSource | null;
  converted?: boolean;
}>();
</script>

<template>
  <span class="source-badge" :class="{ 'source-badge--converted': converted }">
    {{ converted ? 'Chuyển đổi từ Lead' : source || 'Other' }}
  </span>
</template>

<style scoped>
.source-badge {
  background: var(--color-info-bg);
  border: 1px solid var(--color-info-border);
  border-radius: 999px;
  color: var(--color-info);
  display: inline-flex;
  font-size: 11px;
  font-weight: 700;
  line-height: 1;
  padding: 6px 9px;
  white-space: nowrap;
}

.source-badge--converted {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
}
</style>
