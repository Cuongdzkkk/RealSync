<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/useAuthStore'
import { mockUsers } from '@/utils/mockData'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const showPassword = ref(false)
const remember = ref(false)

const emailError = ref('')
const passwordError = ref('')

const form = reactive({
  email: 'admin@realsync.vn',
  password: 'Admin@123'
});

const validate = () => {
  let valid = true

  emailError.value = ''
  passwordError.value = ''

  if (!form.email) {
    emailError.value = 'Vui lòng nhập email'
    valid = false
  }
  if (!form.password) {
    passwordError.value = 'Vui lòng nhập mật khẩu'
    valid = false
  }

  return valid
}

const MOCK_USERS = [
  ...mockUsers,
  { id: 'u-admin', fullName: 'RealSync Admin', email: 'admin@realsync.vn', role: 'Admin' as const, status: 'active' as const, lastSeenAt: '' }
]

const handleSubmit = async () => {
  if (!validate()) return

  loading.value = true
  try {
    // Thử đăng nhập thật qua API backend
    await authStore.login({
      email: form.email,
      password: form.password
    });
    ElMessage.success('Đăng nhập thành công');
    router.push('/admin/dashboard');
  } catch (err: any) {
    console.warn('API login failed, falling back to mock login:', err);
    
    const matched = MOCK_USERS.find(u => u.email === form.email)
    if (!matched) {
      emailError.value = 'Email không tồn tại hoặc mật khẩu sai'
      loading.value = false
      return
    }

    const profile = {
      id: matched.id,
      fullName: matched.fullName,
      email: matched.email,
      role: matched.role
    }
    authStore.user = profile
    authStore.accessToken = 'mock-token'
    localStorage.setItem('realsync.accessToken', 'mock-token')
    localStorage.setItem('realsync.user', JSON.stringify(profile))
    ElMessage.success('Đăng nhập thành công (Giả lập)')
    router.push('/admin/dashboard')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div
    class="min-h-screen relative flex items-center justify-center p-4
           bg-slate-50 dark:bg-slate-950"
  >
    <div
      class="fixed inset-0 pointer-events-none"
      :class="$style.mesh"
    />

    <div class="relative z-10 w-full" style="max-width: 480px">
      <div
        class="w-full rounded-[24px] border p-8 sm:p-10
               transition-all duration-300
               bg-white/80 dark:bg-slate-900/80
               backdrop-blur-xl
               border-gray-200/60 dark:border-white/[0.06]
               shadow-[0_1px_3px_rgba(0,0,0,0.02),0_4px_20px_-2px_rgba(148,163,184,0.12)]
               dark:shadow-[0_1px_2px_rgba(0,0,0,0.2),0_8px_24px_-4px_rgba(0,0,0,0.4)]"
      >
        <div class="text-center mb-8">
          <div
            class="inline-flex items-center justify-center w-14 h-14 mb-5
                   bg-[#FACC15] rounded-[16px]
                   shadow-[0_0_16px_rgba(250,204,21,0.3)]"
          >
            <span class="text-xl font-extrabold tracking-tight text-slate-900">RS</span>
          </div>
          <h1 class="text-2xl font-bold tracking-tight mb-1.5 text-slate-900 dark:text-white">
            Đăng nhập
          </h1>
          <p class="text-sm text-slate-500 dark:text-slate-400">
            Nhập thông tin để truy cập hệ thống
          </p>
        </div>

        <form class="space-y-5" @submit.prevent="handleSubmit">
          <div class="space-y-1.5">
            <label for="email" class="block text-sm font-medium text-slate-700 dark:text-slate-300">
              Email
            </label>
            <div class="relative">
              <span class="absolute top-1/2 -translate-y-1/2 left-3.5 flex items-center justify-center text-slate-400 dark:text-slate-500">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="2" y="4" width="20" height="16" rx="2" />
                  <path d="M22 4L12 13L2 4" />
                </svg>
              </span>
              <input
                id="email"
                v-model="form.email"
                type="email"
                placeholder="admin@realsync.vn"
                autocomplete="email"
                @input="emailError = ''"
                class="w-full px-4 py-3 pl-10 text-sm
                       bg-white dark:bg-slate-800/50
                       border rounded-xl
                       text-slate-900 dark:text-white
                       placeholder:text-slate-400 dark:placeholder:text-slate-500
                       outline-none
                       transition-all duration-200
                       focus:ring-2
                       focus:border-[#FACC15] focus:ring-[#FACC15]/30"
                :class="emailError
                  ? 'border-red-400 dark:border-red-500/50 focus:border-red-400 focus:ring-red-400/30'
                  : 'border-gray-200 dark:border-white/10'"
              />
              <p v-if="emailError" class="mt-1 text-xs text-red-500">{{ emailError }}</p>
            </div>
          </div>

          <div class="space-y-1.5">
            <label for="password" class="block text-sm font-medium text-slate-700 dark:text-slate-300">
              Mật khẩu
            </label>
            <div class="relative">
              <span class="absolute top-1/2 -translate-y-1/2 left-3.5 flex items-center justify-center text-slate-400 dark:text-slate-500">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                </svg>
              </span>
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="••••••••"
                autocomplete="current-password"
                @input="passwordError = ''"
                class="w-full px-4 py-3 pl-10 pr-11 text-sm
                       bg-white dark:bg-slate-800/50
                       border rounded-xl
                       text-slate-900 dark:text-white
                       placeholder:text-slate-400 dark:placeholder:text-slate-500
                       outline-none
                       transition-all duration-200
                       focus:ring-2
                       focus:border-[#FACC15] focus:ring-[#FACC15]/30"
                :class="passwordError
                  ? 'border-red-400 dark:border-red-500/50 focus:border-red-400 focus:ring-red-400/30'
                  : 'border-gray-200 dark:border-white/10'"
              />
              <p v-if="passwordError" class="mt-1 text-xs text-red-500">{{ passwordError }}</p>
              <button
                type="button"
                class="absolute top-1/2 -translate-y-1/2 right-3.5 flex items-center justify-center
                       text-slate-400 dark:text-slate-500 cursor-pointer"
                @click="showPassword = !showPassword"
                :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
              >
                <svg v-if="!showPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94" />
                  <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19" />
                  <line x1="1" y1="1" x2="23" y2="23" />
                </svg>
              </button>
            </div>
          </div>

          <div class="flex items-center justify-between pt-1">
            <label class="inline-flex items-center gap-2 cursor-pointer select-none group">
              <span
                class="relative w-4 h-4 rounded border flex-shrink-0
                       border-gray-300 dark:border-white/20
                       bg-white dark:bg-slate-800/50
                       transition-all duration-150
                       group-focus-within:ring-2 group-focus-within:ring-[#FACC15]/40
                       group-focus-within:ring-offset-1
                       group-focus-within:ring-offset-white dark:group-focus-within:ring-offset-slate-900"
                :class="remember ? 'bg-[#FACC15] border-[#FACC15]' : ''"
              >
                <svg
                  v-if="remember"
                  class="absolute inset-0 m-auto"
                  width="12"
                  height="12"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="#0f172a"
                  stroke-width="3"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              </span>
              <input
                type="checkbox"
                v-model="remember"
                class="sr-only"
              />
              <span class="text-sm text-slate-600 dark:text-slate-400">
                Ghi nhớ đăng nhập
              </span>
            </label>
            <a
              href="#"
              class="text-sm font-medium text-[#FACC15] hover:text-[#EAB308] transition-colors duration-150"
              @click.prevent
            >Quên mật khẩu?</a>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full py-3 px-4 rounded-xl
                   bg-[#FACC15] hover:bg-[#EAB308]
                   text-slate-900 font-semibold text-sm
                   transition-all duration-200
                   flex items-center justify-center gap-2
                   disabled:opacity-60 disabled:cursor-not-allowed
                   active:scale-[0.98]
                   hover:shadow-[0_0_20px_rgba(250,204,21,0.35)]"
          >
            <svg
              v-if="loading"
              class="animate-spin"
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2.5"
              stroke-linecap="round"
            >
              <path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83" />
            </svg>
            <span v-else>Đăng nhập</span>
          </button>
        </form>

        <p class="text-center mt-8 text-[11px] text-slate-400 dark:text-slate-500">
          &copy; 2026 RealSync. Hệ thống vận hành dữ liệu BĐS.
        </p>
      </div>
    </div>
  </div>
</template>

<style module>
.mesh {
  background:
    radial-gradient(ellipse 80% 50% at 50% -20%, rgba(250, 204, 21, 0.08), transparent),
    radial-gradient(ellipse 50% 30% at 80% 80%, rgba(14, 165, 233, 0.04), transparent),
    radial-gradient(ellipse 50% 30% at 20% 80%, rgba(250, 204, 21, 0.04), transparent);
}

:global(.dark) .mesh {
  background:
    radial-gradient(ellipse 80% 50% at 50% -20%, rgba(250, 204, 21, 0.06), transparent),
    radial-gradient(ellipse 50% 30% at 80% 80%, rgba(14, 165, 233, 0.06), transparent);
}
</style>
