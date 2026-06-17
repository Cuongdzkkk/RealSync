<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import LeadActivityDialog from '@/components/crm/leads/LeadActivityDialog.vue';
import LeadActivityTimeline from '@/components/crm/leads/LeadActivityTimeline.vue';
import LeadAssignDialog from '@/components/crm/leads/LeadAssignDialog.vue';
import LeadConvertDialog from '@/components/crm/leads/LeadConvertDialog.vue';
import LeadFollowUpDialog from '@/components/crm/leads/LeadFollowUpDialog.vue';
import LeadFormDrawer from '@/components/crm/leads/LeadFormDrawer.vue';
import LeadOverviewPanel from '@/components/crm/leads/LeadOverviewPanel.vue';
import LeadProfileHeader from '@/components/crm/leads/LeadProfileHeader.vue';
import LeadWorkflowPanel from '@/components/crm/leads/LeadWorkflowPanel.vue';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import { useLeadStore } from '@/stores/useLeadStore';
import { useToastStore } from '@/stores/useToastStore';
import type {
  LeadActivityCreateModel,
  LeadAssignModel,
  LeadConvertModel,
  LeadCreateModel,
  LeadFollowUpModel,
  LeadStatus
} from '@/types/crm/lead';

const route = useRoute();
const router = useRouter();
const leadStore = useLeadStore();
const toastStore = useToastStore();

const editOpen = ref(false);
const assignOpen = ref(false);
const activityOpen = ref(false);
const followUpOpen = ref(false);
const convertOpen = ref(false);

const leadId = computed(() => String(route.params.id));
const lead = computed(() => leadStore.selectedLead);

onMounted(() => {
  leadStore.fetchLeadById(leadId.value);
});

async function submitEdit(payload: LeadCreateModel) {
  if (!lead.value) return;
  try {
    await leadStore.updateLead(lead.value.id, payload);
    toastStore.success('Đã cập nhật Lead');
    editOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể cập nhật', error instanceof Error ? error.message : undefined);
  }
}

async function updateStatus(status: LeadStatus) {
  if (!lead.value || status === lead.value.status) return;
  try {
    await leadStore.updateStatus(lead.value.id, { status, note: 'Cập nhật trạng thái từ trang chi tiết.' });
    toastStore.success('Đã cập nhật trạng thái');
  } catch (error) {
    toastStore.error('Không thể cập nhật trạng thái', error instanceof Error ? error.message : undefined);
  }
}

async function submitAssign(payload: LeadAssignModel) {
  if (!lead.value) return;
  try {
    await leadStore.assignLead(lead.value.id, payload);
    toastStore.success('Đã phân công Lead');
    assignOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể phân công', error instanceof Error ? error.message : undefined);
  }
}

async function submitActivity(payload: LeadActivityCreateModel) {
  if (!lead.value) return;
  if (payload.activityType === 'Note' && !payload.description?.trim()) {
    toastStore.warning('Thiếu mô tả', 'Ghi chú cần có nội dung.');
    return;
  }
  try {
    await leadStore.addActivity(lead.value.id, payload);
    toastStore.success('Đã thêm hoạt động');
    activityOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể thêm hoạt động', error instanceof Error ? error.message : undefined);
  }
}

async function submitFollowUp(payload: LeadFollowUpModel) {
  if (!lead.value) return;
  try {
    await leadStore.setFollowUp(lead.value.id, payload);
    toastStore.success('Đã đặt lịch follow-up');
    followUpOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể đặt lịch', error instanceof Error ? error.message : undefined);
  }
}

async function submitConvert(payload: LeadConvertModel) {
  if (!lead.value) return;
  try {
    await leadStore.convertToCustomer(lead.value.id, payload);
    toastStore.success('Đã chuyển Lead thành khách hàng.');
    convertOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể chuyển đổi', error instanceof Error ? error.message : undefined);
  }
}

async function deleteLead() {
  if (!lead.value) return;
  try {
    await ElMessageBox.confirm(`Xóa Lead ${lead.value.fullName}?`, 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
    });
    await leadStore.deleteLead(lead.value.id);
    toastStore.success('Đã xóa Lead');
    router.push({ name: 'lead-list' });
  } catch {
    // User canceled.
  }
}
</script>

<template>
  <div class="page lead-detail-page">
    <el-breadcrumb separator="/">
      <el-breadcrumb-item :to="{ name: 'lead-list' }">Lead tiềm năng</el-breadcrumb-item>
      <el-breadcrumb-item>Chi tiết Lead</el-breadcrumb-item>
    </el-breadcrumb>

    <el-skeleton v-if="leadStore.detailLoading" :rows="8" animated class="glass-card detail-loading" />

    <CrmEmptyState
      v-else-if="!lead"
      class="glass-card"
      title="Không tìm thấy Lead"
      description="Lead có thể đã bị xóa khỏi mock database hoặc đường dẫn không hợp lệ."
    >
      <template #action>
        <el-button type="primary" @click="router.push({ name: 'lead-list' })">Quay lại danh sách</el-button>
      </template>
    </CrmEmptyState>

    <template v-else>
      <LeadProfileHeader :lead="lead" @back="router.push({ name: 'lead-list' })" @edit="editOpen = true" />

      <div class="detail-grid">
        <div class="detail-main">
          <LeadOverviewPanel :lead="lead" />
          <LeadActivityTimeline :activities="lead.activities" />
        </div>
        <aside class="detail-side">
          <LeadWorkflowPanel
            :lead="lead"
            @status="updateStatus"
            @assign="assignOpen = true"
            @activity="activityOpen = true"
            @follow-up="followUpOpen = true"
            @convert="convertOpen = true"
            @edit="editOpen = true"
            @delete="deleteLead"
          />
        </aside>
      </div>

      <LeadFormDrawer v-model="editOpen" :lead="lead" :submitting="leadStore.submitting" @submit="submitEdit" />
      <LeadAssignDialog v-model="assignOpen" :lead="lead" :submitting="leadStore.submitting" @submit="submitAssign" />
      <LeadActivityDialog v-model="activityOpen" :submitting="leadStore.submitting" @submit="submitActivity" />
      <LeadFollowUpDialog v-model="followUpOpen" :lead="lead" :submitting="leadStore.submitting" @submit="submitFollowUp" />
      <LeadConvertDialog v-model="convertOpen" :lead="lead" :submitting="leadStore.submitting" @submit="submitConvert" />
    </template>
  </div>
</template>

<style scoped>
.lead-detail-page {
  gap: 16px;
}

.detail-loading {
  padding: 24px;
}

.detail-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: minmax(0, 2fr) 360px;
}

.detail-main {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
}

.detail-side {
  position: sticky;
  top: 84px;
}

@media (max-width: 1100px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }

  .detail-side {
    position: static;
  }
}
</style>
