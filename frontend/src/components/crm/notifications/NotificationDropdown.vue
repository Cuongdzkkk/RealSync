<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, Check } from '@element-plus/icons-vue';
import { useNotificationStore } from '@/stores/useNotificationStore';
import type { CrmNotification, NotificationReadFilter } from '@/types/crm/notification';
import { parseNotificationData, resolveNotificationLink } from '@/utils/notification';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import NotificationItem from './NotificationItem.vue';

const props = defineProps<{ canWrite: boolean }>();
const store = useNotificationStore();
const router = useRouter();
const activeTab = ref<NotificationReadFilter>('all');

const visibleItems = computed(() => {
  if (activeTab.value === 'unread') return store.previewItems.filter((item) => !item.isRead);
  return store.previewItems;
});

async function openItem(notification: CrmNotification) {
  const opened = props.canWrite ? await store.openNotification(notification.id) : notification;
  const parsed = parseNotificationData(opened?.data ?? notification.data);
  store.closeDropdown();
  router.push(resolveNotificationLink(opened?.link ?? notification.link, parsed));
}

async function markAll() {
  if (!props.canWrite) return;
  await store.markAllAsRead();
}
</script>

<template>
  <section class="notification-dropdown glass-card">
    <header class="notification-dropdown__header">
      <div>
        <strong>Thông báo</strong>
        <span>{{ store.summary.unreadCount }} chưa đọc</span>
      </div>
      <el-tooltip v-if="canWrite" content="Đánh dấu tất cả đã đọc">
        <el-button text :icon="Check" :disabled="store.summary.unreadCount === 0 || store.submitting" @click="markAll">
          Đã đọc
        </el-button>
      </el-tooltip>
    </header>

    <div class="notification-tabs">
      <button :class="{ active: activeTab === 'all' }" @click="activeTab = 'all'">Tất cả</button>
      <button :class="{ active: activeTab === 'unread' }" @click="activeTab = 'unread'">Chưa đọc</button>
    </div>

    <div class="notification-dropdown__body">
      <el-skeleton v-if="store.previewLoading" :rows="4" animated />
      <CrmEmptyState v-else-if="store.error" title="Không tải được thông báo" :description="store.error" />
      <CrmEmptyState v-else-if="visibleItems.length === 0" title="Không có thông báo" description="Các cập nhật CRM mới sẽ xuất hiện tại đây." />
      <NotificationItem
        v-for="item in visibleItems"
        v-else
        :key="item.id"
        :notification="item"
        mode="compact"
        :can-write="canWrite"
        @open="openItem"
      />
    </div>

    <footer>
      <el-button text :icon="ArrowRight" @click="store.closeDropdown(); router.push({ name: 'notification-list' })">
        Xem tất cả thông báo
      </el-button>
    </footer>
  </section>
</template>

<style scoped>
.notification-dropdown {
  background: var(--color-surface);
  border: 1px solid var(--color-border-strong);
  box-shadow: var(--elevation-floating);
  padding: 0;
  position: absolute;
  right: 0;
  top: calc(100% + 8px);
  width: min(390px, calc(100vw - 24px));
  z-index: var(--z-overlay);
}

.notification-dropdown__header {
  align-items: center;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  justify-content: space-between;
  padding: 12px 14px;
}

.notification-dropdown__header > div {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

strong {
  color: var(--color-text-primary);
  font-size: 14px;
}

span {
  color: var(--color-text-muted);
  font-size: 11px;
}

.notification-tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  padding: 8px;
  gap: 6px;
}

.notification-tabs button {
  background: transparent;
  border: 1px solid transparent;
  border-radius: 7px;
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 12px;
  font-weight: 700;
  height: 30px;
}

.notification-tabs button.active {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
}

.notification-dropdown__body {
  max-height: min(430px, calc(100vh - 180px));
  overflow: auto;
}

footer {
  border-top: 1px solid var(--color-divider);
  display: flex;
  justify-content: center;
  padding: 9px;
}

@media (max-width: 560px) {
  .notification-dropdown {
    position: fixed;
    left: 10px;
    right: 10px;
    top: 62px;
    width: auto;
  }
}
</style>
