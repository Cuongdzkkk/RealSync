<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAppStore } from '@/stores/useAppStore';
import { useAuthStore } from '@/stores/useAuthStore';

const appStore = useAppStore();
const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

interface SidebarIcon {
  label: string;
  to: string;
  icon: string;
  ai?: boolean;
}

const navIcons: SidebarIcon[] = [
  {
    label: 'Tổng quan',
    to: '/admin/dashboard',
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1'
  },
  {
    label: 'Sản phẩm',
    to: '/admin/properties',
    icon: 'M19 3H5a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2V5a2 2 0 00-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z'
  },
  {
    label: 'Dự án',
    to: '/admin/projects',
    icon: 'M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z'
  },
  {
    label: 'Khách hàng',
    to: '/admin/leads',
    icon: 'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75'
  },
  {
    label: 'Máy thu thập',
    to: '/admin/crawlers',
    icon: 'M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1'
  },
  {
    label: 'Phân loại AI',
    to: '/admin/ai-classification',
    icon: 'M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714a2.25 2.25 0 00.659 1.591L19 14.5M14.25 3.104c.251.023.501.05.75.082M19 14.5l-1.471 4.412a1.5 1.5 0 01-1.423 1.026H7.894a1.5 1.5 0 01-1.423-1.026L5 14.5m14 0H5',
    ai: true
  },
  {
    label: 'Nội dung AI',
    to: '/admin/content-ai',
    icon: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z',
    ai: true
  }
];

const systemIcons: SidebarIcon[] = [
  {
    label: 'Cài đặt',
    to: '/admin/settings',
    icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z'
  }
];

const collapsed = computed(() => appStore.sidebarCollapsed);
const isActive = (to: string) => route.path === to || route.path.startsWith(to + '/');

const userInitial = computed(() => authStore.user?.fullName?.charAt(0) ?? 'A');

function navigateTo(to: string) {
  router.push(to);
}
</script>

<template>
  <aside class="sidebar" :class="{ 'sidebar--collapsed': collapsed }">
    <!-- Logo -->
    <RouterLink class="sidebar-logo" to="/admin/dashboard" aria-label="RealSync Home">
      <span class="sidebar-logo-mark">RS</span>
      <span v-if="!collapsed" class="sidebar-logo-text">RealSync</span>
    </RouterLink>

    <!-- Main Nav -->
    <nav class="sidebar-nav" aria-label="Điều hướng chính">
      <button
        v-for="item in navIcons"
        :key="item.to"
        class="sidebar-btn"
        :class="{
          'sidebar-btn--active': isActive(item.to),
          'sidebar-btn--ai': item.ai
        }"
        :data-tooltip="collapsed ? item.label : undefined"
        :aria-label="item.label"
        @click="navigateTo(item.to)"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path :d="item.icon" />
        </svg>
        <span v-if="!collapsed" class="sidebar-btn__label">{{ item.label }}</span>
      </button>
    </nav>

    <!-- Divider -->
    <div class="sidebar-divider" />

    <!-- System Icons -->
    <div class="sidebar-system">
      <button
        v-for="item in systemIcons"
        :key="item.to"
        class="sidebar-btn"
        :class="{ 'sidebar-btn--active': isActive(item.to) }"
        :data-tooltip="collapsed ? item.label : undefined"
        :aria-label="item.label"
        @click="navigateTo(item.to)"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path :d="item.icon" />
        </svg>
        <span v-if="!collapsed" class="sidebar-btn__label">{{ item.label }}</span>
      </button>
    </div>

    <!-- Bottom: Toggle + Avatar -->
    <div class="sidebar-bottom">
      <button class="sidebar-toggle" :data-tooltip="collapsed ? 'Mở rộng' : 'Thu gọn'" @click="appStore.toggleSidebar()">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path v-if="collapsed" d="M9 18l6-6-6-6" />
          <path v-else d="M15 18l-6-6 6-6" />
        </svg>
      </button>
      <button
        class="sidebar-avatar"
        :data-tooltip="collapsed ? (authStore.user?.fullName ?? 'Guest') : undefined"
        aria-label="Tài khoản"
      >
        {{ userInitial }}
        <span v-if="!collapsed" class="sidebar-avatar__name">{{ authStore.user?.fullName ?? 'Guest' }}</span>
      </button>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 200px;
  background: #FFFFFF;
  border-right: 1px solid #E8E8E8;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  height: 100dvh;
  position: sticky;
  top: 0;
  z-index: var(--z-sidebar);
  padding: 12px 10px;
  gap: 2px;
  transition: width var(--duration-base) var(--ease-standard);
  overflow: hidden;
}

.sidebar--collapsed {
  width: 56px;
  padding: 12px 0;
  align-items: center;
}

/* ── Logo ──────────────────────────────────────── */
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
  margin-bottom: 12px;
  padding: 0 2px;
  text-decoration: none;
  min-height: 36px;
}

.sidebar-logo-mark {
  width: 36px;
  height: 36px;
  min-width: 36px;
  background: #F5E642;
  border-radius: 8px;
  color: #0D0D0D;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.sidebar-logo-text {
  font-size: 15px;
  font-weight: 700;
  color: #0D0D0D;
  white-space: nowrap;
}

.sidebar--collapsed .sidebar-logo {
  justify-content: center;
  padding: 0;
}

/* ── Navigation ────────────────────────────────── */
.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 4px 0;
}

.sidebar--collapsed .sidebar-nav {
  align-items: center;
}

/* ── Icon Button ───────────────────────────────── */
.sidebar-btn {
  position: relative;
  width: 100%;
  min-height: 40px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #9E9E9E;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 10px;
  flex-shrink: 0;
  transition:
    background var(--duration-fast) var(--ease-standard),
    color var(--duration-fast) var(--ease-standard);
}

.sidebar-btn:hover {
  background: #F5F5F5;
  color: #0D0D0D;
}

.sidebar-btn--active {
  background: #F5E642;
  color: #0D0D0D;
}

.sidebar-btn--active:hover {
  background: #EDD800;
}

.sidebar-btn--ai {
  color: var(--color-ai);
}

.sidebar-btn--ai.sidebar-btn--active {
  color: #0D0D0D;
}

.sidebar-btn__label {
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
  line-height: 1;
}

.sidebar--collapsed .sidebar-btn {
  width: 40px;
  justify-content: center;
  padding: 0;
}

/* ── Tooltip (collapsed only) ──────────────────── */
.sidebar-btn[data-tooltip]::after,
.sidebar-toggle[data-tooltip]::after {
  content: attr(data-tooltip);
  position: absolute;
  left: calc(100% + 10px);
  top: 50%;
  transform: translateY(-50%);
  background: #0D0D0D;
  color: #FFFFFF;
  font-size: 12px;
  font-weight: 500;
  line-height: 1.3;
  padding: 4px 10px;
  border-radius: 6px;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity var(--duration-fast) var(--ease-standard);
  z-index: calc(var(--z-sidebar) + 1);
}

.sidebar-btn[data-tooltip]:hover::after,
.sidebar-toggle[data-tooltip]:hover::after {
  opacity: 1;
}

/* ── Divider ───────────────────────────────────── */
.sidebar-divider {
  width: 100%;
  height: 1px;
  background: #E8E8E8;
  flex-shrink: 0;
  margin: 4px 0;
}

.sidebar--collapsed .sidebar-divider {
  width: 24px;
}

/* ── System Icons ──────────────────────────────── */
.sidebar-system {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex-shrink: 0;
}

.sidebar--collapsed .sidebar-system {
  align-items: center;
}

/* ── Bottom: Toggle + Avatar ───────────────────── */
.sidebar-bottom {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex-shrink: 0;
  margin-top: 8px;
}

.sidebar--collapsed .sidebar-bottom {
  align-items: center;
}

.sidebar-toggle {
  position: relative;
  width: 100%;
  height: 32px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #9E9E9E;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background var(--duration-fast) var(--ease-standard),
              color var(--duration-fast) var(--ease-standard);
}

.sidebar-toggle:hover {
  background: #F5F5F5;
  color: #0D0D0D;
}

.sidebar--collapsed .sidebar-toggle {
  width: 32px;
}

.sidebar-avatar {
  position: relative;
  width: 100%;
  height: 36px;
  border-radius: 8px;
  background: #F5E642;
  color: #0D0D0D;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 8px;
  font-size: 13px;
  font-weight: 700;
  transition: box-shadow var(--duration-fast) var(--ease-standard);
}

.sidebar-avatar:hover {
  box-shadow: 0 0 0 2px var(--ring-color);
}

.sidebar-avatar__name {
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar--collapsed .sidebar-avatar {
  width: 32px;
  height: 32px;
  border-radius: 9999px;
  justify-content: center;
  padding: 0;
}

/* ── Mobile ────────────────────────────────────── */
@media (max-width: 640px) {
  .sidebar {
    display: none;
  }
}
</style>
