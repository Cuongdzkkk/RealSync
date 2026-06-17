<script setup lang="ts">
import { computed } from 'vue';
import { formatRelativeDate, getFollowUpState } from '@/utils/crm';
import type { LeadStatus } from '@/types/crm/lead';

const props = defineProps<{
  date?: string | null;
  status?: LeadStatus;
}>();

const state = computed(() => getFollowUpState(props.date, props.status));
const label = computed(() => {
  if (state.value === 'none') return 'Không có lịch';
  if (state.value === 'inactive') return `Đã đóng: ${formatRelativeDate(props.date)}`;
  if (state.value === 'overdue') return `Quá hạn: ${formatRelativeDate(props.date)}`;
  if (state.value === 'today') return `Hôm nay: ${formatRelativeDate(props.date)}`;
  return formatRelativeDate(props.date);
});
</script>

<template>
  <span class="follow-up" :class="`follow-up--${state}`">{{ label }}</span>
</template>

<style scoped>
.follow-up {
  border: 1px solid var(--color-border);
  border-radius: 999px;
  display: inline-flex;
  font-size: 11px;
  font-weight: 700;
  line-height: 1.2;
  padding: 6px 9px;
  white-space: nowrap;
}

.follow-up--overdue { background: var(--color-danger-bg); border-color: var(--color-danger-border); color: var(--color-danger); }
.follow-up--today { background: var(--color-warning-bg); border-color: var(--color-warning-border); color: var(--color-warning); }
.follow-up--upcoming { background: var(--color-info-bg); border-color: var(--color-info-border); color: var(--color-info); }
.follow-up--none,
.follow-up--inactive { background: var(--color-surface-glass); color: var(--color-text-muted); }
</style>
