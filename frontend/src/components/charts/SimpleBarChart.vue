<script setup lang="ts">
import { computed } from 'vue';

import type { MarketPoint } from '@/types/dashboard';

const props = withDefaults(defineProps<{
  data: MarketPoint[];
  hideLabels?: boolean;
  activeIndex?: number;
}>(), {
  activeIndex: 0
});

const maxValue = computed(() => Math.max(...props.data.map((item) => item.value), 1));
</script>

<template>
  <div class="simple-chart">
    <div
      v-for="(item, i) in data"
      :key="item.label"
      class="simple-chart__bar"
      :class="{ 'is-compact': hideLabels }"
    >
      <span v-if="!hideLabels">{{ item.label }}</span>
      <div class="bar-bg">
        <i
          class="bar-fill"
          :class="{ 'bar-fill--accent': i === activeIndex }"
          :style="{ width: `${(item.value / maxValue) * 100}%` }"
        />
      </div>
      <strong v-if="!hideLabels" class="numeric">{{ item.value }}</strong>
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

.simple-chart__bar.is-compact {
  grid-template-columns: 1fr;
  gap: 4px;
}

.simple-chart__bar span {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.bar-bg {
  background: #E0E0E0;
  border-radius: 9999px;
  height: 8px;
  overflow: hidden;
}

.bar-fill {
  background: #0D0D0D;
  display: block;
  height: 100%;
  border-radius: 9999px;
  transition: width var(--duration-slow) var(--ease-standard);
}

.bar-fill--accent {
  background: var(--color-yellow);
}

.simple-chart__bar strong {
  text-align: right;
  color: var(--color-text-primary);
}
</style>
