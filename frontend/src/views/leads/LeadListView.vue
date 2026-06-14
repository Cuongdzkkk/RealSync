<script setup lang="ts">
import { computed, ref } from 'vue';
import { useLeadStore } from '@/stores/useLeadStore';
import { useToastStore } from '@/stores/useToastStore';
import type { Lead, LeadStage, LeadTemperature } from '@/types/lead';
import { LEAD_STAGES } from '@/utils/constants';
import { formatCurrency, formatDate } from '@/utils/format';
import StatusBadge from '@/components/common/StatusBadge.vue';
import RoleGate from '@/components/common/RoleGate.vue';

const leadStore = useLeadStore();
const toastStore = useToastStore();

// --- Filters & Controls State ---
const searchQuery = ref('');
const selectedTemp = ref<string>('all');
const selectedStage = ref<string>('all');
const viewMode = ref<'table' | 'kanban'>('table');

// --- Drawer & Modal State ---
const activeInsightLead = ref<Lead | null>(null);
const showFormModal = ref(false);
const editingLead = ref<Lead | null>(null);

// --- Form State ---
const formTitle = computed(() => editingLead.value ? 'Cập nhật Khách hàng' : 'Thêm Khách hàng mới');
const formLead = ref({
  fullName: '',
  phone: '',
  demand: '',
  budget: 0,
  stage: 'new' as LeadStage,
  temperature: 'warm' as LeadTemperature,
  assignedTo: ''
});

// --- AI Suggestion Simulator ---
const isGeneratingScript = ref(false);
const generatedScript = ref('');

// Computed filtered list
const filteredLeads = computed(() => {
  return leadStore.items.filter(lead => {
    const matchesSearch = lead.fullName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          lead.phone.includes(searchQuery.value) ||
                          lead.demand.toLowerCase().includes(searchQuery.value.toLowerCase());
    const matchesTemp = selectedTemp.value === 'all' || lead.temperature === selectedTemp.value;
    const matchesStage = selectedStage.value === 'all' || lead.stage === selectedStage.value;
    return matchesSearch && matchesTemp && matchesStage;
  });
});

// Helper for AI Score Mapping (since it's not in the lead interface but we want to show it!)
function getAiScore(temp: LeadTemperature, id: string): number {
  // Semi-deterministic scoring based on id hash
  const hash = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const base = temp === 'hot' ? 90 : temp === 'warm' ? 70 : 40;
  return base + (hash % 10);
}

// Stage labels mapping
const stageLabels: Record<LeadStage, string> = {
  new: 'Mới nhận',
  contacted: 'Đã liên hệ',
  qualified: 'Đủ điều kiện',
  viewing: 'Đi xem nhà',
  closed: 'Đã chốt'
};

// Open Modals / Drawers
function openAiInsight(lead: Lead) {
  activeInsightLead.value = lead;
  generatedScript.value = '';
}

function closeAiInsight() {
  activeInsightLead.value = null;
}

function openAddModal() {
  editingLead.value = null;
  formLead.value = {
    fullName: '',
    phone: '',
    demand: '',
    budget: 0,
    stage: 'new',
    temperature: 'warm',
    assignedTo: 'Lan'
  };
  showFormModal.value = true;
}

function openEditModal(lead: Lead) {
  editingLead.value = lead;
  formLead.value = {
    fullName: lead.fullName,
    phone: lead.phone,
    demand: lead.demand,
    budget: lead.budget,
    stage: lead.stage,
    temperature: lead.temperature,
    assignedTo: lead.assignedTo
  };
  showFormModal.value = true;
}

// CRUD actions
function saveLead() {
  if (!formLead.value.fullName || !formLead.value.phone) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng nhập tên và số điện thoại của khách hàng.');
    return;
  }

  if (editingLead.value) {
    // Update
    const updated: Lead = {
      ...editingLead.value,
      ...formLead.value
    };
    leadStore.updateLead(updated);
    toastStore.success('Thành công', `Đã cập nhật thông tin khách hàng ${formLead.value.fullName}.`);
  } else {
    // Create
    const created: Lead = {
      id: `l-${Date.now()}`,
      ...formLead.value,
      lastContactAt: new Date().toISOString()
    };
    leadStore.addLead(created);
    toastStore.success('Thành công', `Đã thêm khách hàng mới ${formLead.value.fullName}.`);
  }
  showFormModal.value = false;
}

function deleteLeadItem(id: string, name: string) {
  if (confirm(`Bạn chắc chắn muốn xóa khách hàng ${name}?`)) {
    leadStore.deleteLead(id);
    toastStore.success('Đã xóa', `Đã xóa khách hàng ${name} khỏi hệ thống.`);
  }
}

// Simulate changing stages in Kanban board directly
function updateLeadStage(lead: Lead, newStage: LeadStage) {
  const updated = { ...lead, stage: newStage, lastContactAt: new Date().toISOString() };
  leadStore.updateLead(updated);
  toastStore.success('Cập nhật trạng thái', `Di chuyển ${lead.fullName} sang trạng thái "${stageLabels[newStage]}".`);
}

// Generate Personalized AI Consulting Script
function generateAiScript(lead: Lead) {
  if (isGeneratingScript.value) return;
  isGeneratingScript.value = true;
  generatedScript.value = '';

  setTimeout(() => {
    generatedScript.value = `Chào anh/chị **${lead.fullName}**,\nEm là Lan từ **RealSync**. Em thấy anh/chị đang tìm hiểu phân khúc **${lead.demand}** với ngân sách tầm **${formatCurrency(lead.budget)}**.\n\nHệ thống AI của bên em vừa lọc ra **3 căn hộ đắt giá nhất** đáp ứng trọn vẹn yêu cầu này, đặc biệt có căn view sông tầng cao giá cực tốt. Em gửi anh/chị thông tin chi tiết qua Zalo để mình tiện tham khảo nhé ạ!`;
    isGeneratingScript.value = false;
    toastStore.success('AI hoàn tất', 'Đã sinh kịch bản chăm sóc cá nhân hóa.');
  }, 1500);
}
</script>

<template>
  <RoleGate :roles="['Admin', 'Manager', 'Sales']">
    <div class="page">
      <!-- Header Section -->
      <div class="leads-header glass-card">
      <div class="header-main">
        <div>
          <h2>Quản lý Khách hàng</h2>
          <p class="subtitle-text">CRM CRM SaaS: Chấm điểm leads bằng AI, theo dõi phễu bán hàng và phân công sales.</p>
        </div>
        <button class="add-lead-btn glow-yellow" @click="openAddModal">
          + Thêm khách hàng
        </button>
      </div>

      <!-- Filters & Controls Row -->
      <div class="controls-row">
        <!-- Search -->
        <div class="search-box">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm theo tên, SĐT, nhu cầu..." 
          />
        </div>

        <!-- Filters -->
        <div class="filters-group">
          <div class="select-wrapper">
            <select v-model="selectedTemp">
              <option value="all">Tất cả nhiệt độ</option>
              <option value="hot">Hot 🔥</option>
              <option value="warm">Warm ☀️</option>
              <option value="cold">Cold ❄️</option>
            </select>
          </div>

          <div class="select-wrapper">
            <select v-model="selectedStage">
              <option value="all">Tất cả trạng thái</option>
              <option v-for="stage in LEAD_STAGES" :key="stage" :value="stage">
                {{ stageLabels[stage] }}
              </option>
            </select>
          </div>
        </div>

        <!-- View Switcher -->
        <div class="view-switcher">
          <button 
            class="switch-btn" 
            :class="{ 'is-active': viewMode === 'table' }"
            @click="viewMode = 'table'"
            title="Dạng bảng"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="7" height="9" /><rect x="14" y="3" width="7" height="5" /><rect x="14" y="12" width="7" height="9" /><rect x="3" y="16" width="7" height="5" />
            </svg>
            <span>Bảng</span>
          </button>
          <button 
            class="switch-btn" 
            :class="{ 'is-active': viewMode === 'kanban' }"
            @click="viewMode = 'kanban'"
            title="Dạng Kanban"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="18" height="18" rx="2" /><line x1="9" y1="3" x2="9" y2="21" /><line x1="15" y1="3" x2="15" y2="21" />
            </svg>
            <span>Phễu (Kanban)</span>
          </button>
        </div>
      </div>
    </div>

    <!-- MAIN VIEW AREA -->
    <!-- Mode 1: Table View -->
    <div v-if="viewMode === 'table'" class="table-container glass-card">
      <table class="leads-table">
        <thead>
          <tr>
            <th>Khách hàng</th>
            <th>Nhu cầu chi tiết</th>
            <th class="numeric-header">Ngân sách</th>
            <th>Trạng thái</th>
            <th>Độ ưu tiên</th>
            <th class="numeric-header">AI Score</th>
            <th>Liên hệ cuối</th>
            <th class="actions-header">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="lead in filteredLeads" :key="lead.id" class="lead-row">
            <td>
              <div class="client-cell">
                <div class="client-avatar" :class="lead.temperature">
                  {{ lead.fullName.charAt(0) }}
                </div>
                <div>
                  <span class="client-name">{{ lead.fullName }}</span>
                  <span class="client-phone mono">{{ lead.phone }}</span>
                </div>
              </div>
            </td>
            <td>
              <span class="demand-text">{{ lead.demand }}</span>
            </td>
            <td class="numeric numeric-cell">
              {{ formatCurrency(lead.budget) }}
            </td>
            <td>
              <span class="stage-tag" :class="lead.stage">
                {{ stageLabels[lead.stage] }}
              </span>
            </td>
            <td>
              <StatusBadge 
                :label="lead.temperature" 
                :variant="lead.temperature === 'hot' ? 'danger' : lead.temperature === 'warm' ? 'warning' : 'info'" 
              />
            </td>
            <td class="numeric-cell">
              <span class="score-badge glow-ai">
                {{ getAiScore(lead.temperature, lead.id) }}%
              </span>
            </td>
            <td>
              <span class="time-text">{{ formatDate(lead.lastContactAt) }}</span>
            </td>
            <td>
              <div class="actions-group">
                <!-- AI Insight Button -->
                <button class="icon-btn icon-btn--ai" @click="openAiInsight(lead)" title="AI Insights & Kịch bản">
                  ✨
                </button>
                <!-- Edit Button -->
                <button class="icon-btn" @click="openEditModal(lead)" title="Sửa">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4z" />
                  </svg>
                </button>
                <!-- Delete Button -->
                <button class="icon-btn icon-btn--danger" @click="deleteLeadItem(lead.id, lead.fullName)" title="Xóa">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                  </svg>
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="filteredLeads.length === 0">
            <td colspan="8" class="empty-cell">
              Không tìm thấy khách hàng nào khớp với bộ lọc.
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Mode 2: Kanban View (Collapsible layout friendly columns) -->
    <div v-else class="kanban-container">
      <div 
        v-for="stage in LEAD_STAGES" 
        :key="stage" 
        class="kanban-column glass-card"
      >
        <div class="kanban-column-header">
          <h3>{{ stageLabels[stage] }}</h3>
          <span class="column-count">{{ filteredLeads.filter(l => l.stage === stage).length }}</span>
        </div>

        <div class="kanban-cards-wrapper">
          <div 
            v-for="lead in filteredLeads.filter(l => l.stage === stage)" 
            :key="lead.id" 
            class="kanban-card glass-card glass-card--hoverable"
          >
            <!-- Card Header -->
            <div class="k-card-header">
              <span class="k-client-name">{{ lead.fullName }}</span>
              <span class="score-badge glow-ai">
                {{ getAiScore(lead.temperature, lead.id) }}%
              </span>
            </div>

            <!-- Card Body -->
            <p class="k-demand">{{ lead.demand }}</p>
            
            <div class="k-budget numeric">
              {{ formatCurrency(lead.budget) }}
            </div>

            <!-- Card Footer -->
            <div class="k-card-footer">
              <span class="k-temp" :class="lead.temperature">{{ lead.temperature }}</span>
              
              <!-- Quick Stage Mover Selector for fast testing -->
              <div class="stage-mover select-wrapper">
                <select 
                  :value="lead.stage" 
                  @change="updateLeadStage(lead, ($event.target as HTMLSelectElement).value as LeadStage)"
                >
                  <option v-for="s in LEAD_STAGES" :key="s" :value="s">
                    -> {{ stageLabels[s] }}
                  </option>
                </select>
              </div>
            </div>

            <!-- Quick Action toolbar on card hover -->
            <div class="k-card-actions">
              <button class="k-btn k-btn--ai" @click="openAiInsight(lead)">✨ Insights</button>
              <button class="k-btn" @click="openEditModal(lead)">Sửa</button>
            </div>
          </div>

          <div v-if="filteredLeads.filter(l => l.stage === stage).length === 0" class="k-empty">
            Kéo thả hoặc chuyển trạng thái khách hàng vào cột này
          </div>
        </div>
      </div>
    </div>

    <!-- 1. AI INSIGHT DRAWER (Sliding panel from the right) -->
    <div v-if="activeInsightLead" class="drawer-overlay" @click.self="closeAiInsight">
      <div class="drawer-content glass-card">
        <div class="drawer-header">
          <h3>Phân tích AI Lead Score</h3>
          <button class="close-btn" @click="closeAiInsight">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        <div class="drawer-body">
          <!-- Lead Info -->
          <div class="drawer-lead-profile glass-card">
            <div class="profile-details">
              <h4>{{ activeInsightLead.fullName }}</h4>
              <p class="mono">{{ activeInsightLead.phone }}</p>
              <div class="profile-badges">
                <span class="stage-tag" :class="activeInsightLead.stage">{{ stageLabels[activeInsightLead.stage] }}</span>
                <span class="temp-badge" :class="activeInsightLead.temperature">{{ activeInsightLead.temperature }}</span>
              </div>
            </div>
            <div class="score-display glow-ai">
              <div class="val">{{ getAiScore(activeInsightLead.temperature, activeInsightLead.id) }}%</div>
              <div class="lbl">AI Match</div>
            </div>
          </div>

          <!-- Analysis points -->
          <div class="analysis-section">
            <h5 class="section-title">Phân tích hành vi tương tác</h5>
            <ul class="analysis-list">
              <li class="plus">Khách hàng điền form tư vấn từ Facebook Ads chỉ trong 12 giây.</li>
              <li class="plus">Tìm kiếm phân khúc căn hộ cùng địa bàn khu vực Quận 2 trên 2 trang crawler.</li>
              <li class="plus">Có hồ sơ tài chính tương thích với mức giá bán dự án.</li>
              <li class="minus">Chưa trả lời cuộc gọi đầu tiên của Sales Agent.</li>
            </ul>
          </div>

          <!-- AI Consulting Pitch Message Generator -->
          <div class="ai-pitch-generator glass-card">
            <div class="ai-pitch-header">
              <span>Kịch bản tư vấn AI cá nhân hóa</span>
              <button 
                class="pitch-btn glow-ai" 
                :disabled="isGeneratingScript" 
                @click="generateAiScript(activeInsightLead)"
              >
                ⚡ Tạo kịch bản
              </button>
            </div>
            <div class="ai-pitch-body">
              <p v-if="!generatedScript" class="empty-text">Click nút bên trên để AI sinh nội dung kịch bản gọi điện/Zalo tối ưu nhất cho khách hàng này.</p>
              <div v-else class="rich-script" v-html="generatedScript.replace(/\n/g, '<br>').replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 2. ADD/EDIT LEAD FORM MODAL -->
    <div v-if="showFormModal" class="modal-overlay" @click.self="showFormModal = false">
      <div class="modal-content glass-card">
        <h3>{{ formTitle }}</h3>
        
        <form class="lead-form" @submit.prevent="saveLead">
          <div class="form-group">
            <label>Họ và tên khách hàng</label>
            <input v-model="formLead.fullName" type="text" placeholder="Nguyễn Văn A" required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Số điện thoại</label>
              <input v-model="formLead.phone" type="text" placeholder="0901234567" required />
            </div>
            <div class="form-group">
              <label>Người phụ trách</label>
              <input v-model="formLead.assignedTo" type="text" placeholder="Tên nhân viên sales" />
            </div>
          </div>

          <div class="form-group">
            <label>Nhu cầu chi tiết</label>
            <textarea v-model="formLead.demand" placeholder="Căn hộ 2 phòng ngủ view sông Thủ Thiêm..." rows="2"></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Ngân sách (VND)</label>
              <input v-model.number="formLead.budget" type="number" placeholder="12000000000" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Nhiệt độ (Độ nóng)</label>
              <select v-model="formLead.temperature">
                <option value="hot">Hot 🔥</option>
                <option value="warm">Warm ☀️</option>
                <option value="cold">Cold ❄️</option>
              </select>
            </div>
            <div class="form-group">
              <label>Trạng thái phễu</label>
              <select v-model="formLead.stage">
                <option v-for="stage in LEAD_STAGES" :key="stage" :value="stage">
                  {{ stageLabels[stage] }}
                </option>
              </select>
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="showFormModal = false">Hủy bỏ</button>
            <button type="submit" class="btn-submit glow-yellow">Xác nhận</button>
          </div>
        </form>
      </div>
    </div>
  </div>
  </RoleGate>
</template>

<style scoped>
.leads-header {
  padding: 20px 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.header-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
}

.header-main h2 {
  font-size: 18px;
  font-weight: 700;
  margin: 0 0 4px 0;
  color: var(--color-text-primary);
}

.subtitle-text {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  margin: 0;
}

.add-lead-btn {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12.5px;
  font-weight: 600;
  padding: 10px 18px;
  border-radius: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.add-lead-btn:hover {
  background-color: var(--color-yellow-hover);
  transform: translateY(-1px);
}

/* Controls row */
.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.search-box {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 0 12px;
  height: 38px;
  flex: 1;
  max-width: 300px;
  min-width: 200px;
}

.search-box input {
  background: transparent;
  border: none;
  font-size: 12.5px;
  width: 100%;
  color: var(--color-text-primary);
}

.search-box input:focus {
  outline: none;
}

.search-box svg {
  color: var(--color-text-muted);
}

.filters-group {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.select-wrapper select {
  height: 38px;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 0 12px 0 8px;
  font-size: 12px;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--duration-fast);
}

.select-wrapper select:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

/* View Switcher */
.view-switcher {
  display: flex;
  background: rgba(148, 163, 184, 0.08);
  padding: 3px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.switch-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  background: transparent;
  border: none;
  font-size: 11.5px;
  font-weight: 600;
  color: var(--color-text-secondary);
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.switch-btn:hover {
  color: var(--color-text-primary);
}

.switch-btn.is-active {
  background-color: var(--color-surface);
  color: var(--color-text-primary);
  box-shadow: var(--elevation-surface);
}

/* --- Table Styles --- */
.table-container {
  overflow-x: auto;
}

.leads-table {
  width: 100%;
  border-collapse: collapse;
}

.leads-table th {
  font-size: 10.5px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border);
  padding: 14px 18px;
  text-align: left;
  white-space: nowrap;
}

.leads-table td {
  padding: 14px 18px;
  border-bottom: 1px solid var(--color-divider);
  font-size: 13px;
}

.client-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.client-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #ffffff;
  font-size: 12.5px;
}

.client-avatar.hot { background-color: var(--color-danger); }
.client-avatar.warm { background-color: var(--color-warning); color: #000; }
.client-avatar.cold { background-color: var(--color-info); }

.client-name {
  font-weight: 600;
  display: block;
  color: var(--color-text-primary);
}

.client-phone {
  font-size: 10.5px;
  color: var(--color-text-muted);
}

.demand-text {
  color: var(--color-text-secondary);
  max-width: 240px;
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.numeric-header {
  text-align: right !important;
}

.numeric-cell {
  text-align: right;
  font-weight: 600;
  color: var(--color-text-primary);
}

.stage-tag {
  font-size: 9.5px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  white-space: nowrap;
  display: inline-block;
}
.stage-tag.new { background-color: var(--color-surface-hover); color: var(--color-text-secondary); border: 1px solid var(--color-border); }
.stage-tag.contacted { background-color: var(--color-info-bg); color: var(--color-info); border: 1px solid var(--color-info-border); }
.stage-tag.qualified { background-color: var(--color-warning-bg); color: var(--color-warning); border: 1px solid var(--color-warning-border); }
.stage-tag.viewing { background-color: rgba(168, 85, 247, 0.08); color: rgb(168, 85, 247); border: 1px solid rgba(168, 85, 247, 0.2); }
.stage-tag.closed { background-color: var(--color-success-bg); color: var(--color-success); border: 1px solid var(--color-success-border); }

.score-badge {
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 700;
  color: var(--color-ai);
  background-color: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  padding: 3px 6px;
  border-radius: 6px;
  display: inline-block;
}

.time-text {
  color: var(--color-text-muted);
  font-size: 11.5px;
}

.actions-header {
  text-align: right !important;
}

.actions-group {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

.icon-btn {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.icon-btn:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-strong);
  color: var(--color-text-primary);
}

.icon-btn--ai {
  color: var(--color-ai);
  border-color: var(--color-ai-border);
  background-color: var(--color-ai-bg);
}
.icon-btn--ai:hover {
  box-shadow: var(--color-ai-glow);
}

.icon-btn--danger {
  color: var(--color-danger);
}
.icon-btn--danger:hover {
  background-color: var(--color-danger-bg);
  border-color: var(--color-danger-border);
}

.empty-cell {
  text-align: center;
  color: var(--color-text-muted);
  padding: 48px 0;
  font-size: 13px;
}

/* --- Kanban Styles --- */
.kanban-container {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
  overflow-x: auto;
  align-items: flex-start;
}

.kanban-column {
  min-width: 200px;
  padding: 16px 12px;
}

.kanban-column-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.kanban-column-header h3 {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.column-count {
  font-size: 10px;
  font-weight: 700;
  background-color: var(--color-divider);
  color: var(--color-text-secondary);
  padding: 2px 6px;
  border-radius: 99px;
}

.kanban-cards-wrapper {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 400px;
}

.kanban-card {
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
  overflow: hidden;
}

.k-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 8px;
}

.k-client-name {
  font-weight: 600;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.k-demand {
  font-size: 11.5px;
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.k-budget {
  font-weight: 700;
  color: var(--color-text-primary);
  font-size: 12px;
}

.k-card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid var(--color-divider);
  padding-top: 8px;
  margin-top: 4px;
}

.k-temp {
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
}
.k-temp.hot { color: var(--color-danger); }
.k-temp.warm { color: var(--color-warning); }
.k-temp.cold { color: var(--color-info); }

.stage-mover select {
  font-size: 9px;
  padding: 2px 4px;
  height: 20px;
  border: none;
  background: transparent;
  color: var(--color-text-muted);
}

/* Card quick action bar on hover */
.k-card-actions {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.85);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  opacity: 0;
  transition: opacity var(--duration-fast);
  border-radius: inherit;
}

.kanban-card:hover .k-card-actions {
  opacity: 1;
}

.k-btn {
  font-size: 10.5px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 6px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.k-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.k-btn--ai {
  background: var(--color-ai);
  border-color: var(--color-ai);
}
.k-btn--ai:hover {
  box-shadow: var(--color-ai-glow);
}

.k-empty {
  font-size: 10px;
  color: var(--color-text-muted);
  text-align: center;
  padding: 24px 0;
  border: 1px dashed var(--color-border);
  border-radius: 8px;
}

/* --- Sliding AI Drawer --- */
.drawer-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.4);
  z-index: var(--z-modal);
  display: flex;
  justify-content: flex-end;
}

.drawer-content {
  width: 420px;
  height: 100vh;
  border-radius: 0;
  border-left: 1px solid var(--color-border);
  border-right: none;
  border-top: none;
  border-bottom: none;
  display: flex;
  flex-direction: column;
  box-shadow: var(--elevation-floating);
  animation: slide-in var(--duration-base) var(--ease-spring);
}

@keyframes slide-in {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}

.drawer-header {
  padding: 20px;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.drawer-header h3 {
  margin: 0;
  font-size: 15px;
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
}

.close-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.drawer-body {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.drawer-lead-profile {
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile-details h4 {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.profile-details p {
  margin: 0 0 8px 0;
  font-size: 11px;
  color: var(--color-text-muted);
}

.profile-badges {
  display: flex;
  gap: 6px;
}

.score-display {
  width: 68px;
  height: 68px;
  border-radius: 50%;
  background-color: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.score-display .val {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-ai);
  font-family: var(--font-mono);
  line-height: 1.1;
}

.score-display .lbl {
  font-size: 8px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.analysis-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.section-title {
  margin: 0;
  font-size: 12.5px;
  font-weight: 600;
}

.analysis-list {
  margin: 0;
  padding-left: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.analysis-list li.plus {
  list-style-type: '✅ ';
}
.analysis-list li.minus {
  list-style-type: '⚠️ ';
}

/* AI Consulting script */
.ai-pitch-generator {
  display: flex;
  flex-direction: column;
}

.ai-pitch-header {
  padding: 10px 14px;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11.5px;
  font-weight: 600;
}

.pitch-btn {
  background: var(--color-ai);
  color: #fff;
  border: none;
  font-size: 10px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 6px;
  cursor: pointer;
}

.ai-pitch-body {
  padding: 14px;
  min-height: 120px;
  font-size: 12px;
  line-height: 1.5;
  color: var(--color-text-primary);
}

.rich-script {
  animation: fadein var(--duration-base);
}

/* --- Form Modals --- */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
}

.modal-content {
  width: 440px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: var(--elevation-floating);
  animation: fadein var(--duration-base) var(--ease-spring);
}

.modal-content h3 {
  margin: 0;
  font-size: 15px;
}

.lead-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-group label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.form-group input,
.form-group textarea,
.form-group select {
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.form-group input:focus,
.form-group textarea:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

.btn-cancel {
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: transparent;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.btn-submit {
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
  border: none;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

/* Breakpoints */
@media (max-width: 900px) {
  .kanban-container {
    grid-template-columns: repeat(3, 1fr);
  }
}
@media (max-width: 580px) {
  .kanban-container {
    grid-template-columns: 1fr;
  }
  .controls-row {
    flex-direction: column;
    align-items: stretch;
  }
  .search-box {
    max-width: none;
  }
}
</style>
