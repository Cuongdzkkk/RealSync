<script setup lang="ts">
defineProps<{
  title: string;
  description?: string;
}>();
</script>

<template>
  <div class="empty">
    <div class="empty__icon">
      <slot name="icon" />
    </div>
    <h3>{{ title }}</h3>
    <p v-if="description">{{ description }}</p>
    <slot name="action" />
  </div>
</template>

<style scoped>
.empty {
  align-items: center;
  color: var(--color-text-secondary);
  display: flex;
  flex-direction: column;
  gap: 10px;
  justify-content: center;
  min-height: 220px;
  padding: 28px;
  text-align: center;
}

.empty__icon {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  color: var(--color-text-muted);
  display: flex;
  height: 44px;
  justify-content: center;
  width: 44px;
}

h3 {
  color: var(--color-text-primary);
  font-size: 15px;
  margin: 0;
}

p {
  line-height: 1.6;
  margin: 0;
  max-width: 420px;
}
</style>
