<script setup lang="ts">
import { ref } from 'vue';
import { DocumentAdd, MagicStick } from '@element-plus/icons-vue';

import ActionToolbar from '@/components/common/ActionToolbar.vue';
import ContentQueueTable from '@/components/content/ContentQueueTable.vue';
import ModuleShell from '@/components/common/ModuleShell.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockAiContents } from '@/utils/mockData';

const keyword = ref('');

const templates = [
  { name: 'Listing description', channel: 'listing', tone: 'Chuyên nghiệp', status: 'ready' },
  { name: 'SEO meta package', channel: 'seo', tone: 'Tối ưu tìm kiếm', status: 'ready' },
  { name: 'Facebook/Zalo post', channel: 'social', tone: 'Ngắn gọn, có CTA', status: 'review' }
];
</script>

<template>
  <ModuleShell title="Content AI" description="Tạo mô tả, SEO content và bài social từ dữ liệu BĐS đã chuẩn hóa.">
    <template #actions>
      <el-button :icon="MagicStick" type="primary">Generate content</el-button>
    </template>

    <div class="content-grid">
      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Content queue</h2>
        </div>
        <div class="panel__body">
          <ActionToolbar v-model:keyword="keyword" placeholder="Tìm nội dung..." >
            <el-button :icon="DocumentAdd">Template</el-button>
          </ActionToolbar>
        </div>
        <ContentQueueTable :items="mockAiContents" />
      </section>

      <section class="panel">
        <div class="panel__header">
          <h2 class="panel__title">Templates</h2>
        </div>
        <div class="panel__body template-list">
          <article v-for="template in templates" :key="template.name" class="template-list__item">
            <div>
              <strong>{{ template.name }}</strong>
              <span>{{ template.channel }} · {{ template.tone }}</span>
            </div>
            <StatusBadge :label="template.status" :variant="template.status === 'ready' ? 'success' : 'warning'" />
          </article>
        </div>
      </section>
    </div>
  </ModuleShell>
</template>

<style scoped>
.content-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: minmax(0, 1fr) 360px;
}

.template-list {
  display: grid;
  gap: 14px;
}

.template-list__item {
  align-items: flex-start;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  gap: 12px;
  justify-content: space-between;
  padding-bottom: 14px;
}

.template-list__item:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.template-list__item strong,
.template-list__item span {
  display: block;
}

.template-list__item span {
  color: var(--color-text-secondary);
  margin-top: 4px;
}

@media (max-width: 1080px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}
</style>
