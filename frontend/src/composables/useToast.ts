import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export type ToastVariant = 'success' | 'warning' | 'error' | 'info';

export interface Toast {
  id: string;
  title: string;
  message?: string;
  variant: ToastVariant;
  dismissible?: boolean;
  duration?: number; // ms, 0 = no auto-dismiss
}

const nextId = () => Math.random().toString(36).slice(2, 11);

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([]);

  const add = (
    title: string,
    options?: {
      message?: string;
      variant?: ToastVariant;
      dismissible?: boolean;
      duration?: number;
    }
  ) => {
    const id = nextId();
    const toast: Toast = {
      id,
      title,
      message: options?.message,
      variant: options?.variant ?? 'info',
      dismissible: options?.dismissible ?? true,
      duration: options?.duration ?? (options?.variant === 'error' ? 0 : 4000),
    };

    toasts.value.push(toast);

    // Auto-dismiss if duration > 0
    if (toast.duration > 0) {
      setTimeout(() => remove(id), toast.duration);
    }

    return id;
  };

  const remove = (id: string) => {
    const index = toasts.value.findIndex((t) => t.id === id);
    if (index !== -1) {
      toasts.value.splice(index, 1);
    }
  };

  const clear = () => {
    toasts.value = [];
  };

  // Convenience methods
  const success = (title: string, message?: string) =>
    add(title, { message, variant: 'success', duration: 4000 });

  const error = (title: string, message?: string) =>
    add(title, { message, variant: 'error', duration: 0 });

  const warning = (title: string, message?: string) =>
    add(title, { message, variant: 'warning', duration: 4000 });

  const info = (title: string, message?: string) =>
    add(title, { message, variant: 'info', duration: 4000 });

  // Only show max 4 toasts
  const visibleToasts = computed(() => toasts.value.slice(-4));

  return {
    toasts: visibleToasts,
    add,
    remove,
    clear,
    success,
    error,
    warning,
    info,
  };
});

// Composable wrapper for convenience
export const useToast = () => useToastStore();
