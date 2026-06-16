<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAppStore } from '@/stores/useAppStore';
import { useAuthStore } from '@/stores/useAuthStore';

const appStore = useAppStore();
const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

interface NavItem {
  label: string;
  to: string;
  icon: string; // SVG path
  group: 'crm' | 'properties' | 'ai' | 'crawler' | 'analytics' | 'admin';
  roles: ('Admin' | 'Manager' | 'Sales' | 'Marketing' | 'Data Analyst')[];
  badge?: string;
  isAi?: boolean;
}

const navItems: NavItem[] = [
  // Dashboard is visible to all, but displays custom KPIs based on role
  {
    label: 'Tổng quan',
    to: '/admin/dashboard',
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1',
    group: 'crm',
    roles: ['Admin', 'Manager', 'Sales', 'Marketing', 'Data Analyst']
  },
  // CRM Group
  {
    label: 'Khách hàng (Leads)',
    to: '/admin/leads',
    icon: 'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75',
    group: 'crm',
    roles: ['Admin', 'Manager', 'Sales'],
    badge: 'Hot'
  },
  // Properties Group
  {
    label: 'Bất động sản',
    to: '/admin/properties',
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3',
    group: 'properties',
    roles: ['Admin', 'Manager', 'Sales', 'Marketing', 'Data Analyst']
  },
  {
    label: 'Danh mục BĐS',
    to: '/admin/property-categories',
    icon: 'M3 7a2 2 0 012-2h5l2 2h7a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V7z',
    group: 'properties',
    roles: ['Admin', 'Manager', 'Marketing']
  },
  {
    label: 'Dự án',
    to: '/admin/projects',
    icon: 'M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5',
    group: 'properties',
    roles: ['Admin', 'Marketing']
  },
  // AI Center Group
  {
    label: 'Phân loại AI',
    to: '/admin/ai-classification',
    icon: 'M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0',
    group: 'ai',
    roles: ['Admin', 'Manager', 'Sales'],
    isAi: true
  },
  {
    label: 'Nội dung AI',
    to: '/admin/content-ai',
    icon: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z',
    group: 'ai',
    roles: ['Admin', 'Sales', 'Marketing'],
    isAi: true
  },
  // Crawler Group
  {
    label: 'Crawler Data',
    to: '/admin/crawlers',
    icon: 'M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1',
    group: 'crawler',
    roles: ['Admin', 'Marketing', 'Data Analyst']
  },
  // Admin Group
  {
    label: 'Cài đặt hệ thống',
    to: '/admin/settings',
    icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573',
    group: 'admin',
    roles: ['Admin']
  }
];

const collapsed = computed(() => appStore.sidebarCollapsed);
const currentRole = computed(() => authStore.user?.role ?? 'Guest');

const showProfilePopup = ref(false);

function goToSettings() {
  showProfilePopup.value = false;
  router.push('/admin/settings');
}

function handleLogout() {
  showProfilePopup.value = false;
  authStore.logout();
  router.push('/login');
}

// Filter nav items based on user role
const filteredNavItems = computed(() => {
  return navItems.filter(item => item.roles.includes(currentRole.value as any));
});

const isActive = (to: string) => route.path === to || route.path.startsWith(to + '/');

function navigateTo(to: string) {
  router.push(to);
}
</script>

<template>
  <aside class="sidebar glass-card" :class="{ 'sidebar--collapsed': collapsed }">
    <!-- Brand Logo -->
    <div class="sidebar-logo">
      <div class="logo-mark glow-yellow">
        <span>RS</span>
        <!-- Small animated pulse for AI-powered feel -->
        <span class="logo-pulse"></span>
      </div>
      <div v-if="!collapsed" class="logo-text-wrapper">
        <span class="logo-title">RealSync</span>
        <span class="logo-subtitle">AI Platform</span>
      </div>
    </div>

    <!-- Divider -->
    <div class="sidebar-divider" />

    <!-- Navigation Lists grouped by type -->
    <nav class="sidebar-nav">
      <div class="nav-section">
        <span v-if="!collapsed" class="nav-section-title">Menu quản lý</span>
        <div class="nav-items-list">
          <button
            v-for="item in filteredNavItems"
            :key="item.to"
            class="sidebar-btn"
            :class="{
              'sidebar-btn--active': isActive(item.to),
              'sidebar-btn--ai': item.isAi
            }"
            :data-tooltip="collapsed ? item.label : undefined"
            :aria-label="item.label"
            @click="navigateTo(item.to)"
          >
            <div class="icon-container">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <path :d="item.icon" />
              </svg>
              <!-- AI glowing dot -->
              <span v-if="item.isAi" class="ai-dot"></span>
            </div>
            
            <span v-if="!collapsed" class="sidebar-btn__label">{{ item.label }}</span>
            
            <span v-if="item.badge && !collapsed" class="item-badge">{{ item.badge }}</span>
          </button>
        </div>
      </div>
    </nav>

    <!-- Bottom Actions / Profile info -->
    <div class="sidebar-footer">
      <button 
        class="sidebar-toggle-btn" 
        :title="collapsed ? 'Mở rộng menu' : 'Thu gọn menu'"
        @click="appStore.toggleSidebar()"
      >
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path v-if="collapsed" d="M9 18l6-6-6-6" />
          <path v-else d="M15 18l-6-6 6-6" />
        </svg>
        <span v-if="!collapsed">Thu gọn</span>
      </button>

      <!-- Profile click-away backdrop overlay -->
      <div v-if="showProfilePopup" class="profile-backdrop" @click="showProfilePopup = false"></div>

      <!-- Floating profile popover -->
      <div v-if="showProfilePopup" class="profile-popup glass-card animate-slide-up">
        <div class="popup-header">
          <div class="user-avatar glow-yellow large">
            {{ authStore.user?.fullName?.charAt(0) ?? 'G' }}
          </div>
          <div class="popup-user-meta">
            <h4>{{ authStore.user?.fullName ?? 'Guest' }}</h4>
            <span class="popup-email">{{ authStore.user?.email ?? 'admin@realsync.vn' }}</span>
          </div>
        </div>

        <div class="popup-divider"></div>

        <div class="popup-body">
          <div class="popup-info-item">
            <span class="info-label">Vai trò</span>
            <span class="info-val role-badge">{{ currentRole }}</span>
          </div>
          <div class="popup-info-item">
            <span class="info-label">Trạng thái</span>
            <span class="info-val status-badge active">Online</span>
          </div>
          <div class="popup-info-item">
            <span class="info-label">Workspace</span>
            <span class="info-val workspace-name">RealSync HQ</span>
          </div>
        </div>

        <div class="popup-divider"></div>

        <div class="popup-actions">
          <button class="popup-action-btn" @click="goToSettings">
            ⚙️ Cài đặt tài khoản
          </button>
          <button class="popup-action-btn danger" @click="handleLogout">
            🚪 Đăng xuất
          </button>
        </div>
      </div>

      <div 
        class="sidebar-user-section clickable-user"
        :class="{ active: showProfilePopup }"
        @click="showProfilePopup = !showProfilePopup"
      >
        <div class="user-avatar glow-yellow">
          {{ authStore.user?.fullName?.charAt(0) ?? 'G' }}
        </div>
        <div v-if="!collapsed" class="user-meta">
          <div class="user-name">{{ authStore.user?.fullName ?? 'Guest' }}</div>
          <div class="user-role">{{ currentRole }}</div>
        </div>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 220px;
  height: 100vh;
  position: sticky;
  top: 0;
  z-index: var(--z-sidebar);
  display: flex;
  flex-direction: column;
  background: var(--color-sidebar-bg);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-right: 1px solid var(--color-sidebar-border);
  border-left: none;
  border-top: none;
  border-bottom: none;
  border-radius: 0;
  padding: 16px 12px;
  gap: 16px;
  box-shadow: none;
  transition: width var(--duration-base) var(--ease-spring);
}

.sidebar--collapsed {
  width: 68px;
  padding: 16px 8px;
  align-items: center;
}

/* ── Logo ── */
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 4px;
}

.logo-mark {
  width: 36px;
  height: 36px;
  min-width: 36px;
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 800;
  letter-spacing: -0.05em;
  position: relative;
}

.logo-pulse {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: var(--color-ai);
  border: 1.5px solid var(--color-yellow);
  animation: pulse-ring 2.5s infinite;
}

.logo-text-wrapper {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.logo-title {
  font-size: 15px;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
  line-height: 1.1;
}

.logo-subtitle {
  font-size: 9px;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-ai);
  letter-spacing: 0.05em;
}

.sidebar-divider {
  height: 1px;
  background-color: var(--color-divider);
  width: 100%;
}

/* ── Nav List ── */
.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
  overflow-y: auto;
  overflow-x: hidden;
}

.nav-section {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.nav-section-title {
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  letter-spacing: 0.05em;
  padding-left: 8px;
  margin-bottom: 2px;
}

.nav-items-list {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

/* ── Menu Buttons ── */
.sidebar-btn {
  width: 100%;
  height: 38px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 10px;
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-secondary);
  border-radius: 8px;
  cursor: pointer;
  position: relative;
  transition: all var(--duration-fast) var(--ease-standard);
}

.sidebar-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
  border-color: var(--color-border);
}

.sidebar-btn--active {
  background: var(--color-yellow) !important;
  color: var(--color-yellow-text) !important;
  font-weight: 600;
  box-shadow: var(--color-yellow-glow);
}

.sidebar-btn--active:hover {
  background: var(--color-yellow-hover) !important;
}

.sidebar-btn--ai {
  color: var(--color-ai);
}

.sidebar-btn--ai.sidebar-btn--active {
  color: var(--color-yellow-text);
}

.icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  flex-shrink: 0;
}

.ai-dot {
  position: absolute;
  top: -1px;
  right: -1px;
  width: 4px;
  height: 4px;
  border-radius: 50%;
  background-color: var(--color-ai);
}

.sidebar-btn__label {
  font-size: 12.5px;
  white-space: nowrap;
}

.item-badge {
  margin-left: auto;
  font-size: 9px;
  font-weight: 700;
  background-color: var(--color-danger-bg);
  color: var(--color-danger);
  border: 1px solid var(--color-danger-border);
  padding: 1px 5px;
  border-radius: 4px;
  text-transform: uppercase;
}

.sidebar--collapsed .sidebar-btn {
  width: 42px;
  height: 42px;
  justify-content: center;
  padding: 0;
}

/* Tooltip on collapsed state */
.sidebar-btn[data-tooltip]::after {
  content: attr(data-tooltip);
  position: absolute;
  left: calc(100% + 12px);
  top: 50%;
  transform: translateY(-50%);
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  font-size: 11px;
  font-weight: 600;
  padding: 5px 10px;
  border-radius: 6px;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  box-shadow: var(--elevation-floating);
  transition: opacity var(--duration-fast) var(--ease-standard);
  z-index: 999;
}

.sidebar-btn[data-tooltip]:hover::after {
  opacity: 1;
}

/* ── Footer ── */
.sidebar-footer {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: auto;
}

.sidebar-toggle-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px;
  transition: all var(--duration-fast);
}

.sidebar-toggle-btn:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

.sidebar--collapsed .sidebar-toggle-btn {
  width: 42px;
  justify-content: center;
  padding: 8px 0;
}

.sidebar--collapsed .sidebar-toggle-btn span {
  display: none;
}

.sidebar-user-section {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 4px;
  border-top: 1px solid var(--color-divider);
}

.sidebar--collapsed .sidebar-user-section {
  justify-content: center;
  border-top: none;
  padding: 6px 0;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.user-meta {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.user-name {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 9px;
  color: var(--color-text-muted);
  text-transform: uppercase;
  font-weight: 600;
}

.clickable-user {
  cursor: pointer;
  border-radius: 8px;
  padding: 8px;
  transition: background-color var(--duration-fast);
}

.clickable-user:hover,
.clickable-user.active {
  background-color: var(--color-surface-hover);
}

.profile-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 999;
  background: transparent;
}

.profile-popup {
  position: absolute;
  bottom: 64px;
  left: 12px;
  width: calc(100% - 24px);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: var(--elevation-floating);
  z-index: 1000;
  border: 1px solid var(--color-border);
}

.sidebar--collapsed .profile-popup {
  left: 68px;
  bottom: 16px;
  width: 240px;
}

.popup-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar.large {
  width: 44px;
  height: 44px;
  font-size: 16px;
}

.popup-user-meta {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.popup-user-meta h4 {
  margin: 0;
  font-size: 13.5px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.popup-email {
  font-size: 11px;
  color: var(--color-text-muted);
}

.popup-divider {
  height: 1px;
  background-color: var(--color-divider);
}

.popup-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.popup-info-item {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
}

.info-label {
  color: var(--color-text-muted);
}

.info-val {
  font-weight: 600;
  color: var(--color-text-primary);
}

.info-val.role-badge {
  color: var(--color-yellow-hover);
  text-transform: uppercase;
  font-size: 10px;
  font-weight: 700;
}

.info-val.status-badge {
  color: var(--color-success);
  font-size: 10px;
  font-weight: 700;
}

.popup-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.popup-action-btn {
  height: 32px;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  color: var(--color-text-primary);
  font-size: 11.5px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all var(--duration-fast);
}

.popup-action-btn:hover {
  background-color: var(--color-surface-hover);
}

.popup-action-btn.danger {
  color: var(--color-danger);
  border-color: rgba(239, 68, 68, 0.2);
}

.popup-action-btn.danger:hover {
  background-color: rgba(239, 68, 68, 0.08);
}
</style>
