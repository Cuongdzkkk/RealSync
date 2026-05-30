<script setup lang="ts">
import { ref } from 'vue';
import { Plus } from '@element-plus/icons-vue';

import ActionToolbar from '@/components/common/ActionToolbar.vue';
import ModuleShell from '@/components/common/ModuleShell.vue';
import ProjectOverviewGrid from '@/components/project/ProjectOverviewGrid.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockProjects } from '@/utils/mockData';
import { formatDate } from '@/utils/format';

const keyword = ref('');
</script>

<template>
  <ModuleShell title="Dự án" description="Quản lý dự án, khu vực, tồn kho sản phẩm và lead liên quan.">
    <template #actions>
      <el-button :icon="Plus" type="primary">Thêm dự án</el-button>
    </template>

    <section class="panel">
      <div class="panel__body">
        <ActionToolbar v-model:keyword="keyword" placeholder="Tìm dự án hoặc khu vực..." />
      </div>
    </section>

    <ProjectOverviewGrid :projects="mockProjects" />

    <section class="panel">
      <div class="panel__header">
        <h2 class="panel__title">Danh sách dự án</h2>
      </div>
      <el-table :data="mockProjects">
        <el-table-column prop="name" label="Dự án" min-width="220" />
        <el-table-column prop="area" label="Khu vực" width="160" />
        <el-table-column label="Trạng thái" width="140">
          <template #default="{ row }">
            <StatusBadge :label="row.status" :variant="row.status === 'active' ? 'success' : row.status === 'paused' ? 'warning' : 'info'" />
          </template>
        </el-table-column>
        <el-table-column prop="propertyCount" label="Sản phẩm" width="120" />
        <el-table-column prop="leadCount" label="Khách hàng" width="120" />
        <el-table-column label="Cập nhật" width="130">
          <template #default="{ row }">
            {{ formatDate(row.updatedAt) }}
          </template>
        </el-table-column>
      </el-table>
    </section>
  </ModuleShell>
</template>
