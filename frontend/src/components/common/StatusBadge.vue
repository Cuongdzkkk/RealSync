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
  border-radius: 4px;
  display: inline-flex;
  font-size: 10px;
  font-weight: 600;
  gap: 6px;
  letter-spacing: 0.04em;
  line-height: 1.2;
  padding: 3px 8px;
  text-transform: uppercase;
  white-space: nowrap;
}

.status-badge--success {
  background: var(--color-success-bg);
  color: var(--color-success);
}

.status-badge--warning {
  background: var(--color-warning-bg);
  color: var(--color-warning);
}

.status-badge--danger {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.status-badge--info {
  background: var(--color-info-bg);
  color: var(--color-info);
}

.status-badge--ai {
  background: var(--color-ai-bg);
  color: var(--color-ai);
}

.status-badge--muted {
  background: #F4F4F5;
  color: #71717A;
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
