<script setup lang="ts">
import { computed } from 'vue';
import { Close } from '@element-plus/icons-vue';
import { useToast } from '@/composables/useToast';

const toastStore = useToast();

const toastColors = {
  success: {
    icon: '✓',
    bg: 'var(--color-success-bg)',
    text: 'var(--color-success)',
    border: 'var(--color-success-border)',
  },
  warning: {
    icon: '⚠',
    bg: 'var(--color-warning-bg)',
    text: 'var(--color-warning)',
    border: 'var(--color-warning-border)',
  },
  error: {
    icon: '✕',
    bg: 'var(--color-danger-bg)',
    text: 'var(--color-danger)',
    border: 'var(--color-danger-border)',
  },
  info: {
    icon: 'ℹ',
    bg: 'var(--color-info-bg)',
    text: 'var(--color-info)',
    border: 'var(--color-info-border)',
  },
};
</script>

<template>
  <Transition name="toast-stack">
    <div v-if="toastStore.toasts.length > 0" class="toast-container">
      <Transition-group name="toast" tag="div" class="toast-list">
        <div v-for="toast in toastStore.toasts" :key="toast.id" class="toast" :style="{
          backgroundColor: toastColors[toast.variant].bg,
          borderColor: toastColors[toast.variant].border,
        }">
          <div class="toast__icon" :style="{ color: toastColors[toast.variant].text }">
            {{ toastColors[toast.variant].icon }}
          </div>

          <div class="toast__content">
            <div class="toast__title" :style="{ color: toastColors[toast.variant].text }">
              {{ toast.title }}
            </div>
            <div v-if="toast.message" class="toast__message">
              {{ toast.message }}
            </div>
          </div>

          <button
            v-if="toast.dismissible"
            class="toast__close"
            :aria-label="`Close: ${toast.title}`"
            @click="toastStore.remove(toast.id)"
          >
            <Close />
          </button>
        </div>
      </Transition-group>
    </div>
  </Transition>
</template>

<style scoped>
.toast-container {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: var(--z-toast);
  pointer-events: none;
}

.toast-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  pointer-events: auto;
}

.toast {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px 16px;
  box-shadow: var(--elevation-floating);
  max-width: 360px;
  min-width: 320px;
  animation: slideInRight 200ms var(--ease-decelerate) forwards;
}

.toast__icon {
  flex: 0 0 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 14px;
}

.toast__content {
  flex: 1;
  min-width: 0;
}

.toast__title {
  font-weight: 600;
  font-size: 14px;
  line-height: 1.4;
  margin: 0;
}

.toast__message {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin: 4px 0 0 0;
  line-height: 1.4;
}

.toast__close {
  flex: 0 0 24px;
  height: 24px;
  background: transparent;
  border: 0;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: background-color 200ms var(--ease-standard);
}

.toast__close:hover {
  background-color: rgba(0, 0, 0, 0.04);
}

/* Animations */
@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes slideOutRight {
  from {
    opacity: 1;
    transform: translateX(0);
  }
  to {
    opacity: 0;
    transform: translateX(100%);
  }
}

.toast-enter-active,
.toast-leave-active {
  transition: all 200ms var(--ease-standard);
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-stack-enter-active,
.toast-stack-leave-active {
  transition: opacity 200ms var(--ease-standard);
}

.toast-stack-enter-from,
.toast-stack-leave-to {
  opacity: 0;
}

@media (max-width: 640px) {
  .toast-container {
    inset: 16px 16px auto auto;
  }

  .toast {
    min-width: auto;
    width: calc(100vw - 32px);
  }
}

@media (prefers-reduced-motion: reduce) {
  .toast,
  .toast-enter-active,
  .toast-leave-active {
    animation: none !important;
    transition: none !important;
  }
}
</style>
