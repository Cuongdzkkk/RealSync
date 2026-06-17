<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { Filter, Refresh } from '@element-plus/icons-vue';
import { LEAD_PRIORITIES, LEAD_SOURCE_CHANNELS, LEAD_STATUSES } from '@/types/crm/lead';
import { getLeadPriorityLabel, getLeadStatusLabel } from '@/utils/crm';
import { mockCrmUsers } from '@/mocks/crm/users';
import type { LeadQuery } from '@/types/crm/lead';

const props = defineProps<{ query: LeadQuery }>();
const emit = defineEmits<{
  (e: 'change', query: Partial<LeadQuery>): void;
  (e: 'reset'): void;
}>();

const drawerOpen = ref(false);
const local = reactive({
  search: props.query.search,
  status: props.query.status,
  priority: props.query.priority,
  sourceChannel: props.query.sourceChannel,
  assignedToId: props.query.assignedToId,
  followUpState: props.query.followUpState ?? 'all',
  scoreRange: [props.query.minScore ?? 0, props.query.maxScore ?? 100] as [number, number]
});

const activeUsers = computed(() => mockCrmUsers.filter((user) => user.isActive));

let searchTimer: number | undefined;
watch(
  () => local.search,
  (value) => {
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(() => emit('change', { search: value, page: 1 }), 300);
  }
);

watch(
  () => [local.status, local.priority, local.sourceChannel, local.assignedToId, local.followUpState, local.scoreRange[0], local.scoreRange[1]],
  () => {
    emit('change', {
      status: local.status,
      priority: local.priority,
      sourceChannel: local.sourceChannel,
      assignedToId: local.assignedToId,
      followUpState: local.followUpState,
      minScore: local.scoreRange[0],
      maxScore: local.scoreRange[1],
      page: 1
    });
  }
);

watch(
  () => props.query,
  (query) => {
    local.search = query.search;
    local.status = query.status;
    local.priority = query.priority;
    local.sourceChannel = query.sourceChannel;
    local.assignedToId = query.assignedToId;
    local.followUpState = query.followUpState ?? 'all';
    local.scoreRange = [query.minScore ?? 0, query.maxScore ?? 100];
  },
  { deep: true }
);
</script>

<template>
  <section class="filters glass-card">
    <div class="filters__desktop">
      <el-input v-model="local.search" clearable placeholder="Tìm tên, phone, email, nhu cầu" />
      <el-select v-model="local.status" clearable placeholder="Trạng thái">
        <el-option v-for="status in LEAD_STATUSES" :key="status" :label="getLeadStatusLabel(status)" :value="status" />
      </el-select>
      <el-select v-model="local.priority" clearable placeholder="Ưu tiên">
        <el-option v-for="priority in LEAD_PRIORITIES" :key="priority" :label="getLeadPriorityLabel(priority)" :value="priority" />
      </el-select>
      <el-select v-model="local.sourceChannel" clearable placeholder="Nguồn">
        <el-option v-for="source in LEAD_SOURCE_CHANNELS" :key="source" :label="source" :value="source" />
      </el-select>
      <el-select v-model="local.assignedToId" clearable placeholder="Phụ trách">
        <el-option v-for="user in activeUsers" :key="user.id" :label="user.fullName" :value="user.id" />
      </el-select>
      <el-select v-model="local.followUpState" placeholder="Follow-up">
        <el-option label="Tất cả" value="all" />
        <el-option label="Quá hạn" value="overdue" />
        <el-option label="Hôm nay" value="today" />
        <el-option label="7 ngày tới" value="upcoming" />
        <el-option label="Không có lịch" value="none" />
      </el-select>
      <div class="score-filter">
        <span>Score</span>
        <el-slider v-model="local.scoreRange" range :min="0" :max="100" />
      </div>
      <el-button :icon="Refresh" @click="emit('reset')">Reset</el-button>
    </div>

    <div class="filters__mobile">
      <el-input v-model="local.search" clearable placeholder="Tìm Lead" />
      <el-button :icon="Filter" @click="drawerOpen = true">Bộ lọc</el-button>
    </div>

    <el-drawer v-model="drawerOpen" title="Bộ lọc Lead" size="100%">
      <div class="drawer-filters">
        <el-select v-model="local.status" clearable placeholder="Trạng thái">
          <el-option v-for="status in LEAD_STATUSES" :key="status" :label="getLeadStatusLabel(status)" :value="status" />
        </el-select>
        <el-select v-model="local.priority" clearable placeholder="Ưu tiên">
          <el-option v-for="priority in LEAD_PRIORITIES" :key="priority" :label="getLeadPriorityLabel(priority)" :value="priority" />
        </el-select>
        <el-select v-model="local.sourceChannel" clearable placeholder="Nguồn">
          <el-option v-for="source in LEAD_SOURCE_CHANNELS" :key="source" :label="source" :value="source" />
        </el-select>
        <el-select v-model="local.assignedToId" clearable placeholder="Phụ trách">
          <el-option v-for="user in activeUsers" :key="user.id" :label="user.fullName" :value="user.id" />
        </el-select>
        <el-select v-model="local.followUpState" placeholder="Follow-up">
          <el-option label="Tất cả" value="all" />
          <el-option label="Quá hạn" value="overdue" />
          <el-option label="Hôm nay" value="today" />
          <el-option label="7 ngày tới" value="upcoming" />
          <el-option label="Không có lịch" value="none" />
        </el-select>
        <div class="score-filter">
          <span>Score</span>
          <el-slider v-model="local.scoreRange" range :min="0" :max="100" />
        </div>
        <el-button :icon="Refresh" @click="emit('reset')">Reset filters</el-button>
      </div>
    </el-drawer>
  </section>
</template>

<style scoped>
.filters {
  padding: 12px;
}

.filters__desktop {
  align-items: center;
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(220px, 1.3fr) repeat(5, minmax(120px, 0.7fr)) minmax(150px, 0.8fr) auto;
}

.filters__mobile {
  display: none;
  gap: 10px;
}

.score-filter {
  align-items: center;
  display: grid;
  gap: 8px;
  grid-template-columns: auto 1fr;
}

.score-filter span {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
}

.drawer-filters {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

@media (max-width: 1180px) {
  .filters__desktop {
    display: none;
  }

  .filters__mobile {
    display: grid;
    grid-template-columns: 1fr auto;
  }
}
</style>
