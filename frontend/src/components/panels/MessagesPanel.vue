<script setup lang="ts">
import { ref } from 'vue';

export interface MessageThread {
  id: string;
  senderName: string;
  avatarUrl: string;
  preview: string;
  timestamp: string;
  unread: boolean;
}

defineProps<{
  messages: MessageThread[];
}>();

const activeTab = ref('agents');

function formatTime(timestamp: string) {
  const date = new Date(timestamp);
  return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
}
</script>

<template>
  <aside class="messages-panel">
    <!-- Messages Header -->
    <div class="messages-panel__section">
      <h2 class="messages-panel__title">Tin nhắn mới</h2>
      <div class="messages-panel__tabs">
        <button 
          class="messages-panel__tab" 
          :class="{ 'is-active': activeTab === 'agents' }"
          @click="activeTab = 'agents'"
        >
          Agents (9)
        </button>
        <button 
          class="messages-panel__tab" 
          :class="{ 'is-active': activeTab === 'customers' }"
          @click="activeTab = 'customers'"
        >
          Khách hàng 1
        </button>
      </div>
      
      <div class="messages-list">
        <div v-for="msg in messages" :key="msg.id" class="message-item">
          <img :src="msg.avatarUrl" :alt="msg.senderName" class="message-item__avatar" />
          <div class="message-item__content">
            <div class="message-item__header">
              <span class="message-item__name">{{ msg.senderName }}</span>
              <span class="message-item__time">{{ formatTime(msg.timestamp) }}</span>
            </div>
            <div class="message-item__preview" :class="{ 'is-unread': msg.unread }">
              {{ msg.preview }}
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Quick Alerts -->
    <div class="messages-panel__section">
      <h2 class="messages-panel__title">Thông báo hệ thống</h2>
      <div class="system-alerts">
        <div class="system-alert">
          <span class="system-alert__dot system-alert__dot--success"></span>
          <span class="system-alert__text">Crawler chạy thành công lúc 09:20</span>
        </div>
        <div class="system-alert">
          <span class="system-alert__dot system-alert__dot--warning"></span>
          <span class="system-alert__text">2 leads cần chăm sóc lại</span>
        </div>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.messages-panel {
  background: var(--color-surface);
  height: 100%;
  display: flex;
  flex-direction: column;
}

.messages-panel__section {
  padding: 20px 0;
  border-bottom: 1px solid var(--color-border);
}

.messages-panel__section:first-child {
  padding-top: 0;
}

.messages-panel__section:last-child {
  border-bottom: none;
}

.messages-panel__title {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 16px 0;
}

.messages-panel__tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 20px;
}

.messages-panel__tab {
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-secondary);
  background: transparent;
  border: none;
  border-radius: 20px;
  padding: 6px 14px;
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
}

.messages-panel__tab:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.messages-panel__tab.is-active {
  background: var(--color-text-primary);
  color: #FFFFFF;
}

.messages-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.message-item {
  display: flex;
  gap: 12px;
  padding: 10px 0;
  cursor: pointer;
  border-radius: 8px;
  transition: background var(--duration-fast) var(--ease-standard);
}

.message-item:hover {
  background: var(--color-surface-hover);
  margin: 0 -8px;
  padding: 10px 8px;
}

.message-item__avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
  background: var(--color-border);
  flex-shrink: 0;
}

.message-item__content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 2px;
}

.message-item__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.message-item__name {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.message-item__time {
  font-size: 11px;
  color: var(--color-text-muted);
}

.message-item__preview {
  font-size: 12px;
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.message-item__preview.is-unread {
  color: var(--color-text-primary);
  font-weight: 500;
}

.system-alerts {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.system-alert {
  display: flex;
  align-items: center;
  gap: 10px;
}

.system-alert__dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  flex-shrink: 0;
}

.system-alert__dot--success { background: var(--color-success); }
.system-alert__dot--warning { background: var(--color-warning); }

.system-alert__text {
  font-size: 12px;
  color: var(--color-text-secondary);
  line-height: 1.4;
}
</style>
