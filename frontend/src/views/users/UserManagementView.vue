<script setup lang="ts">
import { ref } from 'vue';
import { Plus } from '@element-plus/icons-vue';

import ActionToolbar from '@/components/common/ActionToolbar.vue';
import ModuleShell from '@/components/common/ModuleShell.vue';
import PermissionMatrix from '@/components/settings/PermissionMatrix.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockRoleCapabilities, mockUsers } from '@/utils/mockData';
import { formatDate } from '@/utils/format';

const keyword = ref('');
</script>

<template>
  <ModuleShell title="Users & Roles" description="Quản lý tài khoản nội bộ, vai trò và quyền truy cập theo module.">
    <template #actions>
      <el-button :icon="Plus" type="primary">Mời user</el-button>
    </template>

    <section class="panel">
      <div class="panel__body">
        <ActionToolbar v-model:keyword="keyword" placeholder="Tìm user hoặc email..." />
      </div>
      <el-table :data="mockUsers">
        <el-table-column prop="fullName" label="Người dùng" min-width="220" />
        <el-table-column prop="email" label="Email" min-width="240" />
        <el-table-column prop="role" label="Role" width="120" />
        <el-table-column label="Trạng thái" width="130">
          <template #default="{ row }">
            <StatusBadge :label="row.status" :variant="row.status === 'active' ? 'success' : row.status === 'invited' ? 'warning' : 'muted'" />
          </template>
        </el-table-column>
        <el-table-column label="Lần cuối" width="130">
          <template #default="{ row }">
            {{ formatDate(row.lastSeenAt) }}
          </template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel">
      <div class="panel__header">
        <h2 class="panel__title">Ma trận phân quyền</h2>
      </div>
      <PermissionMatrix :capabilities="mockRoleCapabilities" />
    </section>
  </ModuleShell>
</template>
