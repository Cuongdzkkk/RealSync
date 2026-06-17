<script setup lang="ts">
import { Bell, Clock, OfficeBuilding, Setting, User, UserFilled } from '@element-plus/icons-vue';
import type { NotificationType } from '@/types/crm/notification';
import { getNotificationTypeVariant } from '@/utils/notification';

const props = defineProps<{
  type: NotificationType;
  size?: 'sm' | 'md';
}>();

const icons = {
  System: Setting,
  Lead: User,
  Property: OfficeBuilding,
  Task: Clock,
  Assignment: UserFilled
};
</script>

<template>
  <span class="type-icon" :class="[`type-icon--${getNotificationTypeVariant(props.type)}`, `type-icon--${props.size ?? 'md'}`]">
    <component :is="icons[props.type] ?? Bell" />
  </span>
</template>

<style scoped>
.type-icon {
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  display: inline-flex;
  flex-shrink: 0;
  justify-content: center;
}

.type-icon--sm {
  height: 28px;
  width: 28px;
}

.type-icon--md {
  height: 36px;
  width: 36px;
}

.type-icon :deep(svg) {
  height: 16px;
  width: 16px;
}

.type-icon--muted {
  background: var(--color-surface-glass);
  color: var(--color-text-muted);
}

.type-icon--ai {
  background: var(--color-ai-bg);
  border-color: var(--color-ai-border);
  color: var(--color-ai);
}

.type-icon--success {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
}

.type-icon--warning {
  background: var(--color-warning-bg);
  border-color: var(--color-warning-border);
  color: var(--color-warning);
}

.type-icon--info {
  background: var(--color-info-bg);
  border-color: var(--color-info-border);
  color: var(--color-info);
}
</style>
