<script setup lang="ts">
import type { StatusVariant } from '@/types/common';
import { formatNumber } from '@/utils/format';
import StatusBadge from './StatusBadge.vue';

withDefaults(
  defineProps<{
    label: string;
    value: number;
    suffix?: string;
    trend?: string;
    variant?: StatusVariant;
    bgColor?: string;
    loading?: boolean;
  }>(),
  {
    suffix: '',
    trend: '',
    variant: 'info',
    bgColor: '#FFFFFF',
    loading: false
  }
);
</script>

<template>
  <article class="metric-card" :style="{ backgroundColor: bgColor }">
    <template v-if="loading">
      <div class="metric-card__top">
        <span class="skeleton" style="width: 80px; height: 12px" />
        <span class="skeleton" style="width: 48px; height: 16px" />
      </div>
      <div class="skeleton" style="width: 120px; height: 34px; margin-top: 16px" />
    </template>
    <template v-else>
      <div class="metric-card__top">
        <span>{{ label }}</span>
        <StatusBadge v-if="trend" :label="trend" :variant="variant" />
      </div>
      <strong class="numeric">{{ formatNumber(value) }}{{ suffix }}</strong>
    </template>
  </article>
</template>

<style scoped>
.metric-card {
  border-radius: 12px;
  padding: 20px 24px;
  /* Pastel backgrounds replace borders for differentiation */
}

.metric-card__top {
  align-items: center;
  display: flex;
  justify-content: space-between;
}

.metric-card__top span {
  color: var(--color-text-secondary);
  font-size: 10px;
  font-weight: 500;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.metric-card strong {
  display: block;
  font-size: 34px;
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
  line-height: 1.1;
  margin-top: 16px;
}
</style>
