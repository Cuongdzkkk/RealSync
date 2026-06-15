<script setup lang="ts">
import { computed, ref } from 'vue';
import { useProjectStore, type Project } from '@/stores/useProjectStore';
import { useLeadStore } from '@/stores/useLeadStore';
import { useToastStore } from '@/stores/useToastStore';
import { formatCurrency } from '@/utils/format';
import RoleGate from '@/components/common/RoleGate.vue';

const projectStore = useProjectStore();
const leadStore = useLeadStore();
const toastStore = useToastStore();

// --- Filter States ---
const searchQuery = ref('');
const selectedLocation = ref<string>('all');
const selectedStatus = ref<string>('all');

// --- Drawer & Modal States ---
const activeDetailProject = ref<Project | null>(null);
const showFormModal = ref(false);
const editingProject = ref<Project | null>(null);

// --- Form State ---
const formTitle = computed(() => editingProject.value ? 'Cập nhật Dự án' : 'Thêm Dự án mới');
const formProject = ref({
  name: '',
  developer: '',
  location: '',
  type: 'Căn hộ cao cấp',
  status: 'selling' as 'upcoming' | 'selling' | 'delivered',
  priceRange: 'Từ 80 - 120 triệu/m²',
  progress: 50,
  totalUnits: '1,000 căn',
  aiInterest: 85,
  imageUrl: 'https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=900&q=80'
});

// Computed filtered items
const filteredProjects = computed(() => {
  return projectStore.items.filter(p => {
    const matchesSearch = p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          p.developer.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          p.location.toLowerCase().includes(searchQuery.value.toLowerCase());
    
    let matchesLoc = true;
    if (selectedLocation.value !== 'all') {
      matchesLoc = p.location.includes(selectedLocation.value);
    }

    const matchesStatus = selectedStatus.value === 'all' || p.status === selectedStatus.value;
    return matchesSearch && matchesLoc && matchesStatus;
  });
});

// AI Leads Match Simulator:
// Finds leads in leadStore whose budget is above a threshold or whose demand matches the project location.
const matchedLeadsForActiveProject = computed(() => {
  if (!activeDetailProject.value) return [];
  
  // Filter leads that match some basic keywords or have high budgets
  return leadStore.items.filter(lead => {
    // If project is Grand Marina ( Quận 1, budget very high > 10B) or Metropole (Thủ Đức > 8B)
    const isPremiumProject = activeDetailProject.value!.priceRange.includes('180') || activeDetailProject.value!.priceRange.includes('350');
    if (isPremiumProject) {
      return lead.budget >= 8000000000 || lead.temperature === 'hot';
    }
    return lead.budget >= 3000000000;
  }).map(lead => {
    // Add a simulated match score
    const hash = lead.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    const score = 85 + (hash % 15);
    return {
      ...lead,
      matchScore: score
    };
  }).sort((a, b) => b.matchScore - a.matchScore).slice(0, 3);
});

// File Upload Handler
const fileInput = ref<HTMLInputElement | null>(null);

function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  if (!file.type.startsWith('image/')) {
    toastStore.warning('Sai định dạng', 'Vui lòng tải lên file hình ảnh hợp lệ.');
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    if (e.target?.result) {
      formProject.value.imageUrl = e.target.result as string;
      toastStore.success('Thành công', 'Đã tải và lưu trữ ảnh phối cảnh dự án.');
    }
  };
  reader.readAsDataURL(file);
}

// Actions
function openDetail(id: string) {
  const item = projectStore.items.find(p => p.id === id);
  if (item) {
    activeDetailProject.value = item;
  }
}

function closeDetail() {
  activeDetailProject.value = null;
}

function openAddModal() {
  editingProject.value = null;
  formProject.value = {
    name: '',
    developer: '',
    location: '',
    type: 'Căn hộ cao cấp',
    status: 'selling',
    priceRange: 'Từ 80 - 120 triệu/m²',
    progress: 30,
    totalUnits: '1,200 căn',
    aiInterest: 85,
    imageUrl: 'https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=900&q=80'
  };
  showFormModal.value = true;
}

function openEditModal(project: Project) {
  editingProject.value = project;
  formProject.value = {
    name: project.name,
    developer: project.developer,
    location: project.location,
    type: project.type,
    status: project.status,
    priceRange: project.priceRange,
    progress: project.progress,
    totalUnits: project.totalUnits,
    aiInterest: project.aiInterest,
    imageUrl: project.imageUrl
  };
  showFormModal.value = true;
}

function saveProject() {
  if (!formProject.value.name || !formProject.value.developer || !formProject.value.location) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng điền tên dự án, nhà phát triển và địa điểm.');
    return;
  }

  if (editingProject.value) {
    const updated: Project = {
      ...editingProject.value,
      ...formProject.value
    };
    projectStore.updateProject(updated);
    toastStore.success('Thành công', `Đã cập nhật dự án: ${formProject.value.name}`);
  } else {
    const created: Project = {
      id: `p-${Date.now()}`,
      ...formProject.value,
      createdAt: new Date().toISOString()
    };
    projectStore.addProject(created);
    toastStore.success('Thành công', `Đã thêm dự án mới: ${formProject.value.name}`);
  }
  showFormModal.value = false;
}

function deleteProjectItem(id: string, name: string) {
  if (confirm(`Bạn muốn xóa dự án ${name}?`)) {
    projectStore.deleteProject(id);
    toastStore.success('Đã xóa', `Đã xóa dự án ${name}`);
    if (activeDetailProject.value?.id === id) {
      activeDetailProject.value = null;
    }
  }
}
</script>

<template>
  <RoleGate :roles="['Admin', 'Marketing']">
    <div class="page">
      <!-- Header Section -->
      <div class="projects-header glass-card">
      <div class="header-main">
        <div>
          <h2>Danh sách Dự án</h2>
          <p class="subtitle-text">Danh mục các dự án đại đô thị và chung cư cao cấp tích hợp phân tích hành vi nhu cầu khách hàng.</p>
        </div>
        <button class="add-project-btn glow-yellow" @click="openAddModal">
          + Thêm dự án mới
        </button>
      </div>

      <!-- Filters Row -->
      <div class="controls-row">
        <!-- Search -->
        <div class="search-box">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm theo tên dự án, chủ đầu tư..." 
          />
        </div>

        <!-- Filters group -->
        <div class="filters-group">
          <!-- District -->
          <div class="select-wrapper">
            <select v-model="selectedLocation">
              <option value="all">Tất cả khu vực</option>
              <option value="Quận 1">Quận 1</option>
              <option value="Thủ Đức">TP. Thủ Đức</option>
              <option value="Quận 4">Quận 4</option>
            </select>
          </div>

          <!-- Status -->
          <div class="select-wrapper">
            <select v-model="selectedStatus">
              <option value="all">Tất cả trạng thái</option>
              <option value="upcoming">Sắp mở bán</option>
              <option value="selling">Đang mở bán</option>
              <option value="delivered">Đã bàn giao</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Project Grid -->
    <div class="projects-grid">
      <div 
        v-for="p in filteredProjects" 
        :key="p.id"
        class="project-card glass-card glass-card--hoverable"
        @click="openDetail(p.id)"
      >
        <!-- Media Section -->
        <div class="project-media">
          <img :src="p.imageUrl" :alt="p.name" />
          <span class="status-pill" :class="p.status">
            {{ p.status === 'upcoming' ? 'Sắp mở bán' : p.status === 'selling' ? 'Đang mở bán' : 'Đã bàn giao' }}
          </span>

          <div class="interest-badge glow-ai">
            🔥 AI Interest: <strong class="numeric">{{ p.aiInterest }}%</strong>
          </div>
        </div>

        <!-- Content Section -->
        <div class="project-body">
          <span class="developer-lbl">{{ p.developer }}</span>
          <h3 class="project-title">{{ p.name }}</h3>
          <p class="project-location">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" />
            </svg>
            <span>{{ p.location }}</span>
          </p>

          <div class="price-range">
            <span class="lbl">Tầm giá:</span>
            <span class="val">{{ p.priceRange }}</span>
          </div>

          <!-- Progress -->
          <div class="progress-section">
            <div class="progress-info">
              <span>Tiến độ xây dựng</span>
              <span class="numeric">{{ p.progress }}%</span>
            </div>
            <div class="progress-bar-container">
              <div class="progress-bar-fill" :style="{ width: `${p.progress}%` }"></div>
            </div>
          </div>
        </div>

        <!-- Action Overlay -->
        <div class="card-action-overlay" @click.stop>
          <button class="action-btn action-btn--primary" @click="openDetail(p.id)">Xem Chi Tiết</button>
          <div class="action-subgroup">
            <button class="action-btn icon-only" @click="openEditModal(p)" title="Sửa">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4z" />
              </svg>
            </button>
            <button class="action-btn icon-only danger" @click="deleteProjectItem(p.id, p.name)" title="Xóa">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
              </svg>
            </button>
          </div>
        </div>
      </div>
      
      <div v-if="filteredProjects.length === 0" class="empty-state-card glass-card">
        Không tìm thấy dự án nào khớp với điều kiện lọc.
      </div>
    </div>

    <!-- 1. DETAIL PROJECT DRAWER (With CRM Leads Matching!) -->
    <div v-if="activeDetailProject" class="drawer-overlay" @click.self="closeDetail">
      <div class="drawer-content glass-card">
        <div class="drawer-header">
          <h3>Chi tiết & Nhu cầu AI</h3>
          <button class="close-btn" @click="closeDetail">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        <div class="drawer-body">
          <div class="drawer-media">
            <img :src="activeDetailProject.imageUrl" :alt="activeDetailProject.name" />
            <span class="status-badge-overlay" :class="activeDetailProject.status">{{ activeDetailProject.status }}</span>
          </div>

          <div class="detail-header-block">
            <h4 class="project-name-detail">{{ activeDetailProject.name }}</h4>
            <p class="developer-detail">{{ activeDetailProject.developer }}</p>
            <p class="location-detail">{{ activeDetailProject.location }}</p>
          </div>

          <!-- Specs -->
          <div class="detail-specs-grid">
            <div class="spec-cell">
              <span class="lbl">Quy mô</span>
              <span class="val">{{ activeDetailProject.totalUnits }}</span>
            </div>
            <div class="spec-cell border-left">
              <span class="lbl">Loại hình</span>
              <span class="val">{{ activeDetailProject.type }}</span>
            </div>
          </div>

          <!-- AI Leads Match Section (The ultimate CRM integration feature) -->
          <div class="ai-leads-match-section glass-card">
            <div class="match-header glow-ai">
              <span>💡 Đề xuất khách hàng (Leads Match)</span>
              <span class="match-count">Tìm thấy {{ matchedLeadsForActiveProject.length }} lead</span>
            </div>

            <div class="leads-match-list">
              <div 
                v-for="lead in matchedLeadsForActiveProject" 
                :key="lead.id" 
                class="lead-match-item"
              >
                <div class="lead-match-info">
                  <span class="lead-name">{{ lead.fullName }}</span>
                  <span class="lead-demand-preview">{{ lead.demand }}</span>
                  <span class="lead-budget-val numeric">{{ formatCurrency(lead.budget) }}</span>
                </div>
                <div class="lead-match-pct">
                  <span class="pct">{{ lead.matchScore }}%</span>
                  <span class="lbl">Match</span>
                </div>
              </div>
              
              <div v-if="matchedLeadsForActiveProject.length === 0" class="empty-leads">
                Không tìm thấy lead nào phù hợp với ngân sách dự án.
              </div>
            </div>
          </div>

          <!-- AI Market Interest Pros/Cons -->
          <div class="ai-pros-cons-section">
            <h5 class="section-title">Nhận định thị trường AI</h5>
            <ul class="analysis-list">
              <li class="pro">Tỷ lệ quan tâm tìm kiếm trên Google & Facebook tăng **14%** so với tháng trước.</li>
              <li class="pro">Thích hợp cho cả đầu tư dòng tiền cho thuê và an cư lâu dài.</li>
              <li class="con">Giá bán thuộc phân khúc cao cấp, thời gian thu hồi vốn dài hơn trung bình.</li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- 2. ADD/EDIT PROJECT FORM MODAL (With local device image upload!) -->
    <div v-if="showFormModal" class="modal-overlay" @click.self="showFormModal = false">
      <div class="modal-content glass-card">
        <h3>{{ formTitle }}</h3>
        
        <form class="project-form" @submit.prevent="saveProject">
          <div class="form-group">
            <label>Tên dự án bất động sản</label>
            <input v-model="formProject.name" type="text" placeholder="Vinhomes Grand Park, Metropole..." required />
          </div>

          <div class="form-group">
            <label>Nhà phát triển (Chủ đầu tư)</label>
            <input v-model="formProject.developer" type="text" placeholder="Vingroup, Masterise Homes..." required />
          </div>

          <div class="form-group">
            <label>Địa điểm dự án</label>
            <input v-model="formProject.location" type="text" placeholder="Quận 9, TP. Thủ Đức" required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Loại hình BĐS</label>
              <input v-model="formProject.type" type="text" placeholder="Căn hộ cao cấp, Shophouse..." />
            </div>
            <div class="form-group">
              <label>Trạng thái mở bán</label>
              <select v-model="formProject.status">
                <option value="upcoming">Sắp mở bán</option>
                <option value="selling">Đang mở bán</option>
                <option value="delivered">Đã bàn giao</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Tầm giá bán</label>
              <input v-model="formProject.priceRange" type="text" placeholder="Từ 60 - 80 triệu/m²" />
            </div>
            <div class="form-group">
              <label>Tiến độ (%)</label>
              <input v-model.number="formProject.progress" type="number" placeholder="60" required />
            </div>
          </div>

          <!-- Local Image Upload Block -->
          <div class="form-group">
            <label>Hình ảnh phối cảnh dự án</label>
            
            <input 
              type="file" 
              ref="fileInput" 
              accept="image/*" 
              class="hidden-input" 
              @change="handleFileUpload" 
            />

            <!-- Preview Mode -->
            <div v-if="formProject.imageUrl" class="image-preview-container">
              <img :src="formProject.imageUrl" class="image-preview-thumbnail" />
              <div class="preview-actions">
                <button type="button" class="action-btn--change" @click="fileInput?.click()">Thay đổi</button>
                <button type="button" class="action-btn--remove" @click="formProject.imageUrl = ''">Xóa ảnh</button>
              </div>
            </div>

            <!-- Upload Zone -->
            <div v-else class="upload-dropzone" @click="fileInput?.click()">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/>
              </svg>
              <span class="upload-title">Tải ảnh dự án từ máy tính/điện thoại</span>
              <span class="upload-subtitle">Hỗ trợ các định dạng PNG, JPG, JPEG</span>
            </div>

            <!-- URL input fallback -->
            <div class="url-input-fallback">
              <span class="or-divider">Hoặc nhập URL ảnh trực tuyến:</span>
              <input v-model="formProject.imageUrl" type="text" placeholder="https://images.unsplash.com/..." />
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
.projects-header {
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

.add-project-btn {
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

.add-project-btn:hover {
  background-color: var(--color-yellow-hover);
  transform: translateY(-1px);
}

/* Controls */
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

/* Project grid */
.projects-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
}

.project-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
  position: relative;
  cursor: pointer;
}

.project-media {
  aspect-ratio: 16 / 10;
  position: relative;
  overflow: hidden;
  background-color: var(--color-divider);
}

.project-media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--duration-slow) var(--ease-standard);
}

.project-card:hover .project-media img {
  transform: scale(1.05);
}

.status-pill {
  position: absolute;
  top: 12px;
  left: 12px;
  font-size: 9px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  color: #fff;
  letter-spacing: 0.05em;
  z-index: 2;
}
.status-pill.upcoming { background-color: rgba(245, 158, 11, 0.85); }
.status-pill.selling { background-color: rgba(59, 130, 246, 0.85); }
.status-pill.delivered { background-color: rgba(16, 185, 129, 0.85); }

.interest-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  font-size: 9.5px;
  font-weight: 700;
  background: rgba(15, 23, 42, 0.8);
  border: 1px solid var(--color-ai-border);
  color: var(--color-ai);
  padding: 3px 8px;
  border-radius: 6px;
  z-index: 2;
}

.project-body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.developer-lbl {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  letter-spacing: 0.05em;
}

.project-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.project-location {
  font-size: 12px;
  color: var(--color-text-secondary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-location svg {
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.price-range {
  font-size: 12px;
  display: flex;
  gap: 4px;
  margin-top: 4px;
}

.price-range .lbl {
  color: var(--color-text-muted);
}

.price-range .val {
  color: var(--color-text-primary);
  font-weight: 600;
}

/* Progress bar styles */
.progress-section {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: auto;
  padding-top: 10px;
  border-top: 1px solid var(--color-divider);
}

.progress-info {
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.progress-bar-container {
  height: 6px;
  background-color: var(--color-divider);
  border-radius: 99px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background: var(--color-yellow);
  border-radius: 99px;
}

/* Actions Overlay on Hover */
.card-action-overlay {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.85);
  backdrop-filter: blur(4px);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  opacity: 0;
  transition: opacity var(--duration-fast);
  border-radius: inherit;
  z-index: 5;
  padding: 20px;
}

.project-card:hover .card-action-overlay {
  opacity: 1;
}

.action-btn {
  font-size: 12px;
  font-weight: 600;
  height: 36px;
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 16px;
  transition: all var(--duration-fast);
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.25);
  border-color: rgba(255, 255, 255, 0.3);
}

.action-btn--primary {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  box-shadow: var(--color-yellow-glow);
  width: 120px;
}
.action-btn--primary:hover {
  background: var(--color-yellow-hover);
}

.action-subgroup {
  display: flex;
  gap: 8px;
}

.action-btn.icon-only {
  width: 36px;
  padding: 0;
}

.action-btn.icon-only.danger:hover {
  background-color: var(--color-danger);
  border-color: var(--color-danger);
}

.empty-state-card {
  grid-column: 1 / -1;
  text-align: center;
  padding: 48px;
  color: var(--color-text-muted);
  font-size: 13px;
}

/* --- Detail Drawer --- */
.drawer-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.4);
  z-index: var(--z-modal);
  display: flex;
  justify-content: flex-end;
}

.drawer-content {
  width: 440px;
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

.drawer-media {
  width: 100%;
  aspect-ratio: 16 / 10;
  border-radius: 12px;
  overflow: hidden;
  position: relative;
  background-color: var(--color-divider);
}

.drawer-media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.status-badge-overlay {
  position: absolute;
  top: 12px;
  left: 12px;
  font-size: 9px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  color: #fff;
}
.status-badge-overlay.upcoming { background-color: var(--color-warning); color: #000; }
.status-badge-overlay.selling { background-color: var(--color-info); }
.status-badge-overlay.delivered { background-color: var(--color-success); }

.detail-header-block {
  display: flex;
  flex-direction: column;
}

.project-name-detail {
  font-size: 16px;
  font-weight: 700;
  margin: 0 0 4px 0;
  color: var(--color-text-primary);
}

.developer-detail {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-text-muted);
  margin: 0 0 8px 0;
}

.location-detail {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  margin: 0;
}

.detail-specs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 12px;
  border-radius: 8px;
}

.spec-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.spec-cell.border-left {
  border-left: 1px solid var(--color-divider);
  padding-left: 16px;
}

.spec-cell .lbl {
  font-size: 9px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.spec-cell .val {
  font-size: 12.5px;
  font-weight: 600;
  color: var(--color-text-primary);
}

/* Leads Match List */
.ai-leads-match-section {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.match-header {
  padding: 10px 14px;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 700;
  font-size: 12px;
  color: var(--color-ai);
}

.match-count {
  font-size: 9px;
  font-weight: 700;
  background-color: var(--color-ai-bg);
  padding: 2px 6px;
  border-radius: 99px;
}

.leads-match-list {
  display: flex;
  flex-direction: column;
  padding: 8px 0;
}

.lead-match-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  border-bottom: 1px solid var(--color-divider);
}

.lead-match-item:last-child {
  border-bottom: none;
}

.lead-match-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.lead-match-info .lead-name {
  font-size: 12.5px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.lead-match-info .lead-demand-preview {
  font-size: 10.5px;
  color: var(--color-text-muted);
}

.lead-budget-val {
  font-size: 11px;
  color: var(--color-text-secondary);
  font-weight: 600;
}

.lead-match-pct {
  display: flex;
  flex-direction: column;
  align-items: center;
  background-color: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  padding: 4px 8px;
  border-radius: 6px;
  min-width: 44px;
}

.lead-match-pct .pct {
  font-size: 11.5px;
  font-weight: 700;
  color: var(--color-ai);
  font-family: var(--font-mono);
}

.lead-match-pct .lbl {
  font-size: 7.5px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.empty-leads {
  font-size: 11px;
  color: var(--color-text-muted);
  text-align: center;
  padding: 16px 0;
}

.ai-pros-cons-section {
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

.analysis-list li.pro {
  list-style-type: '📈 ';
}

.analysis-list li.con {
  list-style-type: '📉 ';
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
  width: 460px;
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

.project-form {
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
.form-group select {
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.form-group input:focus,
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

/* Image Upload specific classes */
.hidden-input {
  display: none;
}

.image-preview-container {
  display: flex;
  align-items: center;
  gap: 14px;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 10px;
  border-radius: 8px;
}

.image-preview-thumbnail {
  width: 72px;
  height: 48px;
  object-fit: cover;
  border-radius: 6px;
  border: 1px solid var(--color-border);
}

.preview-actions {
  display: flex;
  gap: 8px;
}

.action-btn--change,
.action-btn--remove {
  font-size: 11px;
  font-weight: 600;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--duration-fast);
}

.action-btn--change {
  background: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
}

.action-btn--change:hover {
  background: var(--color-divider);
}

.action-btn--remove {
  background: var(--color-danger-bg);
  border: 1px solid var(--color-danger-border);
  color: var(--color-danger);
}

.action-btn--remove:hover {
  background: var(--color-danger);
  color: #ffffff;
}

.upload-dropzone {
  border: 1px dashed var(--color-border);
  background-color: var(--color-surface-glass);
  border-radius: 8px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.upload-dropzone:hover {
  border-color: var(--color-border-strong);
  background-color: var(--color-surface-hover);
}

.upload-dropzone svg {
  color: var(--color-text-muted);
}

.upload-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.upload-subtitle {
  font-size: 10px;
  color: var(--color-text-muted);
  text-align: center;
}

.url-input-fallback {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.or-divider {
  font-size: 10px;
  color: var(--color-text-muted);
}
</style>
