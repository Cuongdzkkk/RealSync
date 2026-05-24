<script setup lang="ts">
import { computed } from 'vue';

import type { MarketPoint } from '@/types/dashboard';

const props = defineProps<{
  data: MarketPoint[];
}>();

const maxValue = computed(() => Math.max(...props.data.map((item) => item.value), 1));
</script>

<template>
  <div class="simple-chart">
    <div v-for="item in data" :key="item.label" class="simple-chart__bar">
      <span>{{ item.label }}</span>
      <div>
        <i :style="{ width: `${(item.value / maxValue) * 100}%` }" />
      </div>
      <strong class="numeric">{{ item.value }}</strong>
    </div>
  </div>
</template>

<style scoped>
.simple-chart {
  display: grid;
  gap: 14px;
}

.simple-chart__bar {
  align-items: center;
  display: grid;
  gap: 12px;
  grid-template-columns: 96px 1fr 56px;
}

.simple-chart__bar span {
  color: var(--color-text-secondary);
}

.simple-chart__bar div {
  background: var(--color-primary-10);
  border-radius: 9999px;
  height: 10px;
}

.simple-chart__bar i {
  background: var(--color-primary);
  border-radius: inherit;
  display: block;
  height: 100%;
}

.simple-chart__bar strong {
  text-align: right;
}
</style>
