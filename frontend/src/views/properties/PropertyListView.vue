<script setup lang="ts">
import { computed, ref } from 'vue';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useToastStore } from '@/stores/useToastStore';
import type { Property, PropertyStatus } from '@/types/property';
import { PROPERTY_STATUSES } from '@/utils/constants';
import { formatCurrency } from '@/utils/format';
import PropertyCard from '@/components/property/PropertyCard.vue';

const propertyStore = usePropertyStore();
const toastStore = useToastStore();

// --- Filter States ---
const searchQuery = ref('');
const selectedSource = ref<string>('all');
const selectedStatus = ref<string>('all');

// --- Drawer & Modal States ---
const activeDetailProperty = ref<Property | null>(null);
const showFormModal = ref(false);
const editingProperty = ref<Property | null>(null);

// --- Form State ---
const formTitle = computed(() => editingProperty.value ? 'Cập nhật Bất động sản' : 'Thêm Bất động sản mới');
const formProp = ref({
  title: '',
  address: '',
  area: 'TP. Thủ Đức',
  price: 0,
  acreage: 0,
  bedrooms: 1,
  status: 'verified' as PropertyStatus,
  source: 'internal',
  imageUrl: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=900&q=80',
  aiScore: 85
});

// Computed filtered items
const filteredProperties = computed(() => {
  return propertyStore.items.filter(prop => {
    const matchesSearch = prop.title.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          prop.address.toLowerCase().includes(searchQuery.value.toLowerCase());
    const matchesSource = selectedSource.value === 'all' || prop.source === selectedSource.value;
    const matchesStatus = selectedStatus.value === 'all' || prop.status === selectedStatus.value;
    return matchesSearch && matchesSource && matchesStatus;
  });
});

// AI Valuation Simulator
const aiValuation = computed(() => {
  if (!activeDetailProperty.value) return 0;
  // Simulates market price estimation (usually within +- 5% of actual price)
  const hash = activeDetailProperty.value.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const factor = 0.95 + ((hash % 10) / 100); // 0.95 to 1.04
  return Math.round(activeDetailProperty.value.price * factor);
});

const priceDiffPercent = computed(() => {
  if (!activeDetailProperty.value) return '0%';
  const diff = (activeDetailProperty.value.price - aiValuation.value) / aiValuation.value * 100;
  return `${diff > 0 ? '+' : ''}${diff.toFixed(1)}%`;
});

// Open/Close Actions
function openDetail(id: string) {
  const item = propertyStore.items.find(p => p.id === id);
  if (item) {
    activeDetailProperty.value = item;
  }
}

function closeDetail() {
  activeDetailProperty.value = null;
}

function openAddModal() {
  editingProperty.value = null;
  formProp.value = {
    title: '',
    address: '',
    area: 'TP. Thủ Đức',
    price: 0,
    acreage: 0,
    bedrooms: 2,
    status: 'verified',
    source: 'internal',
    imageUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=900&q=80',
    aiScore: 85
  };
  showFormModal.value = true;
}

function openEditModal(prop: Property) {
  editingProperty.value = prop;
  formProp.value = {
    title: prop.title,
    address: prop.address,
    area: prop.area,
    price: prop.price,
    acreage: prop.acreage,
    bedrooms: prop.bedrooms,
    status: prop.status,
    source: prop.source,
    imageUrl: prop.imageUrl,
    aiScore: prop.aiScore
  };
  showFormModal.value = true;
}

function saveProperty() {
  if (!formProp.value.title || !formProp.value.address || formProp.value.price <= 0) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng điền tiêu đề, địa chỉ và giá trị bất động sản.');
    return;
  }

  if (editingProperty.value) {
    const updated: Property = {
      ...editingProperty.value,
      ...formProp.value
    };
    propertyStore.updateProperty(updated);
    toastStore.success('Thành công', `Đã cập nhật bất động sản: ${formProp.value.title}`);
  } else {
    const created: Property = {
      id: `p-${Date.now()}`,
      ...formProp.value,
      createdAt: new Date().toISOString()
    };
    propertyStore.addProperty(created);
    toastStore.success('Thành công', `Đã thêm bất động sản mới: ${formProp.value.title}`);
  }
  showFormModal.value = false;
}

function deletePropertyItem(id: string) {
  if (confirm('Bạn chắc chắn muốn xóa tin đăng bất động sản này?')) {
    propertyStore.deleteProperty(id);
    toastStore.success('Đã xóa', 'Tin đăng bất động sản đã được xóa khỏi hệ thống.');
    if (activeDetailProperty.value?.id === id) {
      activeDetailProperty.value = null;
    }
  }
}

const fileInput = ref<HTMLInputElement | null>(null);

function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  if (!file.type.startsWith('image/')) {
    toastStore.warning('Sai định dạng', 'Vui lòng chọn một file hình ảnh.');
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    if (e.target?.result) {
      formProp.value.imageUrl = e.target.result as string;
      toastStore.success('Tải ảnh thành công', 'Ảnh đã được nạp làm hình nền của sản phẩm BĐS.');
    }
  };
  reader.readAsDataURL(file);
}
</script>

<template>
  <div class="page">
    <!-- Header Section -->
    <div class="properties-header glass-card">
      <div class="header-main">
        <div>
          <h2>Kho Bất động sản</h2>
          <p class="subtitle-text">Hệ thống kho hàng tổng hợp từ nguồn Crawler tự động và tin đăng nội bộ của RealSync.</p>
        </div>
        <button class="add-prop-btn glow-yellow" @click="openAddModal">
          + Thêm sản phẩm BĐS
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
            placeholder="Tìm theo tiêu đề, địa chỉ..." 
          />
        </div>

        <!-- Filters group -->
        <div class="filters-group">
          <!-- Source -->
          <div class="select-wrapper">
            <select v-model="selectedSource">
              <option value="all">Tất cả nguồn</option>
              <option value="internal">Tin nội bộ</option>
              <option value="batdongsan.com.vn">Batdongsan.com</option>
              <option value="chotot.com">Chợ Tốt BĐS</option>
            </select>
          </div>

          <!-- Status -->
          <div class="select-wrapper">
            <select v-model="selectedStatus">
              <option value="all">Tất cả trạng thái</option>
              <option v-for="status in PROPERTY_STATUSES" :key="status" :value="status">
                {{ status === 'draft' ? 'Nháp' : status === 'verified' ? 'Đã duyệt' : status === 'published' ? 'Đã đăng' : 'Hết hạn' }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Product Grid -->
    <div class="properties-grid">
      <div 
        v-for="prop in filteredProperties" 
        :key="prop.id"
        class="grid-item"
      >
        <PropertyCard 
          :property="prop"
          @click="openDetail"
          @edit="openEditModal"
          @delete="deletePropertyItem"
        />
      </div>
      
      <div v-if="filteredProperties.length === 0" class="empty-state-card glass-card">
        Không tìm thấy sản phẩm bất động sản nào phù hợp với điều kiện tìm kiếm.
      </div>
    </div>

    <!-- 1. DETAIL PROPERTY INSIGHT DRAWER (Sliding panel) -->
    <div v-if="activeDetailProperty" class="drawer-overlay" @click.self="closeDetail">
      <div class="drawer-content glass-card">
        <div class="drawer-header">
          <h3>Chi tiết & AI Định giá</h3>
          <button class="close-btn" @click="closeDetail">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        <div class="drawer-body">
          <div class="drawer-media">
            <img :src="activeDetailProperty.imageUrl" :alt="activeDetailProperty.title" />
            <span class="source-tag-overlay">{{ activeDetailProperty.source }}</span>
          </div>

          <div class="detail-info-block">
            <h4 class="detail-title">{{ activeDetailProperty.title }}</h4>
            <p class="detail-address">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" />
              </svg>
              {{ activeDetailProperty.address }}
            </p>
            
            <div class="detail-specs">
              <span class="spec-badge">📐 Diện tích: {{ activeDetailProperty.acreage }} m²</span>
              <span class="spec-badge">🛏️ Số phòng ngủ: {{ activeDetailProperty.bedrooms }} PN</span>
            </div>
          </div>

          <!-- AI Valuation Model Widget -->
          <div class="ai-valuation-section glass-card">
            <div class="ai-val-header">
              <span class="sparkle-icon">✨</span>
              <span>AI Valuation Engine</span>
            </div>
            
            <div class="val-grid">
              <div class="val-col">
                <span class="lbl">Giá chào bán</span>
                <strong class="val numeric">{{ formatCurrency(activeDetailProperty.price) }}</strong>
              </div>
              <div class="val-col border-left">
                <span class="lbl">AI Định giá thị trường</span>
                <strong class="val numeric text-ai">{{ formatCurrency(aiValuation) }}</strong>
              </div>
            </div>

            <div class="val-analysis-bar" :class="{ 'overpriced': activeDetailProperty.price > aiValuation }">
              <span>Độ chênh lệch giá chào so với AI: <strong>{{ priceDiffPercent }}</strong></span>
              <span class="status-lbl">
                {{ activeDetailProperty.price > aiValuation ? 'Giá cao hơn thị trường' : 'Giá hấp dẫn / Đáng mua' }}
              </span>
            </div>
          </div>

          <!-- Position Insights -->
          <div class="location-insights-section">
            <h5 class="section-title">Đánh giá tiềm năng vị trí</h5>
            <ul class="insights-list">
              <li>Mật độ tìm kiếm bất động sản tương tự trong khu vực {{ activeDetailProperty.area }} tăng **18.4%** tuần qua.</li>
              <li>Liền kề hạ tầng quy hoạch trọng điểm, thanh khoản cao.</li>
              <li>Độ chính xác dữ liệu thu thập tin đăng: **94%**.</li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- 2. ADD/EDIT PROPERTY FORM MODAL -->
    <div v-if="showFormModal" class="modal-overlay" @click.self="showFormModal = false">
      <div class="modal-content glass-card">
        <h3>{{ formTitle }}</h3>
        
        <form class="prop-form" @submit.prevent="saveProperty">
          <div class="form-group">
            <label>Tiêu đề tin đăng</label>
            <input v-model="formProp.title" type="text" placeholder="Căn hộ 2PN Vinhomes view sông Sài Gòn..." required />
          </div>

          <div class="form-group">
            <label>Địa chỉ bất động sản</label>
            <input v-model="formProp.address" type="text" placeholder="Landmark 81, Vinhomes Central Park, Bình Thạnh" required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Khu vực (Quận/Huyện)</label>
              <select v-model="formProp.area">
                <option value="TP. Thủ Đức">TP. Thủ Đức</option>
                <option value="Quận 7">Quận 7</option>
                <option value="Bình Thạnh">Bình Thạnh</option>
                <option value="Bình Chánh">Bình Chánh</option>
              </select>
            </div>
            <div class="form-group">
              <label>Nguồn tin</label>
              <select v-model="formProp.source">
                <option value="internal">Tin nội bộ (Internal)</option>
                <option value="batdongsan.com.vn">Batdongsan.com.vn</option>
                <option value="chotot.com">Chợ Tốt BĐS</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Giá trị BĐS (VND)</label>
              <input v-model.number="formProp.price" type="number" placeholder="4500000000" required />
            </div>
            <div class="form-group">
              <label>Diện tích (m²)</label>
              <input v-model.number="formProp.acreage" type="number" placeholder="75" required />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Số phòng ngủ</label>
              <input v-model.number="formProp.bedrooms" type="number" placeholder="2" />
            </div>
            <div class="form-group">
              <label>Trạng thái duyệt</label>
              <select v-model="formProp.status">
                <option value="draft">Nháp (Draft)</option>
                <option value="verified">Đã duyệt (Verified)</option>
                <option value="published">Đã đăng (Published)</option>
                <option value="expired">Hết hạn (Expired)</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label>Hình ảnh sản phẩm</label>
            
            <!-- Hidden input for file selection -->
            <input 
              type="file" 
              ref="fileInput" 
              accept="image/*" 
              class="hidden-input" 
              @change="handleFileUpload" 
            />

            <!-- Preview Mode -->
            <div v-if="formProp.imageUrl" class="image-preview-container">
              <img :src="formProp.imageUrl" class="image-preview-thumbnail" />
              <div class="preview-actions">
                <button type="button" class="action-btn--change" @click="fileInput?.click()">Thay đổi</button>
                <button type="button" class="action-btn--remove" @click="formProp.imageUrl = ''">Xóa ảnh</button>
              </div>
            </div>

            <!-- Upload Area (when no image) -->
            <div v-else class="upload-dropzone" @click="fileInput?.click()">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/>
              </svg>
              <span class="upload-title">Tải ảnh lên từ thiết bị</span>
              <span class="upload-subtitle">Hỗ trợ PNG, JPG, JPEG từ máy tính/điện thoại</span>
            </div>

            <!-- Paste URL fallback -->
            <div class="url-input-fallback">
              <span class="or-divider">Hoặc nhập URL ảnh trực tiếp:</span>
              <input v-model="formProp.imageUrl" type="text" placeholder="https://images.unsplash.com/..." />
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
</template>

<style scoped>
.properties-header {
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

.add-prop-btn {
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

.add-prop-btn:hover {
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

/* Properties Grid */
.properties-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.grid-item {
  animation: fadein var(--duration-base) var(--ease-spring);
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

.source-tag-overlay {
  position: absolute;
  top: 12px;
  left: 12px;
  background: rgba(15, 23, 42, 0.8);
  color: #fff;
  font-size: 9px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
}

.detail-info-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-title {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.4;
}

.detail-address {
  margin: 0;
  font-size: 12.5px;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 4px;
}

.detail-address svg {
  color: var(--color-text-muted);
}

.detail-specs {
  display: flex;
  gap: 12px;
  margin-top: 4px;
}

.spec-badge {
  background-color: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 11.5px;
  color: var(--color-text-secondary);
  font-weight: 600;
}

/* AI Valuation Panel */
.ai-valuation-section {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ai-val-header {
  padding: 10px 14px;
  border-bottom: 1px solid var(--color-divider);
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 700;
  font-size: 12px;
  color: var(--color-ai);
}

.val-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  padding: 16px 14px;
}

.val-col {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.val-col.border-left {
  border-left: 1px solid var(--color-divider);
  padding-left: 16px;
}

.val-col .lbl {
  font-size: 10px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.val-col .val {
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.val-analysis-bar {
  background-color: var(--color-success-bg);
  border-top: 1px solid var(--color-success-border);
  color: var(--color-success);
  padding: 8px 14px;
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  font-weight: 600;
}

.val-analysis-bar.overpriced {
  background-color: var(--color-danger-bg);
  border-top-color: var(--color-danger-border);
  color: var(--color-danger);
}

.val-analysis-bar .status-lbl {
  text-transform: uppercase;
  font-size: 9px;
  font-weight: 700;
}

.location-insights-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.section-title {
  margin: 0;
  font-size: 12.5px;
  font-weight: 600;
}

.insights-list {
  margin: 0;
  padding-left: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 12px;
  color: var(--color-text-secondary);
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

.prop-form {
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
