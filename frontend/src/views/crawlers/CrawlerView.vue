<script setup lang="ts">
import { computed, ref, onBeforeUnmount } from 'vue';
import { useCrawlerStore, type CrawlerLog } from '@/stores/useCrawlerStore';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CrawlSource } from '@/types/crawler';

const crawlerStore = useCrawlerStore();
const propertyStore = usePropertyStore();
const toastStore = useToastStore();

// --- Crawler Simulator State ---
const activeSimulatorId = ref<string | null>(null);
const simulatorProgress = ref(0);
let simulatorTimer: any = null;

// --- CRUD Modal State ---
const showModal = ref(false);
const editingSource = ref<CrawlSource | null>(null);
const formSource = ref({
  name: '',
  baseUrl: '',
  isActive: true,
  successRate: 95,
  listingsToday: 0
});

// Computed KPIs
const activeCount = computed(() => crawlerStore.sources.filter(s => s.isActive).length);
const totalListingsToday = computed(() => crawlerStore.sources.reduce((sum, s) => sum + s.listingsToday, 0));
const avgSuccessRate = computed(() => {
  if (crawlerStore.sources.length === 0) return 0;
  const sum = crawlerStore.sources.reduce((total, s) => total + s.successRate, 0);
  return Math.round(sum / crawlerStore.sources.length);
});

// Simulation Runner
function runSimulation(source: CrawlSource) {
  if (activeSimulatorId.value) {
    toastStore.warning('Đang có tiến trình cào', 'Vui lòng chờ tiến trình hiện tại hoàn thành.');
    return;
  }

  activeSimulatorId.value = source.id;
  simulatorProgress.value = 0;
  
  crawlerStore.pushLog(source.name, 'info', `Bắt đầu phiên chạy thử nghiệm cho ${source.name}...`);
  
  // Phase 1 (1s)
  setTimeout(() => {
    if (activeSimulatorId.value !== source.id) return;
    simulatorProgress.value = 25;
    crawlerStore.pushLog(source.name, 'info', `Đang giải quyết hàng đợi proxy... Sử dụng proxy Node-041 (IP: 14.226.12.85)`);
  }, 1000);

  // Phase 2 (2s)
  setTimeout(() => {
    if (activeSimulatorId.value !== source.id) return;
    simulatorProgress.value = 50;
    crawlerStore.pushLog(source.name, 'info', `Đang vượt cơ chế Cloudflare chống cào... Thành công.`);
  }, 2000);

  // Phase 3 (3s)
  setTimeout(() => {
    if (activeSimulatorId.value !== source.id) return;
    simulatorProgress.value = 75;
    crawlerStore.pushLog(source.name, 'success', `Đã phân tích 12 thẻ tin đăng mới. Trích xuất thành công dữ liệu thô.`);
  }, 3000);

  // Phase 4 (4.5s) - Complete & Insert
  setTimeout(() => {
    if (activeSimulatorId.value !== source.id) return;
    simulatorProgress.value = 100;
    
    // Increment scraped listings count
    const updated = { ...source, listingsToday: source.listingsToday + 1, lastRunAt: new Date().toISOString() };
    crawlerStore.updateSource(updated);

    // Create a new crawled property and add to propertyStore
    const randomId = Math.floor(Math.random() * 1000);
    const mockCrawledProperty = {
      id: `crawled-${randomId}`,
      title: `[Cào] Căn hộ mới phát hiện tại Quận 2 - ${source.name}`,
      address: 'Mai Chí Thọ, Quận 2, TP. Thủ Đức',
      area: 'TP. Thủ Đức',
      price: 6500000000 + (Math.random() * 2000000000),
      acreage: 78 + Math.floor(Math.random() * 20),
      bedrooms: 2,
      status: 'draft' as 'draft' | 'verified' | 'published',
      source: source.name.toLowerCase().includes('batdongsan') ? 'batdongsan.com.vn' : 'chotot.com',
      imageUrl: 'https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=900&q=80',
      aiScore: 82 + Math.floor(Math.random() * 15),
      createdAt: new Date().toISOString()
    };
    
    propertyStore.items.unshift(mockCrawledProperty);
    crawlerStore.pushLog(source.name, 'success', `Đã lưu 1 tin đăng vào kho nháp: ${mockCrawledProperty.title}`);
    
    toastStore.success('Hoàn tất cào', `Đã cào thành công 1 tin đăng mới từ ${source.name}. Xem trong Kho BĐS.`);
    
    activeSimulatorId.value = null;
  }, 4500);
}

// Toggle active status
function toggleActive(source: CrawlSource) {
  const updated = { ...source, isActive: !source.isActive };
  crawlerStore.updateSource(updated);
  crawlerStore.pushLog(source.name, updated.isActive ? 'info' : 'warning', `Máy cào đã ${updated.isActive ? 'KÍCH HOẠT' : 'TẠM DỪNG'} chạy tự động.`);
  toastStore.success(updated.isActive ? 'Đã kích hoạt' : 'Đã tạm dừng', `Thay đổi trạng thái ${source.name} thành công.`);
}

// Clear Logs
function clearLogs() {
  crawlerStore.logs = [];
  toastStore.success('Đã xóa logs', 'Bảng nhật ký hoạt động đã được làm sạch.');
}

// Edit/Add CRUD
function openAddModal() {
  editingSource.value = null;
  formSource.value = {
    name: '',
    baseUrl: '',
    isActive: true,
    successRate: 98,
    listingsToday: 0
  };
  showModal.value = true;
}

function openEditModal(source: CrawlSource) {
  editingSource.value = source;
  formSource.value = {
    name: source.name,
    baseUrl: source.baseUrl,
    isActive: source.isActive,
    successRate: source.successRate,
    listingsToday: source.listingsToday
  };
  showModal.value = true;
}

function saveSource() {
  if (!formSource.value.name || !formSource.value.baseUrl) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng điền tên nguồn cào và link trang chủ.');
    return;
  }

  if (editingSource.value) {
    const updated: CrawlSource = {
      ...editingSource.value,
      ...formSource.value
    };
    crawlerStore.updateSource(updated);
    toastStore.success('Thành công', `Cập nhật nguồn cào: ${formSource.value.name}`);
  } else {
    const created: CrawlSource = {
      id: `c-${Date.now()}`,
      ...formSource.value,
      lastRunAt: new Date().toISOString()
    };
    crawlerStore.addSource(created);
    toastStore.success('Thành công', `Đã tạo nguồn cào mới: ${formSource.value.name}`);
  }
  showModal.value = false;
}

function deleteSourceItem(id: string, name: string) {
  if (confirm(`Bạn muốn xóa nguồn cào ${name}?`)) {
    crawlerStore.deleteSource(id);
    toastStore.success('Đã xóa', `Đã xóa nguồn cào ${name}`);
  }
}

onBeforeUnmount(() => {
  if (simulatorTimer) clearInterval(simulatorTimer);
});
</script>

<template>
  <div class="page">
    <!-- Header KPIs Row -->
    <div class="kpis-row">
      <div class="kpi-card glass-card">
        <span class="lbl">Máy cào hoạt động</span>
        <strong class="val numeric">{{ activeCount }} / {{ crawlerStore.sources.length }}</strong>
        <span class="status-indicator" :class="{ running: activeCount > 0 }">
          {{ activeCount > 0 ? 'Online' : 'Offline' }}
        </span>
      </div>

      <div class="kpi-card glass-card">
        <span class="lbl">Tin cào được hôm nay</span>
        <strong class="val numeric">{{ totalListingsToday }} tin</strong>
        <span class="sub">+14% so với hôm qua</span>
      </div>

      <div class="kpi-card glass-card">
        <span class="lbl">Tỷ lệ cào thành công</span>
        <strong class="val numeric">{{ avgSuccessRate }}%</strong>
        <span class="sub">Tỷ lệ kết nối Proxy</span>
      </div>

      <div class="kpi-card glass-card">
        <span class="lbl">Node Proxy Dự Phòng</span>
        <strong class="val numeric">49 / 50</strong>
        <span class="trend success">Độ trễ thấp</span>
      </div>
    </div>

    <!-- Main Section: Sources and Logs console side by side -->
    <div class="main-layout">
      <!-- Left side: Sources list -->
      <div class="sources-panel glass-card">
        <div class="panel-header">
          <div>
            <h3>Nguồn cào Dữ liệu (Sources)</h3>
            <p class="subtitle">Cấu hình các máy tìm kiếm quét dữ liệu từ các website Batdongsan, Chotot để đẩy vào kho.</p>
          </div>
          <button class="add-btn glow-yellow" @click="openAddModal">
            + Thêm nguồn
          </button>
        </div>

        <div class="sources-grid">
          <div 
            v-for="s in crawlerStore.sources" 
            :key="s.id" 
            class="source-card glass-card"
          >
            <div class="card-header">
              <div>
                <h4 class="source-name">{{ s.name }}</h4>
                <a :href="s.baseUrl" target="_blank" class="source-url">{{ s.baseUrl }}</a>
              </div>
              
              <!-- Toggle Switch -->
              <label class="switch">
                <input 
                  type="checkbox" 
                  :checked="s.isActive" 
                  @change="toggleActive(s)" 
                />
                <span class="slider round"></span>
              </label>
            </div>

            <!-- Stats grid -->
            <div class="card-stats">
              <div class="stat-box">
                <span class="lbl">Đã cào hôm nay</span>
                <span class="val numeric">{{ s.listingsToday }} tin</span>
              </div>
              <div class="stat-box border-left">
                <span class="lbl">Tỷ lệ thành công</span>
                <span class="val numeric">{{ s.successRate }}%</span>
              </div>
            </div>

            <!-- Last run -->
            <div class="last-run">
              Lần quét cuối: <span class="numeric">{{ new Date(s.lastRunAt).toLocaleTimeString('vi-VN') }}</span>
            </div>

            <!-- Simulator progress bar inside card if active -->
            <div v-if="activeSimulatorId === s.id" class="simulation-progress-section">
              <div class="progress-info">
                <span>Đang quét thử...</span>
                <span>{{ simulatorProgress }}%</span>
              </div>
              <div class="progress-bar-container">
                <div class="progress-bar-fill" :style="{ width: `${simulatorProgress}%` }"></div>
              </div>
            </div>

            <!-- Actions buttons footer -->
            <div class="card-actions">
              <button 
                class="sim-run-btn" 
                :disabled="activeSimulatorId !== null"
                @click="runSimulation(s)"
              >
                ⚡ Chạy thử
              </button>
              
              <div class="sub-actions">
                <button class="icon-btn" @click="openEditModal(s)" title="Cấu hình">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
                  </svg>
                </button>
                <button class="icon-btn danger" @click="deleteSourceItem(s.id, s.name)" title="Xóa">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right side: Log console (Terminal mockup) -->
      <div class="terminal-panel glass-card">
        <div class="panel-header">
          <div>
            <h3>Nhật ký máy cào (Crawler Logs)</h3>
            <p class="subtitle">Bảng hiển thị các tiến trình cào tin và kiểm soát proxy thời gian thực.</p>
          </div>
          <button class="clear-btn" @click="clearLogs">
            Xóa nhật ký
          </button>
        </div>

        <!-- Terminal Console body -->
        <div class="terminal-body">
          <div 
            v-for="log in crawlerStore.logs" 
            :key="log.id" 
            class="log-row"
          >
            <span class="log-time numeric">[{{ new Date(log.timestamp).toLocaleTimeString() }}]</span>
            <span class="log-source">[{{ log.source }}]</span>
            <span class="log-msg" :class="log.type">{{ log.message }}</span>
          </div>

          <div v-if="crawlerStore.logs.length === 0" class="empty-logs">
            Chưa có dòng nhật ký hoạt động nào. Hãy chạy thử một nguồn cào ở bên trái.
          </div>
        </div>
      </div>
    </div>

    <!-- ADD/EDIT MODAL -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-content glass-card">
        <h3>{{ editingSource ? 'Cập nhật Nguồn cào' : 'Thêm Nguồn cào mới' }}</h3>

        <form class="modal-form" @submit.prevent="saveSource">
          <div class="form-group">
            <label>Tên Nguồn cào (Ví dụ: Batdongsan, Chotot...)</label>
            <input v-model="formSource.name" type="text" placeholder="Nhập tên nguồn cào..." required />
          </div>

          <div class="form-group">
            <label>Đường dẫn trang chủ (Base URL)</label>
            <input v-model="formSource.baseUrl" type="url" placeholder="https://..." required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Tỷ lệ thành công mặc định (%)</label>
              <input v-model.number="formSource.successRate" type="number" placeholder="95" required />
            </div>
            <div class="form-group">
              <label>Kích hoạt mặc định</label>
              <select v-model="formSource.isActive">
                <option :value="true">Có</option>
                <option :value="false">Không</option>
              </select>
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="showModal = false">Hủy</button>
            <button type="submit" class="btn-submit glow-yellow">Lưu nguồn cào</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.kpis-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.kpi-card {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  position: relative;
}

.kpi-card .lbl {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.kpi-card .val {
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.kpi-card .sub {
  font-size: 11px;
  color: var(--color-text-muted);
}

.kpi-card .status-indicator {
  position: absolute;
  top: 16px;
  right: 16px;
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
  color: #fff;
  background-color: var(--color-danger);
}

.kpi-card .status-indicator.running {
  background-color: var(--color-success);
}

.kpi-card .trend.success {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-success);
}

/* Main workspace layout split */
.main-layout {
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  gap: 24px;
  align-items: start;
}

.sources-panel,
.terminal-panel {
  display: flex;
  flex-direction: column;
  padding: 24px;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.panel-header h3 {
  margin: 0 0 4px 0;
  font-size: 15px;
  font-weight: 700;
}

.panel-header .subtitle {
  margin: 0;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.add-btn {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12px;
  font-weight: 600;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
  white-space: nowrap;
  flex-shrink: 0;
}

.add-btn:hover {
  background-color: var(--color-yellow-hover);
}

/* Sources grid */
.sources-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

.source-card {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.source-name {
  font-size: 14px;
  font-weight: 700;
  margin: 0 0 2px 0;
  color: var(--color-text-primary);
}

.source-url {
  font-size: 11px;
  color: var(--color-text-muted);
  text-decoration: none;
}

.source-url:hover {
  color: var(--color-text-secondary);
}

/* Switch styling */
.switch {
  position: relative;
  display: inline-block;
  width: 34px;
  height: 20px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: var(--color-divider);
  transition: .3s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 14px;
  width: 14px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .3s;
}

input:checked + .slider {
  background-color: var(--color-yellow);
}

input:checked + .slider:before {
  transform: translateX(14px);
}

.slider.round {
  border-radius: 34px;
}

.slider.round:before {
  border-radius: 50%;
}

/* Stats */
.card-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 10px;
  border-radius: 8px;
}

.stat-box {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.stat-box.border-left {
  border-left: 1px solid var(--color-divider);
  padding-left: 16px;
}

.stat-box .lbl {
  font-size: 9px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.stat-box .val {
  font-size: 12.5px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.last-run {
  font-size: 11px;
  color: var(--color-text-muted);
}

/* Sim progress */
.simulation-progress-section {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding-top: 6px;
  border-top: 1px dashed var(--color-divider);
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
  transition: width 0.3s ease;
}

/* Card Actions */
.card-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 4px;
  padding-top: 10px;
  border-top: 1px solid var(--color-divider);
}

.sim-run-btn {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 11px;
  font-weight: 600;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.sim-run-btn:hover {
  background-color: var(--color-yellow-hover);
}

.sim-run-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.sub-actions {
  display: flex;
  gap: 8px;
}

.icon-btn {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.icon-btn:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.icon-btn.danger:hover {
  background-color: var(--color-danger-bg);
  border-color: var(--color-danger-border);
  color: var(--color-danger);
}

/* Terminal logs panel */
.clear-btn {
  background: transparent;
  border: 1px solid var(--color-border);
  font-size: 11.5px;
  color: var(--color-text-secondary);
  padding: 4px 10px;
  border-radius: 4px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.clear-btn:hover {
  background-color: var(--color-surface-hover);
}

.terminal-body {
  background-color: #030712;
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  padding: 16px;
  height: 380px;
  overflow-y: auto;
  font-family: var(--font-mono);
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.log-row {
  font-size: 11px;
  line-height: 1.4;
  display: flex;
  gap: 8px;
}

.log-time {
  color: #6b7280;
  flex-shrink: 0;
}

.log-source {
  color: #10b981;
  flex-shrink: 0;
}

.log-msg {
  color: #d1d5db;
}

.log-msg.info { color: #3b82f6; }
.log-msg.success { color: #10b981; }
.log-msg.warning { color: #f59e0b; }
.log-msg.error { color: #ef4444; }

.empty-logs {
  font-size: 11.5px;
  color: #6b7280;
  text-align: center;
  margin: auto 0;
}

/* Modals */
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
}

.modal-content h3 {
  margin: 0;
  font-size: 15px;
}

.modal-form {
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
  height: 38px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
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
</style>
