<script setup lang="ts">
defineProps<{
  title: string;
  subtitle?: string;
}>();
</script>

<template>
  <header class="crm-page-header">
    <div class="crm-page-header__main">
      <slot name="breadcrumb" />
      <div>
        <h1>{{ title }}</h1>
        <p v-if="subtitle">{{ subtitle }}</p>
      </div>
    </div>
    <div v-if="$slots.action" class="crm-page-header__action">
      <slot name="action" />
    </div>
  </header>
</template>

<style scoped>
.crm-page-header {
  align-items: flex-start;
  display: flex;
  gap: 16px;
  justify-content: space-between;
}

.crm-page-header__main {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

h1 {
  color: var(--color-text-primary);
  font-size: 24px;
  font-weight: 800;
  line-height: 1.2;
  margin: 0;
}

p {
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.6;
  margin: 6px 0 0;
}

.crm-page-header__action {
  display: flex;
  flex-shrink: 0;
  gap: 8px;
}

@media (max-width: 768px) {
  .crm-page-header {
    flex-direction: column;
  }

  .crm-page-header__action {
    width: 100%;
  }
}
</style>
