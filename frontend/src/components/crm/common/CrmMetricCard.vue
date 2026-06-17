<script setup lang="ts">
import { Loading } from '@element-plus/icons-vue';

defineProps<{
  label: string;
  value: string | number;
  hint?: string;
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info' | 'ai';
  loading?: boolean;
}>();
</script>

<template>
  <article class="metric glass-card" :class="`metric--${variant ?? 'default'}`">
    <div class="metric__icon">
      <el-icon v-if="loading"><Loading /></el-icon>
      <slot v-else name="icon" />
    </div>
    <div class="metric__body">
      <span class="metric__label">{{ label }}</span>
      <span v-if="loading" class="metric__skeleton skeleton" />
      <strong v-else class="metric__value numeric">{{ value }}</strong>
      <small v-if="hint">{{ hint }}</small>
    </div>
  </article>
</template>

<style scoped>
.metric {
  align-items: center;
  display: flex;
  gap: 12px;
  min-height: 86px;
  padding: 16px;
}

.metric__icon {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  color: var(--color-text-secondary);
  display: flex;
  height: 38px;
  justify-content: center;
  width: 38px;
}

.metric__body {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.metric__label,
small {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 600;
}

.metric__value {
  color: var(--color-text-primary);
  font-size: 22px;
  line-height: 1.1;
}

.metric__skeleton {
  height: 24px;
  width: 80px;
}

.metric--success .metric__icon { color: var(--color-success); background: var(--color-success-bg); border-color: var(--color-success-border); }
.metric--warning .metric__icon { color: var(--color-warning); background: var(--color-warning-bg); border-color: var(--color-warning-border); }
.metric--danger .metric__icon { color: var(--color-danger); background: var(--color-danger-bg); border-color: var(--color-danger-border); }
.metric--info .metric__icon { color: var(--color-info); background: var(--color-info-bg); border-color: var(--color-info-border); }
.metric--ai .metric__icon { color: var(--color-ai); background: var(--color-ai-bg); border-color: var(--color-ai-border); }
</style>
