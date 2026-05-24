<script setup lang="ts">
import ModuleShell from '@/components/common/ModuleShell.vue';
import RoleGate from '@/components/common/RoleGate.vue';
</script>

<template>
  <ModuleShell title="Cài đặt" description="Thiết lập workspace, tích hợp crawler/AI và chính sách vận hành.">
    <div class="settings-grid">
      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Workspace</h2>
        </div>
        <div class="panel__body settings-form">
          <el-form label-position="top">
            <el-form-item label="Tên workspace">
              <el-input model-value="RealSync Operations" />
            </el-form-item>
            <el-form-item label="Môi trường">
              <el-select model-value="development">
                <el-option label="Development" value="development" />
                <el-option label="Staging" value="staging" />
                <el-option label="Production" value="production" />
              </el-select>
            </el-form-item>
          </el-form>
        </div>
      </section>

      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Tích hợp</h2>
        </div>
        <div class="panel__body settings-form">
          <el-form label-position="top">
            <el-form-item label="Webhook crawler">
              <el-input model-value="https://api.realsync.local/webhooks/crawler" />
            </el-form-item>
            <el-form-item label="AI classification">
              <el-switch model-value />
            </el-form-item>
            <el-form-item label="Content AI review bắt buộc">
              <el-switch model-value />
            </el-form-item>
          </el-form>
        </div>
      </section>

      <RoleGate :roles="['Admin']">
        <section class="panel">
          <div class="panel__header">
            <h2 class="panel__title">Bảo mật</h2>
          </div>
          <div class="panel__body settings-form">
            <el-form label-position="top">
              <el-form-item label="JWT access token">
                <el-input model-value="15 phút" />
              </el-form-item>
              <el-form-item label="Refresh token">
                <el-input model-value="7 ngày" />
              </el-form-item>
              <el-form-item label="Rate limit">
                <el-input model-value="100 requests/phút/user" />
              </el-form-item>
            </el-form>
          </div>
        </section>
      </RoleGate>
    </div>
  </ModuleShell>
</template>

<style scoped>
.settings-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.settings-form {
  max-width: 640px;
}

@media (max-width: 960px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }
}
</style>
