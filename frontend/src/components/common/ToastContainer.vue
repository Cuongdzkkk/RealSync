<script setup lang="ts">
import { Check, Close, InfoFilled, WarningFilled } from '@element-plus/icons-vue';
import { useToastStore } from '@/stores/useToastStore';

const toastStore = useToastStore();

const variantConfig: Record<string, { icon: unknown }> = {
  success: { icon: Check },
  warning: { icon: WarningFilled },
  error: { icon: Close },
  info: { icon: InfoFilled }
};
</script>

<template>
  <Teleport to="body">
    <div class="toast-container" aria-live="polite">
      <TransitionGroup name="toast">
        <article
          v-for="toast in toastStore.toasts"
          :key="toast.id"
          class="toast"
          :class="`toast--${toast.variant}`"
        >
          <el-icon :size="16" class="toast-icon">
            <component :is="variantConfig[toast.variant]?.icon" />
          </el-icon>
          <div class="toast-body">
            <strong class="toast-title">{{ toast.title }}</strong>
            <p v-if="toast.message" class="toast-message">{{ toast.message }}</p>
            <button
              v-if="toast.action"
              class="toast-action"
              @click="toast.action!.onClick()"
            >
              {{ toast.action.label }}
            </button>
          </div>
          <button class="toast-close" aria-label="Đóng" @click="toastStore.remove(toast.id)">
            <el-icon :size="14"><Close /></el-icon>
          </button>
        </article>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-container {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: var(--z-toast);
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 320px;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px 16px;
  border-radius: 8px;
  border: 1px solid;
  background: var(--color-surface-2);
  box-shadow: var(--elevation-raised);
  pointer-events: auto;
  width: 100%;
}

.toast--success {
  border-color: var(--color-success-border);
  background: var(--color-success-bg);
}

.toast--warning {
  border-color: var(--color-warning-border);
  background: var(--color-warning-bg);
}

.toast--error {
  border-color: var(--color-danger-border);
  background: var(--color-danger-bg);
}

.toast--info {
  border-color: var(--color-info-border);
  background: var(--color-info-bg);
}

.toast-icon {
  flex-shrink: 0;
  margin-top: 2px;
}

.toast--success .toast-icon { color: var(--color-success); }
.toast--warning .toast-icon { color: var(--color-warning); }
.toast--error .toast-icon { color: var(--color-danger); }
.toast--info .toast-icon { color: var(--color-info); }

.toast-body {
  flex: 1;
  min-width: 0;
}

.toast-title {
  display: block;
  font-size: 14px;
  font-weight: 600;
  line-height: 1.3;
  color: var(--color-text-primary);
}

.toast-message {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.toast-action {
  background: none;
  border: none;
  padding: 0;
  margin-top: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-mint);
  cursor: pointer;
  font-family: inherit;
}

.toast-close {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 2px;
  border-radius: 4px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color var(--duration-fast) var(--ease-standard);
}

.toast-close:hover {
  color: var(--color-text-primary);
}

/* Transitions */
.toast-enter-active {
  animation: slideInRight 200ms var(--ease-decelerate);
}

.toast-leave-active {
  animation: fadeOut 150ms var(--ease-standard);
}

.toast-move {
  transition: all 200ms var(--ease-standard);
}

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

@keyframes fadeOut {
  to {
    opacity: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .toast-enter-active,
  .toast-leave-active,
  .toast-move {
    animation: none;
    transition: none;
  }
}
</style>
