<script setup lang="ts">
import type { NotificationType } from '@/types/crm/notification';
import { getNotificationTypeLabel, getNotificationTypeVariant } from '@/utils/notification';

defineProps<{ type: NotificationType }>();
</script>

<template>
  <span class="type-badge" :class="`type-badge--${getNotificationTypeVariant(type)}`">
    {{ getNotificationTypeLabel(type) }}
  </span>
</template>

<style scoped>
.type-badge {
  border: 1px solid var(--color-border);
  border-radius: 6px;
  display: inline-flex;
  font-size: 10px;
  font-weight: 800;
  line-height: 1;
  padding: 5px 7px;
  text-transform: uppercase;
}

.type-badge--muted {
  background: var(--color-surface-glass);
  color: var(--color-text-muted);
}

.type-badge--ai {
  background: var(--color-ai-bg);
  border-color: var(--color-ai-border);
  color: var(--color-ai);
}

.type-badge--success {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
}

.type-badge--warning {
  background: var(--color-warning-bg);
  border-color: var(--color-warning-border);
  color: var(--color-warning);
}

.type-badge--info {
  background: var(--color-info-bg);
  border-color: var(--color-info-border);
  color: var(--color-info);
}
</style>
