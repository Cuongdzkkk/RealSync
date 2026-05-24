<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { useAppStore } from '@/stores/useAppStore';
import AppSidebar from './AppSidebar.vue';
import AppTopbar from './AppTopbar.vue';
import CommandPalette from '@/components/common/CommandPalette.vue';
import ToastContainer from '@/components/common/ToastContainer.vue';
import MessagesPanel from '@/components/panels/MessagesPanel.vue';
import { mockMessages } from '@/utils/mockData';

const appStore = useAppStore();
const commandPaletteOpen = ref(false);

function openCommandPalette() {
  commandPaletteOpen.value = true;
}

function closeCommandPalette() {
  commandPaletteOpen.value = false;
}

function handleGlobalKeydown(e: KeyboardEvent) {
  if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
    e.preventDefault();
    commandPaletteOpen.value = !commandPaletteOpen.value;
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleGlobalKeydown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleGlobalKeydown);
});
</script>

<template>
  <div class="admin-shell">
    <AppSidebar />

    <div class="admin-main">
      <AppTopbar @open-command-palette="openCommandPalette" />

      <main class="admin-content">
        <RouterView />
        <aside class="col-right">
          <MessagesPanel :messages="mockMessages" />
        </aside>
      </main>
    </div>

    <CommandPalette
      v-if="commandPaletteOpen"
      @close="closeCommandPalette"
      @select="closeCommandPalette"
    />
    <ToastContainer />
  </div>
</template>

<style scoped>
.admin-shell {
  display: flex;
  min-height: 100dvh;
  background: #FAFAFA;
  font-family: var(--font-ui);
}

.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.admin-content {
  display: grid;
  grid-template-columns: 2fr 1.75fr 1.25fr;
  min-height: calc(100dvh - 60px);
}

/* Child pages render col-left + col-center; right column is built-in */
:deep(.col-left) {
  border-right: 1px solid #E8E8E8;
  padding: 24px;
}

:deep(.col-center) {
  border-right: 1px solid #E8E8E8;
  padding: 24px;
}

.col-right {
  padding: 24px;
  background: var(--color-surface);
  position: sticky;
  top: 60px;
  height: calc(100dvh - 60px);
  overflow-y: auto;
}

/* Full-width pages via .page class or .col-span-2 marker */
:deep(.col-span-2) {
  grid-column: 1 / 3;
  padding: 24px;
  border-right: 1px solid #E8E8E8;
}

:deep(.page) {
  grid-column: 1 / 3;
  padding: 24px;
  border-right: 1px solid #E8E8E8;
}

@media (max-width: 1024px) {
  .admin-content {
    grid-template-columns: 1fr;
  }
  :deep(.col-left), :deep(.col-center) {
    border-right: none;
    border-bottom: 1px solid #E8E8E8;
  }
  .col-right {
    display: none;
  }
}
</style>
