<script setup lang="ts">
import { ref, onMounted } from 'vue';
import AppHeader from './AppHeader.vue';
import AppSidebar from './AppSidebar.vue';
import ToastContainer from './ToastContainer.vue';
import CommandPalette from './CommandPalette.vue';

const isCommandPaletteOpen = ref(false);

const openCommandPalette = () => {
  isCommandPaletteOpen.value = true;
};

const closeCommandPalette = () => {
  isCommandPaletteOpen.value = false;
};

onMounted(() => {
  const handleKeyDown = (e: KeyboardEvent) => {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      openCommandPalette();
    }
  };

  window.addEventListener('keydown', handleKeyDown);
  return () => window.removeEventListener('keydown', handleKeyDown);
});
</script>

<template>
  <div class="admin-layout">
    <AppSidebar />
    <div class="admin-layout__main">
      <AppHeader @open-command-palette="openCommandPalette" />
      <main class="admin-layout__content">
        <RouterView />
      </main>
    </div>
    <ToastContainer />
    <CommandPalette v-if="isCommandPaletteOpen" @close="closeCommandPalette" />
  </div>
</template>

<style scoped>
.admin-layout {
  background: var(--color-canvas);
  display: flex;
  min-height: 100dvh;
}

.admin-layout__main {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

.admin-layout__content {
  margin: 0 auto;
  max-width: 1600px;
  padding: 24px 32px 40px;
  width: 100%;
  flex: 1;
  overflow-y: auto;
}

@media (max-width: 1024px) {
  .admin-layout__content {
    padding: 20px 24px 32px;
  }
}

@media (max-width: 640px) {
  .admin-layout__content {
    padding: 16px 16px 24px;
  }
}
</style>
