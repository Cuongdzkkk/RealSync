import { ref } from 'vue';
import { defineStore } from 'pinia';

export interface Toast {
  id: string;
  title: string;
  message?: string;
  variant: 'success' | 'warning' | 'error' | 'info';
  duration: number;
  action?: { label: string; onClick: () => void };
}

let nextId = 0;

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([]);
  const queue = ref<Toast[]>([]);
  const maxVisible = 4;

  function add(toast: Omit<Toast, 'id'>) {
    const id = `toast-${++nextId}`;
    const item: Toast = {
      ...toast,
      id,
      duration: toast.duration ?? (toast.variant === 'error' ? 0 : 4000)
    };

    if (toasts.value.length >= maxVisible) {
      queue.value.push(item);
    } else {
      toasts.value.push(item);
      if (item.duration > 0) {
        setTimeout(() => remove(id), item.duration);
      }
    }
  }

  function remove(id: string) {
    toasts.value = toasts.value.filter((t) => t.id !== id);
    if (queue.value.length > 0) {
      const next = queue.value.shift()!;
      toasts.value.push(next);
      if (next.duration > 0) {
        setTimeout(() => remove(next.id), next.duration);
      }
    }
  }

  function success(title: string, message?: string) {
    add({ title, message, variant: 'success', duration: 4000 });
  }

  function warning(title: string, message?: string) {
    add({ title, message, variant: 'warning', duration: 4000 });
  }

  function error(title: string, message?: string) {
    add({ title, message, variant: 'error', duration: 0 });
  }

  function info(title: string, message?: string) {
    add({ title, message, variant: 'info', duration: 4000 });
  }

  return {
    toasts,
    add,
    remove,
    success,
    warning,
    error,
    info
  };
});
