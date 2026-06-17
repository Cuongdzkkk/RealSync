<script setup lang="ts">
import { computed } from 'vue';
import { getLeadTemperature } from '@/utils/crm';

const props = defineProps<{ score: number }>();
const temperature = computed(() => getLeadTemperature(props.score));
</script>

<template>
  <span class="score numeric" :class="`score--${temperature.toLowerCase()}`">
    {{ score }}
    <small>{{ temperature }}</small>
  </span>
</template>

<style scoped>
.score {
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  display: inline-flex;
  font-weight: 800;
  gap: 6px;
  padding: 5px 8px;
}

.score small {
  font-family: var(--font-ui);
  font-size: 10px;
  font-weight: 700;
}

.score--hot { background: var(--color-danger-bg); border-color: var(--color-danger-border); color: var(--color-danger); }
.score--warm { background: var(--color-warning-bg); border-color: var(--color-warning-border); color: var(--color-warning); }
.score--cold { background: var(--color-info-bg); border-color: var(--color-info-border); color: var(--color-info); }
</style>
