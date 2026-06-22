<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAppStore } from '@/stores/useAppStore';
import { useAuthStore } from '@/stores/useAuthStore';
import NotificationBell from '@/components/crm/notifications/NotificationBell.vue';

const emit = defineEmits<{
  (e: 'openCommandPalette'): void;
}>();

const appStore = useAppStore();
const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();

const pageTitle = computed(() => route.meta.title as string || 'RealSync');
const showRoleDropdown = ref(false);

const availableRoles = ['Admin', 'Manager', 'Sales', 'Agent', 'Viewer', 'Marketing', 'Data Analyst'] as const;

function switchRole(role: typeof availableRoles[number]) {
  if (authStore.user) {
    authStore.user.role = role;
    if (role === 'Admin') {
      authStore.user.id = 'local-admin';
      authStore.user.fullName = 'RealSync Admin';
      authStore.user.email = 'admin@realsync.vn';
    } else if (role === 'Manager') {
      authStore.user.id = 'local-manager';
      authStore.user.fullName = 'Trần Kinh Doanh (Manager)';
      authStore.user.email = 'manager@realsync.vn';
    } else if (role === 'Sales') {
      authStore.user.id = 'local-sales';
      authStore.user.fullName = 'Lê Thị Sales (Sales)';
      authStore.user.email = 'sales.anh@realsync.vn';
    } else if (role === 'Agent') {
      authStore.user.id = 'local-agent';
      authStore.user.fullName = 'Phạm Ngọc Agent (Agent)';
      authStore.user.email = 'agent.ngoc@realsync.vn';
    } else if (role === 'Viewer') {
      authStore.user.id = 'local-viewer';
      authStore.user.fullName = 'Vũ Viewer (Viewer)';
      authStore.user.email = 'viewer@realsync.vn';
    } else if (role === 'Marketing') {
      authStore.user.id = 'local-marketing';
      authStore.user.fullName = 'Hoàng Quảng Cáo (Marketing)';
      authStore.user.email = 'marketing@realsync.vn';
    } else {
      authStore.user.id = 'local-analyst';
      authStore.user.fullName = 'Phan Phân Tích (Analyst)';
      authStore.user.email = 'analyst@realsync.vn';
    }
  }
  showRoleDropdown.value = false;
}

function openCreateLead() {
  router.push({
    name: 'lead-list',
    query: { action: 'create' }
  });
}
</script>

<template>
  <header class="topbar glass-nav">
    <div class="topbar-left">
      <button class="topbar-icon-btn" @click="appStore.toggleSidebar()" :title="appStore.sidebarCollapsed ? 'Mở rộng' : 'Thu gọn'">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
          <line v-if="!appStore.sidebarCollapsed" x1="3" y1="12" x2="21" y2="12" />
          <line v-if="!appStore.sidebarCollapsed" x1="3" y1="6" x2="21" y2="6" />
          <line v-if="!appStore.sidebarCollapsed" x1="3" y1="18" x2="21" y2="18" />
          
          <path v-else d="M4 12h16M4 6h16M4 18h16" />
        </svg>
      </button>
      <div class="topbar-divider-line" />
      <h1 class="page-title">{{ pageTitle }}</h1>
      
      <span class="role-badge" :class="`role-badge--${authStore.user?.role?.toLowerCase().replace(' ', '-')}`">
        {{ authStore.user?.role }}
      </span>
    </div>

    <div class="topbar-right">
      <!-- Search / Command Palette (Linear-style) -->
      <button class="topbar-search-btn" @click="emit('openCommandPalette')">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8" />
          <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
        <span class="topbar-search-hint">Tìm kiếm lệnh...</span>
        <kbd class="topbar-kbd">⌘K</kbd>
      </button>

      <!-- Theme Switcher (Light/Dark) -->
      <button class="topbar-icon-btn" @click="appStore.toggleTheme()" :title="appStore.theme === 'light' ? 'Chuyển sang chế độ tối' : 'Chuyển sang chế độ sáng'">
        <svg v-if="appStore.theme === 'light'" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
        </svg>
        <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="5" />
          <line x1="12" y1="1" x2="12" y2="3" />
          <line x1="12" y1="21" x2="12" y2="23" />
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
          <line x1="1" y1="12" x2="3" y2="12" />
          <line x1="21" y1="12" x2="23" y2="12" />
          <line x1="4.22" y1="19.07" x2="5.64" y2="17.66" />
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
        </svg>
      </button>

      <NotificationBell />

      <!-- Divider -->
      <div class="topbar-divider-line" />

      <!-- Role Switcher for Testing (HubSpot/Linear theme testing) -->
      <div class="role-selector-container">
        <button class="role-switcher-btn" @click="showRoleDropdown = !showRoleDropdown">
          <span>Đổi vai trò</span>
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="6 9 12 15 18 9" />
          </svg>
        </button>
        <div v-if="showRoleDropdown" class="role-dropdown-menu glass-card">
          <button 
            v-for="role in availableRoles" 
            :key="role" 
            class="dropdown-item"
            :class="{ 'is-active': authStore.user?.role === role }"
            @click="switchRole(role)"
          >
            {{ role }}
          </button>
        </div>
      </div>

      <!-- Quick Action + CTA -->
      <button class="btn-new glow-yellow" @click="openCreateLead">+ Thêm Lead</button>
    </div>
  </header>
</template>

<style scoped>
.topbar {
  height: 60px;
  background: var(--color-surface);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  border-bottom: 1px solid var(--color-border);
  position: sticky;
  top: 0;
  z-index: var(--z-sticky);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  gap: 12px;
  transition: background-color var(--duration-base) var(--ease-standard),
              border-color var(--duration-base) var(--ease-standard);
}

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
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  flex-shrink: 0;
  position: relative;
  transition: all var(--duration-fast) var(--ease-standard);
}

.topbar-icon-btn:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
  color: var(--color-text-primary);
}

.topbar-divider-line {
  width: 1px;
  height: 20px;
  background: var(--color-border);
  flex-shrink: 0;
}

.page-title {
  font-size: 16px;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
  margin: 0;
  white-space: nowrap;
}

.role-badge {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 6px;
  letter-spacing: 0.02em;
  text-transform: uppercase;
}
.role-badge--admin { background: var(--color-yellow-muted); color: var(--color-yellow-hover); border: 1px solid rgba(250, 204, 21, 0.2); }
.role-badge--manager { background: var(--color-info-bg); color: var(--color-info); border: 1px solid var(--color-info-border); }
.role-badge--sales { background: var(--color-success-bg); color: var(--color-success); border: 1px solid var(--color-success-border); }
.role-badge--agent { background: var(--color-surface-glass); color: var(--color-text-secondary); border: 1px solid var(--color-border); }
.role-badge--viewer { background: var(--color-surface-glass); color: var(--color-text-muted); border: 1px solid var(--color-border); }
.role-badge--marketing { background: rgba(168, 85, 247, 0.08); color: rgb(168, 85, 247); border: 1px solid rgba(168, 85, 247, 0.2); }
.role-badge--data-analyst { background: var(--color-ai-bg); color: var(--color-ai); border: 1px solid var(--color-ai-border); }

.topbar-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Linear-style Search Input button */
.topbar-search-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 10px;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  height: 34px;
  cursor: pointer;
  color: var(--color-text-secondary);
  transition: all var(--duration-fast) var(--ease-standard);
}

.topbar-search-btn:hover {
  border-color: var(--color-border-strong);
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.topbar-search-hint {
  font-size: 12px;
  color: var(--color-text-muted);
}

.topbar-kbd {
  font-family: var(--font-mono);
  font-size: 9px;
  color: var(--color-text-muted);
  background: var(--color-divider);
  border: 1px solid var(--color-border);
  border-radius: 4px;
  padding: 1px 4px;
  line-height: 1.2;
}

/* Role Selector */
.role-selector-container {
  position: relative;
}

.role-switcher-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  height: 34px;
  padding: 0 10px;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
}

.role-switcher-btn:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-strong);
  color: var(--color-text-primary);
}

.role-dropdown-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  width: 155px;
  display: flex;
  flex-direction: column;
  padding: 6px;
  z-index: var(--z-overlay);
  background: rgba(255, 255, 255, 0.98) !important;
  border: 1px solid var(--color-border-strong) !important;
  box-shadow: var(--elevation-floating) !important;
  backdrop-filter: blur(8px) !important;
}

[data-theme="dark"] .role-dropdown-menu,
.dark .role-dropdown-menu {
  background: rgba(15, 23, 42, 0.98) !important;
}

.dropdown-item {
  background: transparent;
  border: none;
  border-radius: 6px;
  padding: 8px 12px;
  font-size: 12px;
  text-align: left;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--duration-fast);
  font-weight: 500;
}

.dropdown-item:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.dropdown-item.is-active {
  background: var(--color-yellow) !important;
  color: var(--color-yellow-text) !important;
  font-weight: 700;
}

/* CTA */
.btn-new {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12px;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  padding: 0 14px;
  height: 34px;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all var(--duration-fast) var(--ease-standard);
  white-space: nowrap;
}

.btn-new:hover {
  background: var(--color-yellow-hover);
  transform: scale(1.02);
}

@media (max-width: 900px) {
  .topbar-search-hint,
  .topbar-kbd {
    display: none;
  }
}
</style>
