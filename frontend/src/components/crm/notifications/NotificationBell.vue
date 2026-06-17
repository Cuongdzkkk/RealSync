<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { Bell } from '@element-plus/icons-vue';
import { useAuthStore } from '@/stores/useAuthStore';
import { useNotificationStore } from '@/stores/useNotificationStore';
import { getNotificationUserId } from '@/utils/notification';
import NotificationDropdown from './NotificationDropdown.vue';

const authStore = useAuthStore();
const store = useNotificationStore();
const root = ref<HTMLElement | null>(null);

const userId = computed(() => getNotificationUserId(authStore.user));
const role = computed(() => authStore.user?.role ?? 'Viewer');
const canWrite = computed(() => role.value !== 'Viewer');
const badge = computed(() => {
  const count = store.summary.unreadCount;
  if (count <= 0) return '';
  return count > 99 ? '99+' : String(count);
});
const ariaLabel = computed(() => {
  const count = store.summary.unreadCount;
  return count > 0 ? `Thông báo, ${count} chưa đọc` : 'Thông báo';
});

function onDocumentClick(event: MouseEvent) {
  if (!root.value?.contains(event.target as Node)) store.closeDropdown();
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') store.closeDropdown();
}

function toggle() {
  store.toggleDropdown();
  if (store.dropdownOpen) store.fetchPreview();
}

watch(
  userId,
  (value) => {
    store.initializeForUser(value);
  },
  { immediate: true }
);

onMounted(() => {
  document.addEventListener('click', onDocumentClick);
  document.addEventListener('keydown', onKeydown);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick);
  document.removeEventListener('keydown', onKeydown);
});
</script>

<template>
  <div ref="root" class="notification-bell">
    <el-tooltip content="Thông báo">
      <button class="topbar-icon-btn notification-bell__button" :aria-label="ariaLabel" @click.stop="toggle">
        <Bell />
        <span v-if="badge" class="notification-bell__badge">{{ badge }}</span>
      </button>
    </el-tooltip>
    <NotificationDropdown v-if="store.dropdownOpen" :can-write="canWrite" />
  </div>
</template>

<style scoped>
.notification-bell {
  position: relative;
}

.notification-bell__button {
  align-items: center;
  background: transparent;
  border: 1px solid transparent;
  border-radius: 8px;
  color: var(--color-text-secondary);
  cursor: pointer;
  display: flex;
  flex-shrink: 0;
  height: 34px;
  justify-content: center;
  position: relative;
  transition: all var(--duration-fast) var(--ease-standard);
  width: 34px;
}

.notification-bell__button:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
  color: var(--color-text-primary);
}

.notification-bell__button :deep(svg) {
  height: 18px;
  width: 18px;
}

.notification-bell__badge {
  align-items: center;
  background: var(--color-danger);
  border: 1px solid var(--color-surface);
  border-radius: 99px;
  color: #ffffff;
  display: flex;
  font-size: 9px;
  font-weight: 800;
  height: 15px;
  justify-content: center;
  min-width: 15px;
  padding: 0 3px;
  position: absolute;
  right: 2px;
  top: 2px;
}
</style>
