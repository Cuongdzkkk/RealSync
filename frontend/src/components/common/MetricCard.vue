<script setup lang="ts">
import type { StatusVariant } from '@/types/common';
import { formatNumber } from '@/utils/format';
import StatusBadge from './StatusBadge.vue';

defineProps<{
  label: string;
  value: number;
  suffix?: string;
  trend: string;
  variant: StatusVariant;
}>();
</script>

<template>
  <article class="metric-card">
    <div class="metric-card__top">
      <span>{{ label }}</span>
      <StatusBadge :label="trend" :variant="variant" />
    </div>
    <strong class="numeric">{{ formatNumber(value) }}{{ suffix ?? '' }}</strong>
  </article>
</template>

<style scoped>
.metric-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
  padding: 20px 24px;
}

.metric-card__top {
  align-items: center;
  display: flex;
  justify-content: space-between;
}

.metric-card__top span {
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.metric-card strong {
  display: block;
  font-size: 32px;
  letter-spacing: -0.02em;
  line-height: 1.1;
  margin-top: 16px;
}
</style>
