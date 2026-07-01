<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { Delete, Edit, Plus, Refresh, VideoPlay } from '@element-plus/icons-vue';
import { useCrawlerStore } from '@/stores/useCrawlerStore';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CrawlSource } from '@/types/crawler';

const crawlerStore = useCrawlerStore();
const propertyStore = usePropertyStore();
const toastStore = useToastStore();


const simulatorTimers: number[] = [];
const isSavingSource = ref(false);
const activeLiveTabSource = ref<string | null>(null);

const provinces = [
  {
    name: 'TP. Hồ Chí Minh',
    districts: ['Quận 1', 'Quận 2 (TP. Thủ Đức)', 'Quận 3', 'Quận 4', 'Quận 5', 'Quận 6', 'Quận 7', 'Quận 8', 'Quận 10', 'Quận 12', 'Bình Thạnh', 'Gò Vấp', 'Tân Bình', 'Bình Tân', 'Phú Nhuận', 'Huyện Bình Chánh', 'Huyện Củ Chi', 'Huyện Nhà Bè']
  },
  {
    name: 'TP. Hà Nội',
    districts: ['Quận Hoàn Kiếm', 'Quận Ba Đình', 'Quận Cầu Giấy', 'Quận Đống Đa', 'Quận Thanh Xuân', 'Quận Hoàng Mai', 'Quận Hà Đông', 'Long Biên', 'Nam Từ Liêm']
  },
  {
    name: 'Đồng Nai',
    districts: ['TP. Biên Hòa', 'Nhơn Trạch', 'Long Thành', 'Trảng Bom', 'Thống Nhất', 'Vĩnh Cửu', 'Định Quán', 'Xuân Lộc', 'Cẩm Mỹ']
  },
  {
    name: 'Bình Dương',
    districts: ['TP. Thủ Dầu Một', 'Thuận An', 'Dĩ An', 'Bến Cát', 'Bàu Bàng', 'Phú Giáo', 'Tân Uyên']
  },
  {
    name: 'Bình Phước',
    districts: ['TP. Đồng Xoài', 'Đồng Phú', 'Bù Đăng', 'Hớn Quản', 'Chơn Thành']
  },
  {
    name: 'Long An',
    districts: ['TP. Tân An', 'Bến Lức', 'Đức Hòa', 'Cần Đước', 'Cần Giuộc']
  },
  {
    name: 'Bà Rịa - Vũng Tàu',
    districts: ['TP. Vũng Tàu', 'TP. Bà Rịa', 'Châu Đức', 'Long Điền', 'Đất Đỏ', 'Xuyên Mộc']
  },
  {
    name: 'TP. Đà Nẵng',
    districts: ['Quận Hải Châu', 'Quận Thanh Khê', 'Quận Sơn Trà', 'Quận Ngũ Hành Sơn', 'Quận Liên Chiểu', 'Hòa Vang']
  },
  {
    name: 'TP. Cần Thơ',
    districts: ['Quận Ninh Kiều', 'Quận Bình Thủy', 'Quận Cái Răng', 'Huyện Phong Điền']
  }
];

const selectedProvince = ref('Đồng Nai');
const selectedArea = ref('Vĩnh Cửu');
const isCustomArea = ref(false);
const customAreaText = ref('');
const selectedCategory = ref('Nhà đất bán');
const selectedPropertyType = ref('Đất nền');
const enableAiFilter = ref(true);
const currentCrawlTab = ref<'property' | 'lead'>('property');
const useLocationFilter = ref(true);
const aiPromptText = ref('');

const showModal = ref(false);
const editingSource = ref<CrawlSource | null>(null);
const formSource = ref({
  name: '',
  baseUrl: '',
  isActive: true,
  successRate: 95,
  listingsToday: 0
});

const availableDistricts = computed(() => {
  const province = provinces.find(p => p.name === selectedProvince.value);
  return province ? province.districts : [];
});

const finalArea = computed(() => (
  isCustomArea.value ? (customAreaText.value.trim() || 'Khu vực chung') : selectedArea.value
));

const activeCount = computed(() => crawlerStore.sources.filter((s: CrawlSource) => s.isActive).length);
const totalListingsToday = computed(() => crawlerStore.sources.reduce((sum: number, s: CrawlSource) => sum + s.listingsToday, 0));
const avgSuccessRate = computed(() => {
  if (crawlerStore.sources.length === 0) return 0;
  const sum = crawlerStore.sources.reduce((total: number, s: CrawlSource) => total + s.successRate, 0);
  return Math.round(sum / crawlerStore.sources.length);
});

// Live session tracking
interface LiveSession {
  sourceId: string;
  sourceName: string;
  sourceUrl: string;
  targetArea: string;
  targetProvince: string;
  propertyType: string;
  category: string;
  startedAt: Date;
  progress: number;
  currentUrl: string;
  status: 'running' | 'success' | 'error';
  message: string;
}
const liveSessions = ref<LiveSession[]>([]);

function getLiveSession(sourceId: string): LiveSession | undefined {
  return liveSessions.value.find(s => s.sourceId === sourceId);
}

function buildCrawlUrl(baseUrl: string, area: string): string {
  const isLead = currentCrawlTab.value === 'lead';
  const hasLoc = isLead ? useLocationFilter.value : true;
  
  if (!hasLoc) {
    if (baseUrl.includes('chotot.com')) return `${baseUrl.replace(/\/$/, '')}/mua-ban-bat-dong-san${isLead ? '?f=c' : ''}`;
    if (baseUrl.includes('batdongsan.com.vn')) return `${baseUrl.replace(/\/$/, '')}/${isLead ? 'nha-dat-can-mua' : 'ban-nha-dat'}`;
    return baseUrl;
  }
  
  const encodedArea = encodeURIComponent(area);
  if (baseUrl.includes('chotot.com')) {
    return `${baseUrl.replace(/\/$/, '')}/mua-ban-bat-dong-san?q=${encodedArea}${isLead ? '&f=c' : ''}`;
  }
  if (baseUrl.includes('batdongsan.com.vn')) {
    const slug = area.toLowerCase().replace(/\s+/g, '-').normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/đ/g, "d");
    return `${baseUrl.replace(/\/$/, '')}/${isLead ? 'nha-dat-can-mua' : 'ban-nha-dat'}-${slug}`;
  }
  return `${baseUrl.replace(/\/$/, '')}?q=${encodedArea}`;
}

onMounted(() => {
  crawlerStore.fetchSources();
  propertyStore.fetchProperties();
});

function handleProvinceChange() {
  selectedArea.value = availableDistricts.value[0] ?? '';
}

function scheduleStep(callback: () => void, delay: number) {
  const timer = window.setTimeout(callback, delay);
  simulatorTimers.push(timer);
}

function formatLastRun(value: string) {
  if (!value) return 'Chưa chạy';
  return new Date(value).toLocaleTimeString('vi-VN');
}

async function completeCrawlerRun(source: CrawlSource) {
  const session = getLiveSession(source.id);

  try {
    const crawlMode = currentCrawlTab.value === 'lead' ? 'Lead' : 'Property';
    const targetArea = (crawlMode === 'Lead' && !useLocationFilter.value) ? 'Toàn quốc' : finalArea.value;

    if (session) {
      session.currentUrl = buildCrawlUrl(source.baseUrl, finalArea.value);
      session.message = `Đang cào dữ liệu từ: ${session.currentUrl}`;
    }

    const result = await crawlerStore.runCrawler(
      source.id,
      finalArea.value,
      selectedProvince.value,
      selectedPropertyType.value,
      selectedCategory.value,
      enableAiFilter.value,
      crawlMode,
      crawlMode === 'Lead' ? aiPromptText.value : '',
      crawlMode === 'Lead' ? useLocationFilter.value : true
    );

    await crawlerStore.fetchSources();
    await propertyStore.fetchProperties();

    if (session) {
      session.status = 'success';
      session.progress = 100;
      session.message = crawlMode === 'Lead' 
        ? `✅ Hoàn tất! Đã quét và tìm được ${result.totalCreated} khách hàng phù hợp.`
        : `✅ Hoàn tất! Đã lưu tin mới vào kho BĐS: ${result.propertyTitle}`;
    }

    if (crawlMode === 'Lead') {
      crawlerStore.pushLog(source.name, 'success', `Đã quét và lưu ${result.totalCreated} Leads khách hàng vào CRM.`);
      toastStore.success('Hoàn tất cào khách hàng', `Đã tạo ${result.totalCreated} Leads mới từ AI Lead Hunter.`);
    } else {
      crawlerStore.pushLog(source.name, 'success', `Đã lưu 1 tin đăng vào DB: ${result.propertyTitle}`);
      toastStore.success('Hoàn tất cào', `Đã lưu tin mới vào Kho BĐS: ${result.propertyTitle}`);
    }
  } catch (err: any) {
    const message = err?.response?.data?.message || err?.message || 'Lỗi server';

    if (session) {
      session.status = 'error';
      session.message = `❌ Cào thất bại: ${message}`;
    }

    crawlerStore.pushLog(source.name, 'error', `Cào dữ liệu thất bại: ${message}`);
    toastStore.error('Lỗi cào dữ liệu', message);
  } finally {
    // Keep session visible for 8 seconds after completion
    scheduleStep(() => {
      const idx = liveSessions.value.findIndex(s => s.sourceId === source.id);
      if (idx >= 0) liveSessions.value.splice(idx, 1);
    }, 8000);
  }
}

function runSimulation(source: CrawlSource) {
  const sessionCheck = liveSessions.value.find(s => s.sourceId === source.id);
  if (sessionCheck && sessionCheck.status === 'running') {
    toastStore.warning('Đang chạy', `Tiến trình cào nguồn ${source.name} đang chạy.`);
    return;
  }

  const crawlMode = currentCrawlTab.value === 'lead' ? 'Lead' : 'Property';
  const targetArea = (crawlMode === 'Lead' && !useLocationFilter.value) ? 'Toàn quốc' : finalArea.value;
  const targetUrl = buildCrawlUrl(source.baseUrl, finalArea.value);

  // Create live session
  const session: LiveSession = {
    sourceId: source.id,
    sourceName: source.name,
    sourceUrl: source.baseUrl,
    targetArea: targetArea,
    targetProvince: (crawlMode === 'Lead' && !useLocationFilter.value) ? 'Toàn quốc' : selectedProvince.value,
    propertyType: selectedPropertyType.value,
    category: selectedCategory.value,
    startedAt: new Date(),
    progress: 0,
    currentUrl: targetUrl,
    status: 'running',
    message: `Khởi tạo phiên cào...`
  };
  
  // Clean old session if exists
  const oldIdx = liveSessions.value.findIndex(s => s.sourceId === source.id);
  if (oldIdx >= 0) liveSessions.value.splice(oldIdx, 1);

  liveSessions.value.push(session);
  activeLiveTabSource.value = source.id;

  crawlerStore.pushLog(source.name, 'info', `Bắt đầu phiên cào [${crawlMode.toUpperCase()}] cho ${source.name}...`);
  crawlerStore.pushLog(source.name, 'info', `URL nguồn: ${source.baseUrl}`);
  crawlerStore.pushLog(source.name, 'info', `URL mục tiêu: ${targetUrl}`);
  crawlerStore.pushLog(source.name, 'info', `Khu vực quét: ${targetArea}`);
  if (crawlMode === 'Lead' && aiPromptText.value) {
    crawlerStore.pushLog(source.name, 'info', `Prompt tìm khách: "${aiPromptText.value}"`);
  }

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 15;
    s.message = 'Kết nối proxy...';
    crawlerStore.pushLog(source.name, 'info', 'Đang giải quyết hàng đợi proxy... Sử dụng proxy Node-041 (IP: 14.226.12.85)');
  }, 800);

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 30;
    s.message = `Kết nối đến: ${source.baseUrl}`;
    crawlerStore.pushLog(source.name, 'info', `Đang kết nối đến ${source.baseUrl}...`);
  }, 1600);

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 50;
    s.currentUrl = targetUrl;
    s.message = `Đang cào: ${targetUrl}`;
    crawlerStore.pushLog(source.name, 'info', `Đang kết nối tải trang và xử lý chống chặn...`);
  }, 2500);

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 70;
    s.message = crawlMode === 'Lead' ? 'Phân tích HTML và trích xuất tin cần mua...' : 'Phân tích HTML và trích xuất dữ liệu BĐS...';
    crawlerStore.pushLog(source.name, 'info', `Phân tích HTML... Tìm thấy các tin đăng tại ${targetArea}`);
  }, 3500);

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 85;
    s.message = crawlMode === 'Lead' ? 'Đối chiếu Prompt AI & Lọc khách hàng...' : 'Lọc và chuẩn hóa dữ liệu...';
    crawlerStore.pushLog(source.name, 'info', crawlMode === 'Lead' ? 'Đang gửi thông tin sang Gemini AI để đối chiếu Prompt...' : 'Đang lọc và chuẩn hóa dữ liệu...');
  }, 4500);

  scheduleStep(() => {
    const s = getLiveSession(source.id);
    if (!s || s.status !== 'running') return;
    s.progress = 95;
    s.message = crawlMode === 'Lead' ? 'Tạo Lead khách hàng trong CRM...' : 'Lưu vào cơ sở dữ liệu...';
    completeCrawlerRun(source);
  }, 5500);
}

// Bulk Auto Crawl Trigger
const isBulkCrawling = ref(false);
async function runBulkCrawl() {
  const activeSources = crawlerStore.sources.filter(s => s.isActive);
  if (activeSources.length === 0) {
    toastStore.warning('Không có nguồn hoạt động', 'Vui lòng kích hoạt ít nhất một nguồn cào.');
    return;
  }

  isBulkCrawling.value = true;
  toastStore.info('Bắt đầu cào hàng loạt', `Đang tự động chạy cào trên ${activeSources.length} nguồn cùng lúc...`);

  // Run all active sources in parallel
  activeSources.forEach(source => {
    runSimulation(source);
  });

  isBulkCrawling.value = false;
}

onBeforeUnmount(() => {
  simulatorTimers.forEach(clearTimeout);
});

function openAddModal() {
  editingSource.value = null;
  formSource.value = { name: '', baseUrl: '', isActive: true, successRate: 95, listingsToday: 0 };
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

async function saveSource() {
  if (!formSource.value.name.trim() || !formSource.value.baseUrl.trim()) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng điền tên và URL nguồn cào.');
    return;
  }

  isSavingSource.value = true;
  try {
    if (editingSource.value) {
      await crawlerStore.updateSource({
        id: editingSource.value.id,
        name: formSource.value.name,
        baseUrl: formSource.value.baseUrl,
        isActive: formSource.value.isActive,
        successRate: formSource.value.successRate,
        listingsToday: formSource.value.listingsToday,
        lastRunAt: editingSource.value.lastRunAt
      });
      toastStore.success('Đã cập nhật', `Nguồn cào "${formSource.value.name}" đã được cập nhật.`);
    } else {
      await crawlerStore.addSource(formSource.value);
      toastStore.success('Đã tạo', `Nguồn cào "${formSource.value.name}" đã được tạo thành công.`);
    }
    showModal.value = false;
    await crawlerStore.fetchSources();
  } catch (err: any) {
    toastStore.error('Lỗi', err?.response?.data?.message || 'Không thể lưu nguồn cào.');
  } finally {
    isSavingSource.value = false;
  }
}

async function deleteSource(id: string) {
  if (!confirm('Bạn có chắc muốn xóa nguồn cào này không?')) return;
  try {
    await crawlerStore.deleteSource(id);
    toastStore.success('Đã xóa', 'Nguồn cào đã được xóa.');
    await crawlerStore.fetchSources();
  } catch (err: any) {
    toastStore.error('Lỗi', 'Không thể xóa nguồn cào.');
  }
}
</script>

<template>
  <div class="page crawler-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <h2>🕷️ Crawler Data — Thu thập tự động</h2>
        <p class="page-subtitle">Quản lý và điều phối các nguồn cào dữ liệu bất động sản đa trang.</p>
      </div>
      <button class="btn-primary" @click="openAddModal">
        <span>+ Thêm nguồn cào</span>
      </button>
    </div>

    <!-- Stats Row -->
    <div class="stats-row">
      <div class="stat-card glass-card">
        <span class="stat-icon">🟢</span>
        <div>
          <div class="stat-value">{{ activeCount }}</div>
          <div class="stat-label">Nguồn đang hoạt động</div>
        </div>
      </div>
      <div class="stat-card glass-card">
        <span class="stat-icon">📋</span>
        <div>
          <div class="stat-value">{{ totalListingsToday }}</div>
          <div class="stat-label">Tin cào hôm nay</div>
        </div>
      </div>
      <div class="stat-card glass-card">
        <span class="stat-icon">✅</span>
        <div>
          <div class="stat-value">{{ avgSuccessRate }}%</div>
          <div class="stat-label">Tỷ lệ thành công TB</div>
        </div>
      </div>
      <div class="stat-card glass-card">
        <span class="stat-icon">🏠</span>
        <div>
          <div class="stat-value">{{ propertyStore.items.length }}</div>
          <div class="stat-label">Tổng BĐS trong kho</div>
        </div>
      </div>
    </div>

    <!-- Tabs Navigation -->
    <div class="tabs-navigation glass-card" style="margin-bottom: 24px; display: flex; padding: 6px; gap: 8px; border-radius: 12px; background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.05);">
      <button
        type="button"
        class="nav-tab-btn"
        :style="{
          flex: 1,
          padding: '12px 20px',
          borderRadius: '8px',
          border: 'none',
          background: currentCrawlTab === 'property' ? 'var(--primary-color, #3498db)' : 'transparent',
          color: currentCrawlTab === 'property' ? '#fff' : '#aaa',
          fontWeight: '600',
          cursor: 'pointer',
          transition: 'all 0.3s ease'
        }"
        @click="currentCrawlTab = 'property'"
      >
        🏠 Cào Bất Động Sản (Tin đăng bán/cho thuê)
      </button>
      <button
        type="button"
        class="nav-tab-btn"
        :style="{
          flex: 1,
          padding: '12px 20px',
          borderRadius: '8px',
          border: 'none',
          background: currentCrawlTab === 'lead' ? 'var(--primary-color, #3498db)' : 'transparent',
          color: currentCrawlTab === 'lead' ? '#fff' : '#aaa',
          fontWeight: '600',
          cursor: 'pointer',
          transition: 'all 0.3s ease'
        }"
        @click="currentCrawlTab = 'lead'"
      >
        👤 Cào Khách Hàng (AI CRM Lead Hunter)
      </button>
    </div>

    <div class="crawler-layout">
      <!-- Left: Control Panel -->
      <div class="control-panel glass-card">
        <div class="panel-header">
          <h3>⚙️ {{ currentCrawlTab === 'lead' ? 'Thợ săn khách hàng AI' : 'Bộ điều khiển cào BĐS' }}</h3>
          <p class="subtitle">Cấu hình tham số trước khi chạy</p>
        </div>

        <!-- Optional Location filter toggle for Lead mode -->
        <div v-if="currentCrawlTab === 'lead'" class="toggle-row" style="margin-bottom: 15px; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 12px;">
          <div class="toggle-info">
            <strong>Sử dụng bộ lọc vị trí</strong>
            <span>Giới hạn cào theo địa bàn đã chọn</span>
          </div>
          <label class="switch">
            <input type="checkbox" v-model="useLocationFilter" />
            <span class="slider round"></span>
          </label>
        </div>

        <!-- Location Selection: Hidden in Lead mode if useLocationFilter is unchecked -->
        <template v-if="currentCrawlTab === 'property' || useLocationFilter">
          <div class="form-group">
            <label>Tỉnh / Thành phố</label>
            <select v-model="selectedProvince" @change="handleProvinceChange">
              <option v-for="p in provinces" :key="p.name" :value="p.name">{{ p.name }}</option>
            </select>
          </div>

          <div class="form-group">
            <label>Quận / Huyện / Khu vực</label>
            <div class="area-input-group">
              <select v-if="!isCustomArea" v-model="selectedArea" class="flex-1">
                <option v-for="d in availableDistricts" :key="d" :value="d">{{ d }}</option>
              </select>
              <input v-else v-model="customAreaText" type="text" placeholder="Nhập khu vực tùy chỉnh..." class="flex-1" />
              <button
                type="button"
                class="btn-toggle-custom"
                :class="{ active: isCustomArea }"
                @click="isCustomArea = !isCustomArea"
                title="Nhập khu vực tùy chỉnh"
              >✏️</button>
            </div>
          </div>
        </template>

        <!-- Prompt Textarea for Lead Mode -->
        <div v-if="currentCrawlTab === 'lead'" class="form-group">
          <label>Nhu cầu khách cần tìm (Prompt đối chiếu AI)</label>
          <textarea
            v-model="aiPromptText"
            rows="3"
            placeholder="Ví dụ: Tìm khách mua đất diện tích lớn hơn 1000m2 giá dưới 2 tỷ ở Vĩnh Cửu..."
            style="width: 100%; border: 1px solid rgba(255,255,255,0.1); background: rgba(0,0,0,0.2); border-radius: 8px; padding: 10px; color: #fff; font-family: inherit; font-size: 13px; resize: vertical;"
          ></textarea>
        </div>

        <template v-if="currentCrawlTab === 'property' || useLocationFilter">
          <div class="form-group">
            <label>Loại BĐS</label>
            <select v-model="selectedPropertyType">
              <option>Đất nền</option>
              <option>Nhà phố</option>
              <option>Căn hộ</option>
              <option>Villa / Biệt thự</option>
              <option>Nhà xưởng</option>
              <option>Văn phòng</option>
            </select>
          </div>

          <div class="form-group">
            <label>Phân mục</label>
            <select v-model="selectedCategory">
              <option>Nhà đất bán</option>
              <option>Nhà đất cho thuê</option>
              <option>Bất động sản dự án</option>
            </select>
          </div>
        </template>

        <!-- AI Filter toggle (property mode only) -->
        <div v-if="currentCrawlTab === 'property'" class="toggle-row">
          <div class="toggle-info">
            <strong>AI Filter thông minh</strong>
            <span>Tự động loại bỏ tin rác, trùng lặp</span>
          </div>
          <label class="switch">
            <input type="checkbox" v-model="enableAiFilter" />
            <span class="slider round"></span>
          </label>
        </div>

        <!-- Live Target Display -->
        <div class="target-display">
          <div class="target-label">Mục tiêu cào hiện tại:</div>
          <div class="target-value">
            <span class="badge-area">📍 {{ (currentCrawlTab === 'lead' && !useLocationFilter) ? 'Toàn quốc' : `${finalArea}, ${selectedProvince}` }}</span>
            <span class="badge-type">🏠 {{ selectedPropertyType }}</span>
          </div>
        </div>

        <!-- Browser profile lock safety guidance tip -->
        <div class="profile-tip glass-card" style="margin-top: 15px; padding: 12px; font-size: 11.5px; line-height: 1.4; background: rgba(52, 152, 219, 0.1); border: 1px solid rgba(52, 152, 219, 0.25); border-radius: 8px; color: #d0e7ff;">
          <strong>💡 Mẹo đăng nhập tự động:</strong> Máy cào sử dụng một Profile Chrome biệt lập có tên <em>User Data RealSync</em> để tránh bị khóa file. Khi trình duyệt cào mở lên lần đầu, bạn hãy đăng nhập tài khoản cào của mình vào đó **1 lần duy nhất**, các lần sau máy cào sẽ tự động duy trì phiên đăng nhập này!
        </div>
              
        <!-- Bulk Crawl Button -->
        <div style="margin-top: 20px; border-top: 1px solid var(--color-divider); padding-top: 16px;">
          <button 
            type="button" 
            class="save-btn glow-yellow" 
            style="width: 100%; display: flex; align-items: center; justify-content: center; gap: 8px; background: var(--color-yellow); color: var(--color-yellow-text); border: none; font-weight: 700; height: 42px; border-radius: 8px; cursor: pointer; transition: all var(--duration-fast);"
            :disabled="liveSessions.some(s => s.status === 'running')"
            @click="runBulkCrawl"
          >
            <el-icon><VideoPlay /></el-icon>
            <span>Chạy tự động cào hàng loạt</span>
          </button>
        </div></div>

      <!-- Right: Sources + Live Progress -->
      <div class="sources-panel">
        <!-- Live Sessions Panel -->
        <div v-if="liveSessions.length > 0" class="live-sessions-panel glass-card">
          <div class="live-header">
            <span class="live-dot"></span>
            <h4>Tiến trình cào đang chạy</h4>
          </div>

          <div class="live-tabs">
            <button
              v-for="session in liveSessions"
              :key="session.sourceId"
              class="live-tab-btn"
              :class="{
                active: activeLiveTabSource === session.sourceId,
                error: session.status === 'error',
                success: session.status === 'success'
              }"
              @click="activeLiveTabSource = session.sourceId"
            >
              <span class="tab-dot" :class="session.status"></span>
              {{ session.sourceName }}
            </button>
          </div>

          <div v-for="session in liveSessions" :key="session.sourceId + '-body'">
            <div v-show="activeLiveTabSource === session.sourceId" class="live-session-body">
              <div class="session-info-grid">
                <div class="session-info-item">
                  <span class="info-label">Nguồn URL:</span>
                  <a :href="session.sourceUrl" target="_blank" class="info-url">{{ session.sourceUrl }}</a>
                </div>
                <div class="session-info-item">
                  <span class="info-label">URL đang cào:</span>
                  <a :href="session.currentUrl" target="_blank" class="info-url active-url">{{ session.currentUrl }}</a>
                </div>
                <div class="session-info-item">
                  <span class="info-label">Khu vực:</span>
                  <span class="info-value">{{ session.targetArea }}, {{ session.targetProvince }}</span>
                </div>
                <div class="session-info-item">
                  <span class="info-label">Loại BĐS:</span>
                  <span class="info-value">{{ session.propertyType }} — {{ session.category }}</span>
                </div>
                <div class="session-info-item">
                  <span class="info-label">Bắt đầu lúc:</span>
                  <span class="info-value">{{ session.startedAt.toLocaleTimeString('vi-VN') }}</span>
                </div>
              </div>

              <div class="progress-container">
                <div class="progress-bar-wrapper">
                  <div
                    class="progress-bar-fill"
                    :class="{ error: session.status === 'error', success: session.status === 'success' }"
                    :style="{ width: session.progress + '%' }"
                  ></div>
                </div>
                <span class="progress-pct">{{ session.progress }}%</span>
              </div>

              <div class="session-message" :class="session.status">
                {{ session.message }}
              </div>
            </div>
          </div>
        </div>

        <!-- Sources List -->
        <div class="sources-list">
          <div
            v-for="source in crawlerStore.sources"
            :key="source.id"
            class="source-card glass-card"
            :class="{
              'source-active': liveSessions.some(s => s.sourceId === source.id && s.status === 'running'),
              'source-inactive': !source.isActive
            }"
          >
            <div class="source-header">
              <div class="source-meta">
                <div class="source-title-row">
                  <span class="source-status-dot" :class="source.isActive ? 'active' : 'inactive'"></span>
                  <h4>{{ source.name }}</h4>
                </div>
                <a :href="source.baseUrl" target="_blank" class="source-url">{{ source.baseUrl }}</a>
              </div>
              <div class="source-actions">
                <button class="icon-btn" title="Chỉnh sửa" @click="openEditModal(source)">
                  <el-icon><Edit /></el-icon>
                </button>
                <button class="icon-btn danger" title="Xóa nguồn" @click="deleteSource(source.id)">
                  <el-icon><Delete /></el-icon>
                </button>
              </div>
            </div>

            <div class="source-stats">
              <div class="stat-mini">
                <span class="stat-mini-label">Tin hôm nay</span>
                <span class="stat-mini-value">{{ source.listingsToday }}</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Tỷ lệ thành công</span>
                <span class="stat-mini-value" :class="source.successRate < 80 ? 'danger' : 'success'">
                  {{ source.successRate }}%
                </span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Lần cuối</span>
                <span class="stat-mini-value">{{ formatLastRun(source.lastRunAt) }}</span>
              </div>
            </div>

            <!-- Live Progress for this source -->
            <div v-if="liveSessions.some(s => s.sourceId === source.id && s.status === 'running')" class="source-live-progress">
              <div class="progress-bar-wrapper">
                <div class="progress-bar-fill running" :style="{ width: (liveSessions.find(s => s.sourceId === source.id)?.progress || 0) + '%' }"></div>
              </div>
              <span class="progress-pct">{{ liveSessions.find(s => s.sourceId === source.id)?.progress || 0 }}%</span>
            </div>

            <button
              class="run-btn"
              :class="{ running: liveSessions.some(s => s.sourceId === source.id && s.status === 'running') }"
              :disabled="!source.isActive || liveSessions.some(s => s.sourceId === source.id && s.status === 'running')"
              @click="runSimulation(source)"
            >
              <el-icon v-if="!liveSessions.some(s => s.sourceId === source.id && s.status === 'running')"><VideoPlay /></el-icon>
              <span class="spinning-icon" v-else>↻</span>
              {{ liveSessions.some(s => s.sourceId === source.id && s.status === 'running') ? `Đang cào ${finalArea}...` : `Cào ${finalArea}` }}
            </button>
          </div>

          <div v-if="crawlerStore.sources.length === 0 && !crawlerStore.loading" class="empty-state">
            <div class="empty-icon">🕷️</div>
            <h4>Chưa có nguồn cào nào</h4>
            <p>Thêm nguồn cào đầu tiên để bắt đầu thu thập dữ liệu BĐS tự động.</p>
            <button class="btn-primary" @click="openAddModal">+ Thêm nguồn cào</button>
          </div>
        </div>

        <!-- Activity Log -->
        <div v-if="crawlerStore.logs.length > 0" class="log-panel glass-card">
          <div class="log-header">
            <h4>📜 Nhật ký hoạt động</h4>
            <button class="btn-clear-log" @click="crawlerStore.logs = []">Xóa log</button>
          </div>
          <div class="log-list">
            <div
              v-for="(log, i) in [...crawlerStore.logs].reverse()"
              :key="i"
              class="log-item"
              :class="log.type"
            >
              <span class="log-time">{{ new Date(log.timestamp).toLocaleTimeString('vi-VN') }}</span>
              <span class="log-source">[{{ log.source }}]</span>
              <span class="log-msg">{{ log.message }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Source Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-content glass-card">
        <h3>{{ editingSource ? '✏️ Chỉnh sửa nguồn cào' : '➕ Thêm nguồn cào mới' }}</h3>

        <div class="form-group">
          <label>Tên nguồn</label>
          <input v-model="formSource.name" type="text" placeholder="Ví dụ: ChoTot - BĐS Đồng Nai" />
        </div>
        <div class="form-group">
          <label>URL nguồn (Base URL)</label>
          <input v-model="formSource.baseUrl" type="url" placeholder="https://www.chotot.com/mua-ban-bat-dong-san" />
          <span class="field-hint">URL trang danh sách BĐS. Hệ thống sẽ tự thêm tham số khu vực khi cào.</span>
        </div>
        <div class="toggle-row">
          <div class="toggle-info">
            <strong>Kích hoạt nguồn</strong>
            <span>Nguồn đang hoạt động sẽ được lập lịch cào tự động</span>
          </div>
          <label class="switch">
            <input type="checkbox" v-model="formSource.isActive" />
            <span class="slider round"></span>
          </label>
        </div>

        <div class="modal-actions">
          <button class="btn-cancel" @click="showModal = false">Hủy</button>
          <button class="btn-primary" :disabled="isSavingSource" @click="saveSource">
            {{ isSavingSource ? 'Đang lưu...' : 'Lưu nguồn' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.crawler-page {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.page-header h2 {
  margin: 0 0 4px 0;
  font-size: 20px;
}

.page-subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--color-text-secondary);
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
}

.stat-icon {
  font-size: 28px;
}

.stat-value {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.stat-label {
  font-size: 11px;
  color: var(--color-text-secondary);
  margin-top: 2px;
}

.crawler-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 24px;
  align-items: start;
}

.control-panel {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  border-radius: 12px;
  position: sticky;
  top: 20px;
}

.panel-header h3 {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.subtitle {
  margin: 0;
  font-size: 11px;
  color: var(--color-text-secondary);
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
  height: 36px;
  border: 1px solid var(--color-border);
  background: var(--color-canvas);
  border-radius: 8px;
  padding: 0 10px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

.field-hint {
  font-size: 10.5px;
  color: var(--color-text-secondary);
  margin-top: 2px;
}

.area-input-group {
  display: flex;
  gap: 8px;
}

.flex-1 {
  flex: 1;
}

.btn-toggle-custom {
  width: 36px;
  height: 36px;
  border: 1px solid var(--color-border);
  background: var(--color-canvas);
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.btn-toggle-custom.active {
  background: var(--color-yellow);
  border-color: var(--color-yellow);
}

.toggle-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 12px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface-glass);
}

.toggle-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.toggle-info strong {
  font-size: 12px;
  color: var(--color-text-primary);
}

.toggle-info span {
  font-size: 10.5px;
  color: var(--color-text-secondary);
}

.target-display {
  background: rgba(var(--color-yellow-rgb, 245, 158, 11), 0.08);
  border: 1px dashed var(--color-yellow);
  border-radius: 8px;
  padding: 10px 12px;
  margin-top: 4px;
}

.target-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  margin-bottom: 6px;
}

.target-value {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.badge-area,
.badge-type {
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 4px;
  background: rgba(245, 158, 11, 0.15);
  color: var(--color-yellow);
}

/* Sources Panel */
.sources-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* Live Sessions */
.live-sessions-panel {
  padding: 16px;
  border: 1px solid rgba(16, 185, 129, 0.3);
  border-radius: 12px;
  background: rgba(16, 185, 129, 0.04);
}

.live-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 12px;
}

.live-header h4 {
  margin: 0;
  font-size: 13px;
  color: #10b981;
}

.live-dot {
  width: 8px;
  height: 8px;
  background: #10b981;
  border-radius: 50%;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.5; transform: scale(0.8); }
}

.live-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 14px;
  border-bottom: 1px solid var(--color-divider);
  padding-bottom: 10px;
  flex-wrap: wrap;
}

.live-tab-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-canvas);
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  color: var(--color-text-secondary);
}

.live-tab-btn.active {
  background: var(--color-surface-hover);
  border-color: var(--color-border-strong);
  color: var(--color-text-primary);
}

.live-tab-btn.error { border-color: rgba(239, 68, 68, 0.4); color: #ef4444; }
.live-tab-btn.success { border-color: rgba(16, 185, 129, 0.4); color: #10b981; }

.tab-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--color-text-muted);
}

.tab-dot.running { background: #10b981; animation: pulse 1.5s infinite; }
.tab-dot.error { background: #ef4444; }
.tab-dot.success { background: #10b981; }

.live-session-body {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.session-info-grid {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.session-info-item {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  font-size: 12px;
}

.info-label {
  color: var(--color-text-muted);
  font-weight: 600;
  min-width: 110px;
  flex-shrink: 0;
}

.info-url {
  color: var(--color-text-secondary);
  text-decoration: none;
  word-break: break-all;
  font-family: monospace;
  font-size: 11px;
}

.info-url:hover { color: var(--color-yellow); text-decoration: underline; }
.info-url.active-url { color: #10b981; font-weight: 600; }

.info-value {
  color: var(--color-text-primary);
  font-weight: 500;
}

.progress-container {
  display: flex;
  align-items: center;
  gap: 10px;
}

.progress-bar-wrapper {
  flex: 1;
  height: 6px;
  background: var(--color-divider);
  border-radius: 99px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  border-radius: 99px;
  background: var(--color-yellow);
  transition: width 0.5s ease;
}

.progress-bar-fill.running { background: #10b981; }
.progress-bar-fill.error { background: #ef4444; }
.progress-bar-fill.success { background: #10b981; }

.progress-pct {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-secondary);
  min-width: 32px;
  text-align: right;
}

.session-message {
  font-size: 12px;
  padding: 8px 12px;
  border-radius: 6px;
  background: var(--color-surface-glass);
  color: var(--color-text-secondary);
  border-left: 3px solid var(--color-yellow);
}

.session-message.error { border-left-color: #ef4444; color: #ef4444; }
.session-message.success { border-left-color: #10b981; color: #10b981; }

/* Source Cards */
.sources-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.source-card {
  padding: 16px;
  border-radius: 12px;
  border: 1px solid var(--color-border);
  transition: all 0.2s;
}

.source-card:hover {
  border-color: var(--color-border-strong);
}

.source-card.source-active {
  border-color: rgba(16, 185, 129, 0.4);
  background: rgba(16, 185, 129, 0.03);
}

.source-card.source-inactive {
  opacity: 0.6;
}

.source-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.source-meta {
  flex: 1;
}

.source-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.source-title-row h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.source-status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.source-status-dot.active { background: #10b981; }
.source-status-dot.inactive { background: #6b7280; }

.source-url {
  font-size: 11.5px;
  color: var(--color-text-secondary);
  text-decoration: none;
  font-family: monospace;
  word-break: break-all;
}

.source-url:hover { color: var(--color-yellow); text-decoration: underline; }

.source-actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}

.icon-btn {
  width: 28px;
  height: 28px;
  border: 1px solid var(--color-border);
  background: var(--color-canvas);
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
}

.icon-btn:hover { background: var(--color-surface-hover); }
.icon-btn.danger:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

.source-stats {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
  padding: 10px;
  background: var(--color-surface-glass);
  border-radius: 8px;
}

.stat-mini {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.stat-mini-label {
  font-size: 10px;
  text-transform: uppercase;
  color: var(--color-text-muted);
  font-weight: 600;
}

.stat-mini-value {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.stat-mini-value.danger { color: #ef4444; }
.stat-mini-value.success { color: #10b981; }

.source-live-progress {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.run-btn {
  width: 100%;
  height: 36px;
  border: none;
  border-radius: 8px;
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12.5px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s;
}

.run-btn:hover { background: var(--color-yellow-hover); }

.run-btn.running {
  background: rgba(16, 185, 129, 0.15);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.3);
  cursor: not-allowed;
}

.run-btn:disabled:not(.running) {
  opacity: 0.5;
  cursor: not-allowed;
}

.spinning-icon {
  animation: spin 1.5s linear infinite;
  display: inline-block;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Log Panel */
.log-panel {
  padding: 16px;
  border-radius: 12px;
}

.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.log-header h4 {
  margin: 0;
  font-size: 13px;
}

.btn-clear-log {
  font-size: 11px;
  padding: 4px 10px;
  border: 1px solid var(--color-border);
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
  color: var(--color-text-secondary);
}

.log-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
  max-height: 200px;
  overflow-y: auto;
}

.log-item {
  display: flex;
  gap: 8px;
  font-size: 11.5px;
  padding: 4px 8px;
  border-radius: 4px;
  align-items: flex-start;
}

.log-item.info { color: var(--color-text-secondary); }
.log-item.success { color: #10b981; background: rgba(16, 185, 129, 0.05); }
.log-item.error { color: #ef4444; background: rgba(239, 68, 68, 0.05); }
.log-item.warning { color: #f59e0b; background: rgba(245, 158, 11, 0.05); }

.log-time { color: var(--color-text-muted); font-family: monospace; flex-shrink: 0; }
.log-source { color: var(--color-text-muted); font-weight: 600; flex-shrink: 0; }
.log-msg { color: inherit; word-break: break-word; }

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  text-align: center;
  color: var(--color-text-secondary);
  border: 1px dashed var(--color-border);
  border-radius: 12px;
}

.empty-icon { font-size: 48px; margin-bottom: 12px; }
.empty-state h4 { margin: 0 0 8px 0; font-size: 15px; color: var(--color-text-primary); }
.empty-state p { margin: 0 0 16px 0; font-size: 13px; }

/* Switch */
.switch {
  position: relative;
  display: inline-block;
  width: 34px;
  height: 20px;
  flex-shrink: 0;
}
.switch input { opacity: 0; width: 0; height: 0; }
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
  height: 14px; width: 14px;
  left: 3px; bottom: 3px;
  background-color: white;
  transition: .3s;
}
input:checked + .slider { background-color: var(--color-yellow); }
input:checked + .slider:before { transform: translateX(14px); }
.slider.round { border-radius: 34px; }
.slider.round:before { border-radius: 50%; }

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(3, 7, 18, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
}
.modal-content {
  width: 480px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: var(--elevation-floating);
  border-radius: 12px;
}
.modal-content h3 { margin: 0; font-size: 15px; }
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 8px;
}

/* Buttons */
.btn-primary {
  height: 36px;
  padding: 0 16px;
  border: none;
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12.5px;
  font-weight: 700;
  border-radius: 8px;
  cursor: pointer;
}
.btn-primary:hover { background: var(--color-yellow-hover); }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-cancel {
  height: 36px;
  padding: 0 16px;
  border: 1px solid var(--color-border);
  background: transparent;
  border-radius: 8px;
  font-size: 12px;
  cursor: pointer;
  color: var(--color-text-secondary);
}
</style>
