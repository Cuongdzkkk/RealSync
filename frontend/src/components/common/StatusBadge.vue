<script setup lang="ts">
import type { StatusVariant } from '@/types/common';

defineProps<{
  label: string;
  variant: StatusVariant;
  pulse?: boolean;
}>();
</script>

<template>
  <span class="status-badge" :class="`status-badge--${variant}`">
    <i v-if="pulse" class="status-badge__dot" />
    {{ label }}
  </span>
</template>

<style scoped>
.status-badge {
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  display: inline-flex;
  font-size: 11px;
  font-weight: 600;
  gap: 6px;
  letter-spacing: 0.04em;
  line-height: 1;
  padding: 2px 8px;
  text-transform: uppercase;
  white-space: nowrap;
}

.status-badge--success {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
}

.status-badge--warning {
  background: var(--color-warning-bg);
  border-color: var(--color-warning-border);
  color: var(--color-warning);
}

.status-badge--danger {
  background: var(--color-danger-bg);
  border-color: var(--color-danger-border);
  color: var(--color-danger);
}

.status-badge--info {
  background: var(--color-info-bg);
  border-color: var(--color-info-border);
  color: var(--color-info);
}

.status-badge--ai {
  background: var(--color-ai-bg);
  border-color: var(--color-ai-border);
  color: var(--color-ai);
}

.status-badge--muted {
  background: #f4f4f5;
  color: #71717a;
}

.status-badge__dot {
  animation: pulse 1.4s infinite;
  background: currentColor;
  border-radius: 9999px;
  height: 6px;
  width: 6px;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.35;
  }
}
</style>
