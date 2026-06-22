<script setup lang="ts">
defineProps<{
  title?: string;
  description?: string;
}>();
</script>

<template>
  <section class="crm-section glass-card">
    <div v-if="title || description || $slots.action" class="crm-section__header">
      <div>
        <h2 v-if="title">{{ title }}</h2>
        <p v-if="description">{{ description }}</p>
      </div>
      <slot name="action" />
    </div>
    <div class="crm-section__body">
      <slot />
    </div>
  </section>
</template>

<style scoped>
.crm-section {
  overflow: hidden;
}

.crm-section__header {
  align-items: flex-start;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  gap: 12px;
  justify-content: space-between;
  padding: 16px 18px;
}

h2 {
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 700;
  margin: 0;
}

p {
  color: var(--color-text-muted);
  font-size: 12px;
  margin: 4px 0 0;
}

.crm-section__body {
  padding: 18px;
}
</style>
