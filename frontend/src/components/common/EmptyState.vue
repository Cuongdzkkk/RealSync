<script setup lang="ts">
import { FolderOpened } from '@element-plus/icons-vue';

defineProps<{
  title: string;
  description: string;
}>();
</script>

<template>
  <div class="empty-state">
    <el-icon class="empty-state__icon"><FolderOpened /></el-icon>
    <strong>{{ title }}</strong>
    <p>{{ description }}</p>
    <slot />
  </div>
</template>

<style scoped>
.empty-state {
  align-items: center;
  color: var(--color-text-secondary);
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 48px 24px;
  text-align: center;
}

.empty-state__icon {
  color: var(--color-text-muted);
  font-size: 32px;
}

.empty-state strong {
  color: var(--color-text-primary);
  font-size: 16px;
}

.empty-state p {
  margin: 0;
  max-width: 420px;
}
</style>
