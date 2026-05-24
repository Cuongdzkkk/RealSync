<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';

import { useAuthStore } from '@/stores/useAuthStore';

const router = useRouter();
const authStore = useAuthStore();
const form = reactive({
  email: 'admin@realsync.vn',
  password: ''
});

const submit = async () => {
  if (import.meta.env.DEV) {
    ElMessage.success('Đăng nhập chế độ local');
    await router.push('/admin/dashboard');
    return;
  }

  await authStore.login(form);
  await router.push('/admin/dashboard');
};
</script>

<template>
  <main class="login-view">
    <div class="login-bg" />
    <section class="login-card">
      <div class="login-brand">
        <span class="login-logo">RS</span>
      </div>
      <h1 class="login-title">RealSync</h1>
      <p class="login-desc">Đăng nhập workspace vận hành dữ liệu bất động sản</p>

      <el-form class="login-form" label-position="top" @submit.prevent="submit">
        <el-form-item label="Email">
          <el-input v-model="form.email" type="email" placeholder="admin@realsync.vn" />
        </el-form-item>
        <el-form-item label="Mật khẩu">
          <el-input v-model="form.password" type="password" show-password placeholder="••••••••" />
        </el-form-item>
        <el-button
          :loading="authStore.loading"
          native-type="submit"
          class="login-btn"
        >Đăng nhập</el-button>
      </el-form>

      <p class="login-footer">© 2026 RealSync. Hệ thống vận hành dữ liệu BĐS.</p>
    </section>
  </main>
</template>

<style scoped>
.login-view {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: var(--color-canvas);
  padding: 24px;
  position: relative;
}

.login-bg {
  position: fixed;
  inset: 0;
  background:
    radial-gradient(ellipse 80% 50% at 50% -20%, rgba(245, 230, 66, 0.08), transparent),
    radial-gradient(ellipse 50% 30% at 80% 80%, rgba(245, 230, 66, 0.05), transparent);
  pointer-events: none;
}

.login-card {
  position: relative;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 16px;
  max-width: 400px;
  width: 100%;
  padding: 40px 36px;
  text-align: center;
}

.login-brand {
  margin-bottom: 20px;
}

.login-logo {
  display: inline-flex;
  width: 48px;
  height: 48px;
  background: #F5E642;
  border-radius: 12px;
  color: #0D0D0D;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.login-title {
  font-size: 24px;
  font-weight: 700;
  letter-spacing: -0.01em;
  color: var(--color-text-primary);
  margin: 0 0 6px 0;
}

.login-desc {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin: 0 0 28px 0;
  line-height: 1.5;
}

.login-form {
  text-align: left;
}

.login-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 500;
  color: var(--color-text-primary);
  padding-bottom: 6px;
}

.login-form :deep(.el-input__wrapper) {
  border-radius: 8px;
}

.login-btn {
  width: 100%;
  height: 42px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  background: #0D0D0D;
  border: none;
  color: #FFFFFF;
  margin-top: 8px;
}

.login-btn:hover {
  background: #333333;
}

.login-footer {
  margin: 32px 0 0 0;
  font-size: 11px;
  color: var(--color-text-muted);
}
</style>
