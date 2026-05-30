<script setup lang="ts">
import { ref } from 'vue';
import { MagicStick, Plus } from '@element-plus/icons-vue';

import ActionToolbar from '@/components/common/ActionToolbar.vue';
import AiJobTable from '@/components/ai/AiJobTable.vue';
import MetricCard from '@/components/common/MetricCard.vue';
import ModuleShell from '@/components/common/ModuleShell.vue';
import { mockAiJobs } from '@/utils/mockData';

const keyword = ref('');
</script>

<template>
  <ModuleShell title="Phân loại AI" description="Phân loại BĐS, lead scoring và kiểm duyệt kết quả AI confidence thấp.">
    <template #actions>
      <el-button :icon="MagicStick" type="primary">Chạy phân loại</el-button>
    </template>

    <div class="metric-grid">
      <MetricCard label="Jobs hôm nay" :value="128" trend="+16%" variant="ai" />
      <MetricCard label="Confidence TB" :value="86" suffix="%" trend="+3%" variant="success" />
      <MetricCard label="Cần review" :value="7" trend="human" variant="warning" />
      <MetricCard label="Thất bại" :value="2" trend="-4%" variant="danger" />
    </div>

    <section class="panel">
      <div class="panel__body">
        <ActionToolbar v-model:keyword="keyword" placeholder="Tìm sản phẩm, khách hàng hoặc job..." >
          <el-button :icon="Plus">Tạo rule</el-button>
        </ActionToolbar>
      </div>
      <AiJobTable :jobs="mockAiJobs" />
    </section>
  </ModuleShell>
</template>
