<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'

const router = useRouter()
const authStore = useAuthStore()

const role = computed(() => authStore.user?.role ?? 'Guest')
</script>

<template>
  <div
    class="min-h-screen relative flex items-center justify-center p-4
           bg-slate-50 dark:bg-slate-950"
  >
    <div class="relative z-10 w-full" style="max-width: 480px">
      <div
        class="w-full rounded-[24px] border p-10 text-center
               bg-white/80 dark:bg-slate-900/80
               backdrop-blur-xl
               border-gray-200/60 dark:border-white/[0.06]
               shadow-[0_1px_3px_rgba(0,0,0,0.02),0_4px_20px_-2px_rgba(148,163,184,0.12)]
               dark:shadow-[0_1px_2px_rgba(0,0,0,0.2),0_8px_24px_-4px_rgba(0,0,0,0.4)]"
      >
        <div
          class="inline-flex items-center justify-center w-16 h-16 mb-6
                 bg-red-100 dark:bg-red-900/30 rounded-full"
        >
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="text-red-500">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
        </div>

        <h1 class="text-2xl font-bold tracking-tight mb-2 text-slate-900 dark:text-white">
          403 — Không có quyền truy cập
        </h1>
        <p class="text-sm text-slate-500 dark:text-slate-400 mb-6">
          Vai trò <strong class="text-slate-700 dark:text-slate-300">{{ role }}</strong> của bạn không được phép vào trang này.
        </p>

        <div class="flex flex-col gap-3">
          <button
            class="w-full py-3 px-4 rounded-xl
                   bg-[#FACC15] hover:bg-[#EAB308]
                   text-slate-900 font-semibold text-sm
                   transition-all duration-200"
            @click="router.push('/admin/dashboard')"
          >
            Về trang Tổng quan
          </button>
          <button
            class="w-full py-3 px-4 rounded-xl
                   bg-white dark:bg-slate-800/50
                   border border-gray-200 dark:border-white/10
                   text-slate-600 dark:text-slate-400 text-sm
                   transition-all duration-200
                   hover:bg-gray-50 dark:hover:bg-slate-800"
            @click="router.push('/login')"
          >
            Đăng nhập lại
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
