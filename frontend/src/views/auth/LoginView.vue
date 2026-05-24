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
    <section class="login-card">
      <h1>RealSync</h1>
      <p>Đăng nhập workspace vận hành dữ liệu bất động sản.</p>
      <el-form label-position="top" @submit.prevent="submit">
        <el-form-item label="Email">
          <el-input v-model="form.email" type="email" />
        </el-form-item>
        <el-form-item label="Mật khẩu">
          <el-input v-model="form.password" type="password" show-password />
        </el-form-item>
        <el-button :loading="authStore.loading" native-type="submit" type="primary">Đăng nhập</el-button>
      </el-form>
    </section>
  </main>
</template>

<style scoped>
.login-view {
  align-items: center;
  background: var(--color-canvas);
  display: flex;
  min-height: 100vh;
  justify-content: center;
  padding: 24px;
}

.login-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-floating);
  max-width: 420px;
  padding: 32px;
  width: 100%;
}

.login-card h1 {
  font-size: 28px;
  margin: 0 0 8px;
}

.login-card p {
  color: var(--color-text-secondary);
  margin: 0 0 24px;
}

.login-card .el-button {
  width: 100%;
}
</style>
