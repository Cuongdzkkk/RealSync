<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import { Search, Bell, Expand, Fold } from '@element-plus/icons-vue';
import { useAppStore } from '@/stores/useAppStore';

const emit = defineEmits<{
  (e: 'openCommandPalette'): void;
}>();

const appStore = useAppStore();
const route = useRoute();

const pageTitle = computed(() => route.meta.title as string || 'RealSync');
const notifCount = ref(3);
const showNotifs = ref(false);

const tabs = ['Tổng quan', 'Sản phẩm', 'Giao dịch', 'Khách hàng', 'Phân tích'];
const activeTab = 'Sản phẩm';
</script>

<template>
  <header class="topbar">
    <div class="topbar-left">
      <button class="topbar-icon-btn" @click="appStore.toggleSidebar()" :title="appStore.sidebarCollapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'">
        <el-icon :size="18">
          <Fold v-if="!appStore.sidebarCollapsed" />
          <Expand v-else />
        </el-icon>
      </button>
      <div class="topbar-divider-line" />
      <h1 class="page-title">{{ pageTitle }}</h1>
    </div>

    <nav class="topbar-tabs">
      <button
        v-for="tab in tabs"
        :key="tab"
        class="tab-pill"
        :class="{ 'tab-pill--active': tab === activeTab }"
      >
        {{ tab }}
      </button>
    </nav>

    <div class="topbar-right">
      <!-- Search / Command Palette -->
      <button class="topbar-icon-btn topbar-search-btn" @click="emit('openCommandPalette')">
        <el-icon :size="16"><Search /></el-icon>
        <span class="topbar-search-hint">Tìm kiếm...</span>
        <kbd class="topbar-kbd">⌘K</kbd>
      </button>

      <!-- Notifications -->
      <div class="topbar-notif-wrapper">
        <button class="topbar-icon-btn" @click="showNotifs = !showNotifs">
          <el-badge :value="notifCount" :hidden="notifCount === 0" class="topbar-badge">
            <el-icon :size="18"><Bell /></el-icon>
          </el-badge>
        </button>
      </div>

      <!-- Divider -->
      <div class="topbar-divider-line" />

      <!-- CTA -->
      <button class="btn-new">+ Thêm mới</button>

      <!-- User Avatar -->
      <button class="topbar-user">
        <span class="topbar-user__avatar">A</span>
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="topbar-user__chevron">
          <path d="M6 9l6 6 6-6" />
        </svg>
      </button>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  height: 60px;
  background: #FFFFFF;
  border-bottom: 1px solid #E8E8E8;
  position: sticky;
  top: 0;
  z-index: var(--z-sticky);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  gap: 12px;
}

/* ── Left ──────────────────────────────────────── */
.topbar-left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.topbar-icon-btn {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #6B6B6B;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  flex-shrink: 0;
  transition: background var(--duration-fast) var(--ease-standard),
              color var(--duration-fast) var(--ease-standard);
}

.topbar-icon-btn:hover {
  background: #F5F5F5;
  color: #0D0D0D;
}

.topbar-divider-line {
  width: 1px;
  height: 24px;
  background: #E8E8E8;
  flex-shrink: 0;
}

.page-title {
  font-size: 20px;
  font-weight: 700;
  letter-spacing: -0.01em;
  color: #0D0D0D;
  margin: 0;
  white-space: nowrap;
}

/* ── Center: Tab Pills ─────────────────────────── */
.topbar-tabs {
  display: flex;
  align-items: center;
  gap: 4px;
}

.tab-pill {
  font-size: 13px;
  font-weight: 500;
  color: #6B6B6B;
  padding: 6px 14px;
  border-radius: 20px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
}

.tab-pill:hover {
  background: #F5F5F5;
  color: #0D0D0D;
}

.tab-pill--active {
  background: #0D0D0D;
  color: #FFFFFF;
}

.tab-pill--active:hover {
  background: #0D0D0D;
  color: #FFFFFF;
}

/* ── Right ─────────────────────────────────────── */
.topbar-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Search button with hint */
.topbar-search-btn {
  width: auto;
  gap: 8px;
  padding: 0 12px;
  justify-content: flex-start;
  border: 1px solid #E8E8E8;
  border-radius: 8px;
  height: 34px;
}

.topbar-search-btn:hover {
  border-color: #D0D0D0;
}

.topbar-search-btn:focus-visible,
.topbar-search-btn:active {
  border-color: #F5E642;
  background: #FEFBEA;
  box-shadow: 0 0 0 3px rgba(245, 230, 66, 0.25);
}

.topbar-search-hint {
  font-size: 13px;
  color: #ABABAB;
  white-space: nowrap;
}

.topbar-kbd {
  font-family: var(--font-mono);
  font-size: 10px;
  color: #ABABAB;
  background: #F5F5F5;
  border: 1px solid #E8E8E8;
  border-radius: 4px;
  padding: 1px 5px;
  line-height: 1.4;
}

/* Notifications */
.topbar-notif-wrapper {
  position: relative;
}

:deep(.topbar-badge .el-badge__content) {
  font-size: 9px;
  font-weight: 700;
  height: 16px;
  line-height: 16px;
  padding: 0 5px;
  border: 1px solid #FFFFFF;
  background: #DC2626;
}

/* CTA */
.btn-new {
  background: #F5E642;
  color: #0D0D0D;
  font-size: 13px;
  font-weight: 600;
  border: none;
  border-radius: 20px;
  padding: 7px 16px;
  cursor: pointer;
  transition: background var(--duration-fast) var(--ease-standard);
  white-space: nowrap;
}

.btn-new:hover {
  background: #EDD800;
}

/* User Avatar */
.topbar-user {
  display: flex;
  align-items: center;
  gap: 6px;
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px 4px 4px;
  border-radius: 8px;
  transition: background var(--duration-fast) var(--ease-standard);
}

.topbar-user:hover {
  background: #F5F5F5;
}

.topbar-user__avatar {
  width: 28px;
  height: 28px;
  border-radius: 9999px;
  background: #F5E642;
  color: #0D0D0D;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
}

.topbar-user__chevron {
  color: #ABABAB;
  flex-shrink: 0;
}

/* ── Responsive ────────────────────────────────── */
@media (max-width: 900px) {
  .topbar-tabs {
    display: none;
  }
  .topbar-search-hint,
  .topbar-kbd {
    display: none;
  }
  .topbar-search-btn {
    width: 34px;
    padding: 0;
    justify-content: center;
  }
}
</style>
