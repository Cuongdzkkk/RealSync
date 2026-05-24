<script setup lang="ts">
import { Check, Close } from '@element-plus/icons-vue';

import type { RoleCapability } from '@/types/user';

defineProps<{
  capabilities: RoleCapability[];
}>();

const roles = [
  { key: 'admin', label: 'Admin' },
  { key: 'manager', label: 'Manager' },
  { key: 'agent', label: 'Agent' },
  { key: 'viewer', label: 'Viewer' }
] as const;
</script>

<template>
  <el-table :data="capabilities">
    <el-table-column prop="module" label="Module" min-width="220" />
    <el-table-column v-for="role in roles" :key="role.key" :label="role.label" width="110" align="center">
      <template #default="{ row }">
        <el-icon :class="row[role.key] ? 'permission permission--yes' : 'permission permission--no'">
          <Check v-if="row[role.key]" />
          <Close v-else />
        </el-icon>
      </template>
    </el-table-column>
  </el-table>
</template>

<style scoped>
.permission {
  font-size: 16px;
}

.permission--yes {
  color: var(--color-success);
}

.permission--no {
  color: var(--color-text-muted);
}
</style>
