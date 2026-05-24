<script setup lang="ts">
import { Plus } from '@element-plus/icons-vue';

import PageHeader from '@/components/common/PageHeader.vue';
import LeadKanban from '@/components/lead/LeadKanban.vue';
import { mockLeads } from '@/utils/mockData';
</script>

<template>
  <section class="page">
    <PageHeader title="Lead pipeline" description="Theo dõi nhu cầu, mức độ nóng và trạng thái chăm sóc khách hàng.">
      <template #actions>
        <el-button :icon="Plus" type="primary">Thêm lead</el-button>
      </template>
    </PageHeader>
    <LeadKanban :leads="mockLeads" />
  </section>
</template>
