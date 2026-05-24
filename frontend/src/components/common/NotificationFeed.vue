<script setup lang="ts">
import type { NotificationItem } from '@/types/notification';
import { formatDate } from '@/utils/format';
import StatusBadge from './StatusBadge.vue';

defineProps<{
  items: NotificationItem[];
}>();

const toneVariant = (tone: NotificationItem['tone']) => (tone === 'ai' ? 'ai' : tone);
</script>

<template>
  <div class="notification-feed">
    <article v-for="item in items" :key="item.id" class="notification-feed__item">
      <StatusBadge :label="item.tone" :variant="toneVariant(item.tone)" />
      <div>
        <strong>{{ item.title }}</strong>
        <p>{{ item.description }}</p>
        <small>{{ formatDate(item.createdAt) }}</small>
      </div>
    </article>
  </div>
</template>

<style scoped>
.notification-feed {
  display: grid;
  gap: 14px;
}

.notification-feed__item {
  align-items: flex-start;
  border-bottom: 1px solid var(--color-border);
  display: grid;
  gap: 12px;
  grid-template-columns: auto 1fr;
  padding-bottom: 14px;
}

.notification-feed__item:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.notification-feed__item strong {
  display: block;
}

.notification-feed__item p {
  color: var(--color-text-secondary);
  margin: 6px 0;
}

.notification-feed__item small {
  color: var(--color-text-muted);
}
</style>
