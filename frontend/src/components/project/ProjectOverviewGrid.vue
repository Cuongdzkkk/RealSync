<script setup lang="ts">
import type { Project } from '@/types/project';
import { formatDate } from '@/utils/format';
import StatusBadge from '@/components/common/StatusBadge.vue';

defineProps<{
  projects: Project[];
}>();

const statusVariant = (status: Project['status']) => {
  if (status === 'active') return 'success';
  if (status === 'paused') return 'warning';
  if (status === 'completed') return 'info';
  return 'muted';
};
</script>

<template>
  <div class="project-grid">
    <article v-for="project in projects" :key="project.id" class="project-card">
      <div class="project-card__top">
        <div>
          <strong>{{ project.name }}</strong>
          <span>{{ project.area }}</span>
        </div>
        <StatusBadge :label="project.status" :variant="statusVariant(project.status)" />
      </div>
      <dl>
        <div>
          <dt>Sản phẩm</dt>
          <dd class="numeric">{{ project.propertyCount }}</dd>
        </div>
        <div>
          <dt>Leads</dt>
          <dd class="numeric">{{ project.leadCount }}</dd>
        </div>
      </dl>
      <small>Cập nhật {{ formatDate(project.updatedAt) }}</small>
    </article>
  </div>
</template>

<style scoped>
.project-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
  padding: 18px;
}

.project-card__top {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.project-card strong,
.project-card span,
.project-card small {
  display: block;
}

.project-card span,
.project-card small,
.project-card dt {
  color: var(--color-text-secondary);
}

.project-card dl {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, 1fr);
  margin: 20px 0;
}

.project-card dt {
  font-size: 12px;
}

.project-card dd {
  font-size: 24px;
  font-weight: 800;
  margin: 4px 0 0;
}

@media (max-width: 900px) {
  .project-grid {
    grid-template-columns: 1fr;
  }
}
</style>
